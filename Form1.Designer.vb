<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        menuStrip = New MenuStrip()
        trackMenu = New ToolStripMenuItem()
        playerPanel = New Panel()
        statusLabel = New Label()
        timeLabel = New Label()
        progressBar = New ProgressBar()
        pauseButton = New Button()
        playButton = New Button()
        mainPanel = New Panel()
        eventsListView = New ListView()
        TrackColumn = New ColumnHeader()
        TimeColumn = New ColumnHeader()
        EventTypeColumn = New ColumnHeader()
        NoteColumn = New ColumnHeader()
        VelocityColumn = New ColumnHeader()
        ChannelColumn = New ColumnHeader()
        DescriptionColumn = New ColumnHeader()
        splitter = New Splitter()
        trackPanel = New Panel()
        menuStrip.SuspendLayout()
        playerPanel.SuspendLayout()
        mainPanel.SuspendLayout()
        SuspendLayout()
        ' 
        ' menuStrip
        ' 
        menuStrip.ImageScalingSize = New Size(32, 32)
        menuStrip.Items.AddRange(New ToolStripItem() {trackMenu})
        menuStrip.Location = New Point(0, 0)
        menuStrip.Name = "menuStrip"
        menuStrip.Padding = New Padding(13, 5, 0, 5)
        menuStrip.Size = New Size(2167, 46)
        menuStrip.TabIndex = 0
        menuStrip.Text = "MenuStrip1"
        ' 
        ' trackMenu
        ' 
        trackMenu.Name = "trackMenu"
        trackMenu.Size = New Size(98, 36)
        trackMenu.Text = "Tracks"
        ' 
        ' playerPanel
        ' 
        playerPanel.BackColor = SystemColors.Control
        playerPanel.Controls.Add(statusLabel)
        playerPanel.Controls.Add(timeLabel)
        playerPanel.Controls.Add(progressBar)
        playerPanel.Controls.Add(pauseButton)
        playerPanel.Controls.Add(playButton)
        playerPanel.Dock = DockStyle.Bottom
        playerPanel.Location = New Point(0, 1575)
        playerPanel.Margin = New Padding(6, 7, 6, 7)
        playerPanel.Name = "playerPanel"
        playerPanel.Size = New Size(2167, 148)
        playerPanel.TabIndex = 1
        ' 
        ' statusLabel
        ' 
        statusLabel.Font = New Font("Segoe UI", 8.25F, FontStyle.Italic)
        statusLabel.ForeColor = Color.DarkGreen
        statusLabel.Location = New Point(1777, 49)
        statusLabel.Margin = New Padding(6, 0, 6, 0)
        statusLabel.Name = "statusLabel"
        statusLabel.Size = New Size(368, 62)
        statusLabel.TabIndex = 4
        statusLabel.Text = "Ready"
        statusLabel.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' timeLabel
        ' 
        timeLabel.Font = New Font("Consolas", 9F)
        timeLabel.Location = New Point(1430, 49)
        timeLabel.Margin = New Padding(6, 0, 6, 0)
        timeLabel.Name = "timeLabel"
        timeLabel.Size = New Size(325, 62)
        timeLabel.TabIndex = 3
        timeLabel.Text = "0ms / 0ms"
        timeLabel.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' progressBar
        ' 
        progressBar.Location = New Point(433, 42)
        progressBar.Margin = New Padding(6, 7, 6, 7)
        progressBar.Maximum = 10000
        progressBar.Name = "progressBar"
        progressBar.Size = New Size(975, 62)
        progressBar.TabIndex = 2
        ' 
        ' pauseButton
        ' 
        pauseButton.Enabled = False
        pauseButton.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        pauseButton.Location = New Point(217, 37)
        pauseButton.Margin = New Padding(6, 7, 6, 7)
        pauseButton.Name = "pauseButton"
        pauseButton.Size = New Size(173, 74)
        pauseButton.TabIndex = 1
        pauseButton.Text = "Stop"
        pauseButton.UseVisualStyleBackColor = True
        ' 
        ' playButton
        ' 
        playButton.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        playButton.Location = New Point(22, 37)
        playButton.Margin = New Padding(6, 7, 6, 7)
        playButton.Name = "playButton"
        playButton.Size = New Size(173, 74)
        playButton.TabIndex = 0
        playButton.Text = "Play"
        playButton.UseVisualStyleBackColor = True
        ' 
        ' mainPanel
        ' 
        mainPanel.Controls.Add(eventsListView)
        mainPanel.Controls.Add(splitter)
        mainPanel.Controls.Add(trackPanel)
        mainPanel.Dock = DockStyle.Fill
        mainPanel.Location = New Point(0, 46)
        mainPanel.Margin = New Padding(6, 7, 6, 7)
        mainPanel.Name = "mainPanel"
        mainPanel.Size = New Size(2167, 1529)
        mainPanel.TabIndex = 2
        ' 
        ' eventsListView
        ' 
        eventsListView.Columns.AddRange(New ColumnHeader() {TrackColumn, TimeColumn, EventTypeColumn, NoteColumn, VelocityColumn, ChannelColumn, DescriptionColumn})
        eventsListView.Dock = DockStyle.Fill
        eventsListView.FullRowSelect = True
        eventsListView.GridLines = True
        eventsListView.Location = New Point(439, 0)
        eventsListView.Margin = New Padding(6, 7, 6, 7)
        eventsListView.Name = "eventsListView"
        eventsListView.Size = New Size(1728, 1529)
        eventsListView.TabIndex = 2
        eventsListView.UseCompatibleStateImageBehavior = False
        eventsListView.View = View.Details
        ' 
        ' TrackColumn
        ' 
        TrackColumn.Text = "Track"
        TrackColumn.Width = 100
        ' 
        ' TimeColumn
        ' 
        TimeColumn.Text = "Time"
        TimeColumn.Width = 110
        ' 
        ' EventTypeColumn
        ' 
        EventTypeColumn.Text = "Event Type"
        EventTypeColumn.Width = 200
        ' 
        ' NoteColumn
        ' 
        NoteColumn.Text = "Note"
        NoteColumn.Width = 100
        ' 
        ' VelocityColumn
        ' 
        VelocityColumn.Text = "Velocity"
        VelocityColumn.Width = 100
        ' 
        ' ChannelColumn
        ' 
        ChannelColumn.Text = "Channel"
        ChannelColumn.Width = 100
        ' 
        ' DescriptionColumn
        ' 
        DescriptionColumn.Text = "Description"
        DescriptionColumn.Width = 500
        ' 
        ' splitter
        ' 
        splitter.BackColor = Color.DarkGray
        splitter.Location = New Point(433, 0)
        splitter.Margin = New Padding(6, 7, 6, 7)
        splitter.Name = "splitter"
        splitter.Size = New Size(6, 1529)
        splitter.TabIndex = 1
        splitter.TabStop = False
        ' 
        ' trackPanel
        ' 
        trackPanel.AutoScroll = True
        trackPanel.BackColor = Color.LightGray
        trackPanel.Dock = DockStyle.Left
        trackPanel.Location = New Point(0, 0)
        trackPanel.Margin = New Padding(6, 7, 6, 7)
        trackPanel.Name = "trackPanel"
        trackPanel.Size = New Size(433, 1529)
        trackPanel.TabIndex = 0
        ' 
        ' Form1
        ' 
        AllowDrop = True
        AutoScaleDimensions = New SizeF(13F, 32F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(2167, 1723)
        Controls.Add(mainPanel)
        Controls.Add(playerPanel)
        Controls.Add(menuStrip)
        MainMenuStrip = menuStrip
        Margin = New Padding(6, 7, 6, 7)
        MinimumSize = New Size(1703, 1373)
        Name = "Form1"
        StartPosition = FormStartPosition.CenterScreen
        Text = "MIDI Event Viewer"
        menuStrip.ResumeLayout(False)
        menuStrip.PerformLayout()
        playerPanel.ResumeLayout(False)
        mainPanel.ResumeLayout(False)
        ResumeLayout(False)
        PerformLayout()

    End Sub

    Friend WithEvents menuStrip As MenuStrip
    Friend WithEvents trackMenu As ToolStripMenuItem
    Friend WithEvents playerPanel As Panel
    Friend WithEvents playButton As Button
    Friend WithEvents pauseButton As Button
    Friend WithEvents progressBar As ProgressBar
    Friend WithEvents timeLabel As Label
    Friend WithEvents statusLabel As Label
    Friend WithEvents mainPanel As Panel
    Friend WithEvents trackPanel As Panel
    Friend WithEvents splitter As Splitter
    Friend WithEvents eventsListView As ListView
    Friend WithEvents TrackColumn As ColumnHeader
    Friend WithEvents TimeColumn As ColumnHeader
    Friend WithEvents EventTypeColumn As ColumnHeader
    Friend WithEvents NoteColumn As ColumnHeader
    Friend WithEvents VelocityColumn As ColumnHeader
    Friend WithEvents ChannelColumn As ColumnHeader
    Friend WithEvents DescriptionColumn As ColumnHeader

End Class