Imports System.IO
Imports NAudio.Midi
Imports NAudio.Wave

Public Class Form1
    Private midiFile As MidiFile
    Private midiOut As MidiOut
    Private currentTick As Long = 0
    Private totalTicks As Long = 0
    Private playbackTimer As Timer
    Private isPlaying As Boolean = False
    Private tracks As List(Of TrackDisplayInfo)
    Private allEvents As List(Of MidiEventInfo)
    Private startTime As DateTime
    Private ticksPerQuarterNote As Integer = 120
    Private tempo As Integer = 500000 ' Default tempo (120 BPM)
    Private lastPlayedTick As Long = 0

    Private Structure TrackDisplayInfo
        Public TrackNumber As Integer
        Public TrackName As String
        Public IsMuted As Boolean
        Public IsSolo As Boolean
        Public MuteButton As Button
        Public SoloButton As Button
        Public TrackLabel As Label
        Public Panel As Panel
    End Structure

    Private Structure MidiEventInfo
        Public Track As Integer
        Public AbsoluteTime As Long
        Public EventType As String
        Public Note As String
        Public Velocity As Integer
        Public Channel As Integer
        Public Description As String
        Public MidiEvent As MidiEvent
    End Structure

    Public Sub New()
        InitializeComponent()
        InitializeApplication()
    End Sub

    Private Sub InitializeApplication()
        ' Initialize collections
        tracks = New List(Of TrackDisplayInfo)
        allEvents = New List(Of MidiEventInfo)

        ' Setup event handlers
        SetupEventHandlers()
        SetupDragDrop()
        SetupTimer()

        ' Add resize event handlers for responsive layout
        AddHandler Me.Resize, AddressOf Form1_Resize
        AddHandler trackPanel.Resize, AddressOf TrackPanel_Resize

        ' Initialize MIDI output
        InitializeMidiOutput()
    End Sub

    Private Sub SetupEventHandlers()
        ' Drag and drop events
        AddHandler Me.DragEnter, AddressOf Form1_DragEnter
        AddHandler Me.DragDrop, AddressOf Form1_DragDrop

        ' Player control events
        AddHandler playButton.Click, AddressOf PlayButton_Click
        AddHandler pauseButton.Click, AddressOf PauseButton_Click

        ' Resize events
        AddHandler Me.Resize, AddressOf Form1_Resize
        AddHandler trackPanel.Resize, AddressOf TrackPanel_Resize
    End Sub

    Private Sub InitializeMidiOutput()
        Try
            If MidiOut.NumberOfDevices > 0 Then
                ' Try to find Microsoft GS Wavetable Synth first
                For i As Integer = 0 To MidiOut.NumberOfDevices - 1
                    Dim deviceInfo As MidiOutCapabilities = MidiOut.DeviceInfo(i)
                    If deviceInfo.ProductName.Contains("Microsoft GS Wavetable") OrElse
                       deviceInfo.ProductName.Contains("Windows MIDI") Then
                        midiOut = New MidiOut(i)

                        ' Reset all controllers for clean sound
                        ResetAllControllers()

                        UpdateStatus($"MIDI: {deviceInfo.ProductName}")
                        Return
                    End If
                Next

                ' If GS Wavetable not found, use the first available device
                midiOut = New MidiOut(0)

                ' Reset all controllers for clean sound
                ResetAllControllers()

                Dim firstDevice As MidiOutCapabilities = MidiOut.DeviceInfo(0)
                UpdateStatus($"MIDI: {firstDevice.ProductName}")
            Else
                UpdateStatus("No MIDI devices")
                MessageBox.Show("No MIDI output devices found. Audio playback will not be available.",
                              "MIDI Device Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        Catch ex As Exception
            UpdateStatus("MIDI Error")
            MessageBox.Show($"Error initializing MIDI output: {ex.Message}",
                          "MIDI Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Reset all MIDI controllers for cleaner sound
    Private Sub ResetAllControllers()
        If midiOut IsNot Nothing Then
            Try
                For channel As Integer = 0 To 15
                    ' Reset all controllers
                    Dim resetMsg As Integer = &HB0 Or channel
                    resetMsg = resetMsg Or (121 << 8) ' Controller 121 (Reset All Controllers)
                    resetMsg = resetMsg Or (0 << 16)
                    midiOut.Send(resetMsg)

                    ' Set main volume
                    Dim volumeMsg As Integer = &HB0 Or channel
                    volumeMsg = volumeMsg Or (7 << 8) ' Controller 7 (Volume)
                    volumeMsg = volumeMsg Or (100 << 16) ' Volume level 100
                    midiOut.Send(volumeMsg)

                    ' Set expression
                    Dim expressionMsg As Integer = &HB0 Or channel
                    expressionMsg = expressionMsg Or (11 << 8) ' Controller 11 (Expression)
                    expressionMsg = expressionMsg Or (127 << 16) ' Maximum expression
                    midiOut.Send(expressionMsg)
                Next
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine($"Error resetting controllers: {ex.Message}")
            End Try
        End If
    End Sub

    Private Sub SetupDragDrop()
        Me.AllowDrop = True
        AddHandler Me.DragEnter, AddressOf Form1_DragEnter
        AddHandler Me.DragDrop, AddressOf Form1_DragDrop
    End Sub

    Private Sub SetupTimer()
        playbackTimer = New Timer()
        playbackTimer.Interval = 10 ' Reduced from 50ms to 10ms for smoother playback
        AddHandler playbackTimer.Tick, AddressOf PlaybackTimer_Tick
    End Sub

    Private Sub Form1_DragEnter(sender As Object, e As DragEventArgs)
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            Dim files() As String = CType(e.Data.GetData(DataFormats.FileDrop), String())
            If files.Length > 0 AndAlso Path.GetExtension(files(0)).ToLower() = ".mid" OrElse Path.GetExtension(files(0)).ToLower() = ".midi" Then
                e.Effect = DragDropEffects.Copy
            End If
        End If
    End Sub

    Private Sub Form1_DragDrop(sender As Object, e As DragEventArgs)
        Dim files() As String = CType(e.Data.GetData(DataFormats.FileDrop), String())
        If files.Length > 0 Then
            LoadMidiFile(files(0))
        End If
    End Sub

    Private Sub LoadMidiFile(filePath As String)
        Try
            ' Stop current playback
            If isPlaying Then
                StopPlayback()
            End If

            UpdateStatus("Loading...")

            ' Load MIDI file
            midiFile = New MidiFile(filePath)
            totalTicks = GetTotalTicks(midiFile)
            ticksPerQuarterNote = midiFile.DeltaTicksPerQuarterNote

            ' Clear previous data
            tracks.Clear()
            allEvents.Clear()
            trackPanel.Controls.Clear()
            eventsListView.Items.Clear()

            ' Reset playback position
            currentTick = 0
            lastPlayedTick = 0
            progressBar.Value = 0

            ' Initialize drum channel (Channel 10/9)
            InitializeDrumChannel()

            ' Update menu and track display
            UpdateTrackMenu()
            LoadTrackData()
            DisplayAllEvents()

            ' Update title
            Me.Text = $"MIDI Event Viewer - {Path.GetFileName(filePath)}"

            ' Show file information
            Dim fileInfo As String = $"Tracks: {midiFile.Tracks}, Events: {allEvents.Count}, Duration: {TicksToTimeString(totalTicks)}"
            UpdateStatus($"Loaded - {fileInfo}")

        Catch ex As Exception
            UpdateStatus("Error")
            MessageBox.Show($"Error loading MIDI file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Initialize drum channel to ensure proper drum sound playback
    Private Sub InitializeDrumChannel()
        If midiOut IsNot Nothing Then
            Try
                ' Set channel 9 (0-based) / channel 10 (1-based) to use drum sounds
                ' Send program change to ensure it's set to drum kit
                Dim drumProgramMsg As Integer = &HC9 ' Program Change on channel 9 (0-based)
                drumProgramMsg = drumProgramMsg Or (0 << 8) ' Program 0 (Standard Drum Kit)
                midiOut.Send(drumProgramMsg)

                ' Set volume for drum channel
                Dim volumeMsg As Integer = &HB9 ' Control Change on channel 9 (0-based)
                volumeMsg = volumeMsg Or (7 << 8) ' Controller 7 (Volume)
                volumeMsg = volumeMsg Or (100 << 16) ' Volume level 100
                midiOut.Send(volumeMsg)

                System.Diagnostics.Debug.WriteLine("Drum channel initialized")
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine($"Error initializing drum channel: {ex.Message}")
            End Try
        End If
    End Sub

    Private Function GetTotalTicks(midiFile As MidiFile) As Long
        Dim maxTicks As Long = 0
        For Each track In midiFile.Events
            Dim currentTicks As Long = 0
            For Each midiEvent In track
                currentTicks += CLng(midiEvent.DeltaTime)
            Next
            maxTicks = Math.Max(maxTicks, currentTicks)
        Next
        Return maxTicks
    End Function

    Private Sub UpdateTrackMenu()
        trackMenu.DropDownItems.Clear()

        ' Add "All Tracks" option
        Dim allTracksItem As New ToolStripMenuItem("All Tracks")
        AddHandler allTracksItem.Click, AddressOf ShowAllTracks
        trackMenu.DropDownItems.Add(allTracksItem)

        trackMenu.DropDownItems.Add(New ToolStripSeparator())

        ' Add individual track options
        For i As Integer = 0 To midiFile.Tracks - 1
            Dim trackItem As New ToolStripMenuItem($"Track {i + 1}")
            Dim trackIndex As Integer = i
            AddHandler trackItem.Click, Sub() ShowTrack(trackIndex)
            trackMenu.DropDownItems.Add(trackItem)
        Next
    End Sub

    Private Sub LoadTrackData()
        ' Get the current track panel width for responsive sizing
        Dim availableWidth As Integer = Math.Max(trackPanel.Width - 20, 180) ' Minimum 180px width
        Dim panelWidth As Integer = availableWidth - 10 ' Leave margin for scrollbar
        Dim yPos As Integer = 5

        For trackIndex As Integer = 0 To midiFile.Tracks - 1
            Dim track As IList(Of MidiEvent) = midiFile.Events(trackIndex)

            ' Create track display info
            Dim trackInfo As New TrackDisplayInfo()
            trackInfo.TrackNumber = trackIndex
            trackInfo.TrackName = $"Track {trackIndex + 1}"
            trackInfo.IsMuted = False
            trackInfo.IsSolo = False

            ' Create track panel with increased height
            trackInfo.Panel = New Panel()
            trackInfo.Panel.Size = New Size(panelWidth, 110) ' Increased from 70 to 90
            trackInfo.Panel.Location = New Point(8, yPos)
            trackInfo.Panel.BackColor = Color.White
            trackInfo.Panel.BorderStyle = BorderStyle.FixedSingle
            trackInfo.Panel.Margin = New Padding(3)
            trackInfo.Panel.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right

            ' Create track label with responsive sizing
            trackInfo.TrackLabel = New Label()
            trackInfo.TrackLabel.Text = trackInfo.TrackName
            trackInfo.TrackLabel.Location = New Point(5, 5)
            trackInfo.TrackLabel.Size = New Size(panelWidth - 10, 36)
            trackInfo.TrackLabel.Font = New Font("Segoe UI", 9, FontStyle.Bold)
            trackInfo.TrackLabel.ForeColor = Color.DarkBlue
            trackInfo.TrackLabel.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
            trackInfo.Panel.Controls.Add(trackInfo.TrackLabel)

            ' Calculate button sizes based on available width
            Dim buttonWidth As Integer = Math.Max(30, Math.Min(40, (panelWidth - 30) \ 3))
            Dim buttonSpacing As Integer = Math.Max(5, (panelWidth - (buttonWidth * 2) - 20) \ 3)

            ' Create mute button with responsive sizing
            trackInfo.MuteButton = New Button()
            trackInfo.MuteButton.Text = "M"
            trackInfo.MuteButton.Size = New Size(buttonWidth, 40)
            trackInfo.MuteButton.Location = New Point(8, 55)
            trackInfo.MuteButton.BackColor = Color.LightGray
            trackInfo.MuteButton.FlatStyle = FlatStyle.Flat
            trackInfo.MuteButton.FlatAppearance.BorderColor = Color.Gray
            trackInfo.MuteButton.FlatAppearance.BorderSize = 1
            trackInfo.MuteButton.Font = New Font("Segoe UI", 9, FontStyle.Bold)
            trackInfo.MuteButton.Tag = trackIndex
            trackInfo.MuteButton.UseVisualStyleBackColor = False
            trackInfo.MuteButton.Cursor = Cursors.Hand
            trackInfo.MuteButton.Anchor = AnchorStyles.Top Or AnchorStyles.Left
            AddHandler trackInfo.MuteButton.Click, AddressOf MuteButton_Click
            trackInfo.Panel.Controls.Add(trackInfo.MuteButton)

            ' Create solo button with responsive sizing
            trackInfo.SoloButton = New Button()
            trackInfo.SoloButton.Text = "S"
            trackInfo.SoloButton.Size = New Size(buttonWidth, 40)
            trackInfo.SoloButton.Location = New Point(8 + buttonWidth + buttonSpacing, 55)
            trackInfo.SoloButton.BackColor = Color.LightGray
            trackInfo.SoloButton.FlatStyle = FlatStyle.Flat
            trackInfo.SoloButton.FlatAppearance.BorderColor = Color.Gray
            trackInfo.SoloButton.FlatAppearance.BorderSize = 1
            trackInfo.SoloButton.Font = New Font("Segoe UI", 9, FontStyle.Bold)
            trackInfo.SoloButton.Tag = trackIndex
            trackInfo.SoloButton.UseVisualStyleBackColor = False
            trackInfo.SoloButton.Cursor = Cursors.Hand
            trackInfo.SoloButton.Anchor = AnchorStyles.Top Or AnchorStyles.Left
            AddHandler trackInfo.SoloButton.Click, AddressOf SoloButton_Click
            trackInfo.Panel.Controls.Add(trackInfo.SoloButton)

            ' Add track type indicator with responsive positioning
            Dim trackTypeLabel As New Label()
            trackTypeLabel.Text = GetTrackType(track)
            Dim typeX As Integer = Math.Max(8 + (buttonWidth * 2) + (buttonSpacing * 2), panelWidth - 75)
            trackTypeLabel.Location = New Point(typeX, 40)
            trackTypeLabel.Size = New Size(panelWidth - typeX - 5, 36)
            trackTypeLabel.Font = New Font("Segoe UI", 8, FontStyle.Italic)
            trackTypeLabel.ForeColor = Color.DarkGreen
            trackTypeLabel.TextAlign = ContentAlignment.MiddleLeft
            trackTypeLabel.Anchor = AnchorStyles.Top Or AnchorStyles.Right
            trackInfo.Panel.Controls.Add(trackTypeLabel)

            trackPanel.Controls.Add(trackInfo.Panel)
            tracks.Add(trackInfo)

            yPos += 110 ' Increased spacing from 75 to 95

            ' Process events for this track (keeping existing code)
            Dim absoluteTime As Long = 0
            For Each midiEvent As MidiEvent In track
                absoluteTime += CLng(midiEvent.DeltaTime)

                Dim eventInfo As New MidiEventInfo()
                eventInfo.Track = trackIndex
                eventInfo.AbsoluteTime = absoluteTime
                eventInfo.MidiEvent = midiEvent

                ' Parse event details
                Select Case midiEvent.CommandCode
                    Case MidiCommandCode.NoteOn
                        Dim noteOn As NoteOnEvent = CType(midiEvent, NoteOnEvent)
                        eventInfo.EventType = "Note On"
                        eventInfo.Note = GetNoteName(noteOn.NoteNumber)
                        eventInfo.Velocity = noteOn.Velocity
                        eventInfo.Channel = noteOn.Channel
                        eventInfo.Description = $"Note {eventInfo.Note}, Velocity {eventInfo.Velocity}"

                    Case MidiCommandCode.NoteOff
                        Dim noteOff As NoteEvent = CType(midiEvent, NoteEvent)
                        eventInfo.EventType = "Note Off"
                        eventInfo.Note = GetNoteName(noteOff.NoteNumber)
                        eventInfo.Velocity = noteOff.Velocity
                        eventInfo.Channel = noteOff.Channel
                        eventInfo.Description = $"Note {eventInfo.Note}, Velocity {eventInfo.Velocity}"

                    Case MidiCommandCode.ControlChange
                        Dim cc As ControlChangeEvent = CType(midiEvent, ControlChangeEvent)
                        eventInfo.EventType = "Control Change"
                        eventInfo.Channel = cc.Channel
                        eventInfo.Description = $"Controller {cc.Controller}, Value {cc.ControllerValue}"

                    Case MidiCommandCode.PatchChange
                        Dim pc As PatchChangeEvent = CType(midiEvent, PatchChangeEvent)
                        eventInfo.EventType = "Program Change"
                        eventInfo.Channel = pc.Channel
                        eventInfo.Description = $"Program {pc.Patch}"

                    Case MidiCommandCode.MetaEvent
                        Dim meta As MetaEvent = CType(midiEvent, MetaEvent)
                        eventInfo.EventType = "Meta Event"
                        eventInfo.Description = meta.ToString()

                        ' Check for tempo changes
                        If TypeOf meta Is TempoEvent Then
                            Dim tempoEvent As TempoEvent = CType(meta, TempoEvent)
                            tempo = tempoEvent.MicrosecondsPerQuarterNote
                        End If

                    Case Else
                        eventInfo.EventType = midiEvent.CommandCode.ToString()
                        eventInfo.Description = midiEvent.ToString()
                End Select

                allEvents.Add(eventInfo)
            Next
        Next
    End Sub

    Private Function GetTrackType(track As IList(Of MidiEvent)) As String
        Dim hasNotes As Boolean = track.Any(Function(e) e.CommandCode = MidiCommandCode.NoteOn OrElse e.CommandCode = MidiCommandCode.NoteOff)
        Dim hasProgram As Boolean = track.Any(Function(e) e.CommandCode = MidiCommandCode.PatchChange)
        Dim hasControl As Boolean = track.Any(Function(e) e.CommandCode = MidiCommandCode.ControlChange)

        ' Check for drum channel (Channel 10 = 1-based, Channel 9 = 0-based)
        Dim hasDrums As Boolean = track.Any(Function(e)
                                                If e.CommandCode = MidiCommandCode.NoteOn Then
                                                    Dim noteOn As NoteOnEvent = CType(e, NoteOnEvent)
                                                    Return noteOn.Channel = 10
                                                ElseIf e.CommandCode = MidiCommandCode.NoteOff Then
                                                    Dim noteOff As NoteEvent = CType(e, NoteEvent)
                                                    Return noteOff.Channel = 10
                                                End If
                                                Return False
                                            End Function)

        If hasDrums Then
            Return "Drums"
        ElseIf hasNotes Then
            Return "Melody"
        ElseIf hasProgram Then
            Return "Program"
        ElseIf hasControl Then
            Return "Control"
        Else
            Return "Meta"
        End If
    End Function

    Private Function GetNoteName(noteNumber As Integer) As String
        Dim noteNames() As String = {"C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"}
        Dim octave As Integer = (noteNumber \ 12) - 1
        Dim note As String = noteNames(noteNumber Mod 12)
        Return $"{note}{octave}"
    End Function

    Private Sub DisplayAllEvents()
        eventsListView.Items.Clear()
        eventsListView.BeginUpdate()

        Try
            For Each eventInfo As MidiEventInfo In allEvents.OrderBy(Function(e) e.AbsoluteTime)
                Dim item As New ListViewItem(eventInfo.Track.ToString())
                item.SubItems.Add(TicksToTimeString(eventInfo.AbsoluteTime))
                item.SubItems.Add(eventInfo.EventType)
                item.SubItems.Add(If(String.IsNullOrEmpty(eventInfo.Note), "-", eventInfo.Note))
                item.SubItems.Add(If(eventInfo.Velocity = 0, "-", eventInfo.Velocity.ToString()))
                item.SubItems.Add(If(eventInfo.Channel = 0, "-", eventInfo.Channel.ToString()))
                item.SubItems.Add(eventInfo.Description)
                item.Tag = eventInfo

                ' Color-code different event types
                Select Case eventInfo.EventType
                    Case "Note On"
                        item.BackColor = Color.LightGreen
                    Case "Note Off"
                        item.BackColor = Color.LightCoral
                    Case "Control Change"
                        item.BackColor = Color.LightBlue
                    Case "Program Change"
                        item.BackColor = Color.LightYellow
                    Case "Meta Event"
                        item.BackColor = Color.LightGray
                End Select

                eventsListView.Items.Add(item)
            Next
        Finally
            eventsListView.EndUpdate()
        End Try
    End Sub

    Private Sub ShowAllTracks()
        DisplayAllEvents()
    End Sub

    Private Sub ShowTrack(trackIndex As Integer)
        eventsListView.Items.Clear()
        eventsListView.BeginUpdate()

        Try
            For Each eventInfo As MidiEventInfo In allEvents.Where(Function(e) e.Track = trackIndex).OrderBy(Function(e) e.AbsoluteTime)
                Dim item As New ListViewItem(eventInfo.Track.ToString())
                item.SubItems.Add(TicksToTimeString(eventInfo.AbsoluteTime))
                item.SubItems.Add(eventInfo.EventType)
                item.SubItems.Add(If(String.IsNullOrEmpty(eventInfo.Note), "-", eventInfo.Note))
                item.SubItems.Add(If(eventInfo.Velocity = 0, "-", eventInfo.Velocity.ToString()))
                item.SubItems.Add(If(eventInfo.Channel = 0, "-", eventInfo.Channel.ToString()))
                item.SubItems.Add(eventInfo.Description)
                item.Tag = eventInfo

                ' Color-code different event types
                Select Case eventInfo.EventType
                    Case "Note On"
                        item.BackColor = Color.LightGreen
                    Case "Note Off"
                        item.BackColor = Color.LightCoral
                    Case "Control Change"
                        item.BackColor = Color.LightBlue
                    Case "Program Change"
                        item.BackColor = Color.LightYellow
                    Case "Meta Event"
                        item.BackColor = Color.LightGray
                End Select

                eventsListView.Items.Add(item)
            Next
        Finally
            eventsListView.EndUpdate()
        End Try
    End Sub

    Private Sub MuteButton_Click(sender As Object, e As EventArgs)
        Dim button As Button = CType(sender, Button)
        Dim trackIndex As Integer = CType(button.Tag, Integer)
        ToggleMute(trackIndex)
    End Sub

    Private Sub SoloButton_Click(sender As Object, e As EventArgs)
        Dim button As Button = CType(sender, Button)
        Dim trackIndex As Integer = CType(button.Tag, Integer)
        ToggleSolo(trackIndex)
    End Sub

    Private Sub ToggleMute(trackIndex As Integer)
        If trackIndex >= 0 AndAlso trackIndex < tracks.Count Then
            Dim trackInfo = tracks(trackIndex)
            trackInfo.IsMuted = Not trackInfo.IsMuted
            trackInfo.MuteButton.BackColor = If(trackInfo.IsMuted, Color.Red, Color.LightGray)
            trackInfo.MuteButton.ForeColor = If(trackInfo.IsMuted, Color.White, Color.Black)
            tracks(trackIndex) = trackInfo
        End If
    End Sub

    Private Sub ToggleSolo(trackIndex As Integer)
        If trackIndex >= 0 AndAlso trackIndex < tracks.Count Then
            Dim trackInfo = tracks(trackIndex)
            trackInfo.IsSolo = Not trackInfo.IsSolo
            trackInfo.SoloButton.BackColor = If(trackInfo.IsSolo, Color.Gold, Color.LightGray)
            trackInfo.SoloButton.ForeColor = If(trackInfo.IsSolo, Color.Black, Color.Black)
            tracks(trackIndex) = trackInfo
        End If
    End Sub

    Private Sub PlayButton_Click(sender As Object, e As EventArgs)
        If midiFile IsNot Nothing Then
            StartPlayback()
        End If
    End Sub

    Private Sub PauseButton_Click(sender As Object, e As EventArgs)
        If isPlaying Then
            PausePlayback()
        End If
    End Sub

    Private Sub StartPlayback()
        If midiFile IsNot Nothing Then
            isPlaying = True
            playButton.Enabled = False
            pauseButton.Enabled = True
            startTime = DateTime.Now
            lastPlayedTick = currentTick ' Start from current position
            playbackTimer.Start()
            UpdateStatus("Playing...")
        End If
    End Sub

    Private Sub PausePlayback()
        isPlaying = False
        playButton.Enabled = True
        pauseButton.Enabled = False
        playbackTimer.Stop()
        UpdateStatus("Paused")

        ' Send all notes off when pausing
        SendAllNotesOff()
    End Sub

    Private Sub StopPlayback()
        isPlaying = False
        playButton.Enabled = True
        pauseButton.Enabled = False
        playbackTimer.Stop()
        currentTick = 0
        lastPlayedTick = 0
        progressBar.Value = 0
        UpdateStatus("Stopped")

        ' Send all notes off
        SendAllNotesOff()
    End Sub

    Private Sub SendAllNotesOff()
        If midiOut IsNot Nothing Then
            Try
                For channel As Integer = 0 To 15
                    ' Send all notes off control change
                    Dim noteOffMsg As Integer = &HB0 Or channel ' Control change
                    noteOffMsg = noteOffMsg Or (123 << 8) ' Controller 123 (All notes off)
                    noteOffMsg = noteOffMsg Or (0 << 16) ' Value 0
                    midiOut.Send(noteOffMsg)

                    ' Also send all sound off for good measure
                    Dim soundOffMsg As Integer = &HB0 Or channel ' Control change
                    soundOffMsg = soundOffMsg Or (120 << 8) ' Controller 120 (All sound off)
                    soundOffMsg = soundOffMsg Or (0 << 16) ' Value 0
                    midiOut.Send(soundOffMsg)
                Next
            Catch ex As Exception
                ' Ignore errors when sending all notes off
                System.Diagnostics.Debug.WriteLine($"Error sending all notes off: {ex.Message}")
            End Try
        End If
    End Sub

    Private Sub PlaybackTimer_Tick(sender As Object, e As EventArgs)
        If Not isPlaying OrElse midiFile Is Nothing Then Return

        ' Calculate current position based on elapsed time
        Dim elapsedTime As TimeSpan = DateTime.Now - startTime
        Dim elapsedTicks As Long = CLng((elapsedTime.TotalMilliseconds * ticksPerQuarterNote * 1000) / tempo)
        currentTick = Math.Min(elapsedTicks, totalTicks)

        ' Process and send MIDI events that should be played at current time
        PlayMidiEvents(currentTick)

        ' Update progress bar with better precision
        If totalTicks > 0 Then
            Dim progressValue As Integer = CInt((currentTick * progressBar.Maximum) / totalTicks)
            progressBar.Value = Math.Min(progressValue, progressBar.Maximum)
        End If

        ' Update time display
        Dim currentTime As String = TicksToTimeString(currentTick)
        Dim totalTime As String = TicksToTimeString(totalTicks)
        timeLabel.Text = $"{currentTime} / {totalTime}"

        ' Update status
        UpdateStatus("Playing...")

        ' Check if playback is complete
        If currentTick >= totalTicks Then
            StopPlayback()
        End If
    End Sub

    Private Sub PlayMidiEvents(currentTick As Long)
        If midiOut Is Nothing Then Return

        ' Get events that should be played between lastPlayedTick and currentTick
        Dim eventsToPlay = allEvents.Where(Function(e) e.AbsoluteTime > lastPlayedTick AndAlso e.AbsoluteTime <= currentTick).OrderBy(Function(e) e.AbsoluteTime)

        For Each eventInfo As MidiEventInfo In eventsToPlay
            ' Check if track is muted or if solo is active
            If ShouldPlayEvent(eventInfo.Track) Then
                SendMidiEvent(eventInfo.MidiEvent)
            End If
        Next

        lastPlayedTick = currentTick
    End Sub

    Private Function ShouldPlayEvent(trackIndex As Integer) As Boolean
        If trackIndex < 0 OrElse trackIndex >= tracks.Count Then Return True

        Dim trackInfo = tracks(trackIndex)

        ' If track is muted, don't play
        If trackInfo.IsMuted Then Return False

        ' If any track is soloed, only play soloed tracks
        Dim hasSoloedTracks = tracks.Any(Function(t) t.IsSolo)
        If hasSoloedTracks Then
            Return trackInfo.IsSolo
        End If

        ' Otherwise, play the track
        Return True
    End Function

    Private Sub SendMidiEvent(midiEvent As MidiEvent)
        If midiOut Is Nothing Then Return

        Try
            Select Case midiEvent.CommandCode
                Case MidiCommandCode.NoteOn
                    Dim noteOn As NoteOnEvent = CType(midiEvent, NoteOnEvent)
                    If noteOn.Velocity > 0 Then ' Only send if velocity > 0
                        ' MIDI channels are 1-based in display but 0-based in protocol
                        ' Channel 9 (0-based) = Channel 10 (1-based) = Drum channel
                        Dim msg As Integer = &H90 Or (noteOn.Channel - 1) ' Note On with proper channel mapping
                        msg = msg Or (noteOn.NoteNumber << 8)
                        msg = msg Or (noteOn.Velocity << 16)
                        midiOut.Send(msg)

                        ' Debug output for drum channel
                        If noteOn.Channel - 1 = 9 Then ' Channel 10 (1-based) = Channel 9 (0-based) = Drums
                            System.Diagnostics.Debug.WriteLine($"Drum Note: {GetNoteName(noteOn.NoteNumber)}, Velocity: {noteOn.Velocity}, Channel: {noteOn.Channel}")
                        End If
                    End If

                Case MidiCommandCode.NoteOff
                    Dim noteOff As NoteEvent = CType(midiEvent, NoteEvent)
                    ' Proper channel mapping for Note Off
                    Dim msg As Integer = &H80 Or (noteOff.Channel - 1) ' Note Off with proper channel mapping
                    msg = msg Or (noteOff.NoteNumber << 8)
                    msg = msg Or (noteOff.Velocity << 16)
                    midiOut.Send(msg)

                Case MidiCommandCode.ControlChange
                    Dim cc As ControlChangeEvent = CType(midiEvent, ControlChangeEvent)
                    Dim msg As Integer = &HB0 Or (cc.Channel - 1) ' Control Change with proper channel mapping
                    msg = msg Or (CInt(cc.Controller) << 8)
                    msg = msg Or (cc.ControllerValue << 16)
                    midiOut.Send(msg)

                Case MidiCommandCode.PatchChange
                    Dim pc As PatchChangeEvent = CType(midiEvent, PatchChangeEvent)
                    Dim msg As Integer = &HC0 Or (pc.Channel - 1) ' Program Change with proper channel mapping
                    msg = msg Or (pc.Patch << 8)
                    midiOut.Send(msg)

                Case MidiCommandCode.PitchWheelChange
                    Dim pw As PitchWheelChangeEvent = CType(midiEvent, PitchWheelChangeEvent)
                    Dim msg As Integer = &HE0 Or (pw.Channel - 1) ' Pitch Wheel with proper channel mapping
                    Dim pitchValue As Integer = pw.Pitch + 8192 ' Convert to 14-bit value
                    msg = msg Or ((pitchValue And &H7F) << 8)
                    msg = msg Or (((pitchValue >> 7) And &H7F) << 16)
                    midiOut.Send(msg)

                Case MidiCommandCode.ChannelAfterTouch
                    Dim ca As ChannelAfterTouchEvent = CType(midiEvent, ChannelAfterTouchEvent)
                    Dim msg As Integer = &HD0 Or (ca.Channel - 1) ' Channel Aftertouch with proper channel mapping
                    msg = msg Or (ca.AfterTouchPressure << 8)
                    midiOut.Send(msg)

                    ' Skip events that are not directly playable or have complex handling
                Case MidiCommandCode.MetaEvent, MidiCommandCode.Sysex
                    ' Do nothing - these are not playable MIDI events

                Case Else
                    ' For any other MIDI command codes, try to send as raw data if possible
                    ' This handles other event types gracefully without crashing
            End Select

        Catch ex As Exception
            ' Ignore MIDI send errors to prevent crashes during playback
            System.Diagnostics.Debug.WriteLine($"MIDI send error: {ex.Message}")
        End Try
    End Sub

    Private Function TicksToTimeString(ticks As Long) As String
        Dim totalMilliseconds As Double = (ticks * tempo) / (ticksPerQuarterNote * 1000.0)
        Return $"{CInt(totalMilliseconds)}ms"
    End Function

    Private Sub UpdateStatus(message As String)
        If statusLabel IsNot Nothing Then
            statusLabel.Text = message
            Select Case message
                Case "Playing..."
                    statusLabel.ForeColor = Color.Blue
                Case "Paused"
                    statusLabel.ForeColor = Color.Orange
                Case "Ready"
                    statusLabel.ForeColor = Color.DarkGreen
                Case "Stopped"
                    statusLabel.ForeColor = Color.Red
                Case Else
                    statusLabel.ForeColor = Color.Black
            End Select
        End If
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        If midiOut IsNot Nothing Then
            midiOut.Close()
            midiOut.Dispose()
        End If

        If playbackTimer IsNot Nothing Then
            playbackTimer.Stop()
            playbackTimer.Dispose()
        End If

        MyBase.OnFormClosing(e)
    End Sub

    ' Add resize event handlers for responsive layout
    Private Sub Form1_Resize(sender As Object, e As EventArgs)
        ' Refresh track layout when form is resized
        If midiFile IsNot Nothing AndAlso tracks.Count > 0 Then
            RefreshTrackLayout()
        End If
    End Sub

    Private Sub TrackPanel_Resize(sender As Object, e As EventArgs)
        ' Refresh track layout when track panel is resized
        If midiFile IsNot Nothing AndAlso tracks.Count > 0 Then
            RefreshTrackLayout()
        End If
    End Sub

    ' Method to refresh track layout when window is resized
    Private Sub RefreshTrackLayout()
        If midiFile IsNot Nothing AndAlso tracks.Count > 0 Then
            ' Store current mute/solo states
            Dim muteStates As New Dictionary(Of Integer, Boolean)
            Dim soloStates As New Dictionary(Of Integer, Boolean)

            For i As Integer = 0 To tracks.Count - 1
                muteStates(i) = tracks(i).IsMuted
                soloStates(i) = tracks(i).IsSolo
            Next

            ' Clear track panel controls but preserve events
            trackPanel.Controls.Clear()
            tracks.Clear()

            ' Only reload track display, not events (to prevent duplication)
            RefreshTrackDisplayOnly()

            ' Restore mute/solo states
            For i As Integer = 0 To Math.Min(tracks.Count - 1, muteStates.Count - 1)
                If muteStates.ContainsKey(i) Then
                    Dim trackInfo = tracks(i)
                    trackInfo.IsMuted = muteStates(i)
                    trackInfo.IsSolo = soloStates(i)
                    tracks(i) = trackInfo

                    ' Update button appearances
                    tracks(i).MuteButton.BackColor = If(tracks(i).IsMuted, Color.Red, Color.LightGray)
                    tracks(i).MuteButton.ForeColor = If(tracks(i).IsMuted, Color.White, Color.Black)
                    tracks(i).SoloButton.BackColor = If(tracks(i).IsSolo, Color.Gold, Color.LightGray)
                    tracks(i).SoloButton.ForeColor = If(tracks(i).IsSolo, Color.Black, Color.Black)
                End If
            Next
        End If
    End Sub

    ' Method to refresh only track display without reprocessing events
    Private Sub RefreshTrackDisplayOnly()
        ' Get the current track panel width for responsive sizing
        Dim availableWidth As Integer = Math.Max(trackPanel.Width - 20, 180) ' Minimum 180px width
        Dim panelWidth As Integer = availableWidth - 10 ' Leave margin for scrollbar
        Dim yPos As Integer = 5

        For trackIndex As Integer = 0 To midiFile.Tracks - 1
            Dim track As IList(Of MidiEvent) = midiFile.Events(trackIndex)

            ' Create track display info
            Dim trackInfo As New TrackDisplayInfo()
            trackInfo.TrackNumber = trackIndex
            trackInfo.TrackName = $"Track {trackIndex + 1}"
            trackInfo.IsMuted = False
            trackInfo.IsSolo = False

            ' Create track panel with responsive sizing
            trackInfo.Panel = New Panel()
            trackInfo.Panel.Size = New Size(panelWidth, 110)
            trackInfo.Panel.Location = New Point(8, yPos)
            trackInfo.Panel.BackColor = Color.White
            trackInfo.Panel.BorderStyle = BorderStyle.FixedSingle
            trackInfo.Panel.Margin = New Padding(3)
            trackInfo.Panel.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right

            ' Create track label with responsive sizing
            trackInfo.TrackLabel = New Label()
            trackInfo.TrackLabel.Text = trackInfo.TrackName
            trackInfo.TrackLabel.Location = New Point(5, 5)
            trackInfo.TrackLabel.Size = New Size(panelWidth - 10, 36)
            trackInfo.TrackLabel.Font = New Font("Segoe UI", 9, FontStyle.Bold)
            trackInfo.TrackLabel.ForeColor = Color.DarkBlue
            trackInfo.TrackLabel.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
            trackInfo.Panel.Controls.Add(trackInfo.TrackLabel)

            ' Calculate button sizes based on available width
            Dim buttonWidth As Integer = Math.Max(30, Math.Min(40, (panelWidth - 30) \ 3))
            Dim buttonSpacing As Integer = Math.Max(5, (panelWidth - (buttonWidth * 2) - 20) \ 3)

            ' Create mute button with responsive sizing
            trackInfo.MuteButton = New Button()
            trackInfo.MuteButton.Text = "M"
            trackInfo.MuteButton.Size = New Size(buttonWidth, 40)
            trackInfo.MuteButton.Location = New Point(8, 55)
            trackInfo.MuteButton.BackColor = Color.LightGray
            trackInfo.MuteButton.FlatStyle = FlatStyle.Flat
            trackInfo.MuteButton.FlatAppearance.BorderColor = Color.Gray
            trackInfo.MuteButton.FlatAppearance.BorderSize = 1
            trackInfo.MuteButton.Font = New Font("Segoe UI", 9, FontStyle.Bold)
            trackInfo.MuteButton.Tag = trackIndex
            trackInfo.MuteButton.UseVisualStyleBackColor = False
            trackInfo.MuteButton.Cursor = Cursors.Hand
            trackInfo.MuteButton.Anchor = AnchorStyles.Top Or AnchorStyles.Left
            AddHandler trackInfo.MuteButton.Click, AddressOf MuteButton_Click
            trackInfo.Panel.Controls.Add(trackInfo.MuteButton)

            ' Create solo button with responsive sizing
            trackInfo.SoloButton = New Button()
            trackInfo.SoloButton.Text = "S"
            trackInfo.SoloButton.Size = New Size(buttonWidth, 40)
            trackInfo.SoloButton.Location = New Point(8 + buttonWidth + buttonSpacing, 55)
            trackInfo.SoloButton.BackColor = Color.LightGray
            trackInfo.SoloButton.FlatStyle = FlatStyle.Flat
            trackInfo.SoloButton.FlatAppearance.BorderColor = Color.Gray
            trackInfo.SoloButton.FlatAppearance.BorderSize = 1
            trackInfo.SoloButton.Font = New Font("Segoe UI", 9, FontStyle.Bold)
            trackInfo.SoloButton.Tag = trackIndex
            trackInfo.SoloButton.UseVisualStyleBackColor = False
            trackInfo.SoloButton.Cursor = Cursors.Hand
            trackInfo.SoloButton.Anchor = AnchorStyles.Top Or AnchorStyles.Left
            AddHandler trackInfo.SoloButton.Click, AddressOf SoloButton_Click
            trackInfo.Panel.Controls.Add(trackInfo.SoloButton)

            ' Add track type indicator with responsive positioning
            Dim trackTypeLabel As New Label()
            trackTypeLabel.Text = GetTrackType(track)
            Dim typeX As Integer = Math.Max(8 + (buttonWidth * 2) + (buttonSpacing * 2), panelWidth - 75)
            trackTypeLabel.Location = New Point(typeX, 40)
            trackTypeLabel.Size = New Size(panelWidth - typeX - 5, 36)
            trackTypeLabel.Font = New Font("Segoe UI", 8, FontStyle.Italic)
            trackTypeLabel.ForeColor = Color.DarkGreen
            trackTypeLabel.TextAlign = ContentAlignment.MiddleLeft
            trackTypeLabel.Anchor = AnchorStyles.Top Or AnchorStyles.Right
            trackInfo.Panel.Controls.Add(trackTypeLabel)

            trackPanel.Controls.Add(trackInfo.Panel)
            tracks.Add(trackInfo)

            yPos += 110
        Next
    End Sub

End Class
