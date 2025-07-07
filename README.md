# MIDI Event Viewer

A Windows Forms application built with VB.NET that allows you to view and analyze MIDI files with an intuitive interface.

## Features

- **Drag and Drop Support**: Simply drag a MIDI file (`.mid` or `.midi`) into the application window to load it
- **Track Management**: View all tracks or individual tracks through the menu system
- **Event Display**: Detailed view of all MIDI events including:
  - Track number
  - Timestamp
  - Event type (Note On, Note Off, Control Change, Program Change, Meta Events)
  - Note names (C4, D#5, etc.)
  - Velocity values
  - Channel information
  - Event descriptions
- **Track Controls**: Each track has Mute (M) and Solo (S) buttons for audio control
- **Playback Controls**: Play and Pause buttons with progress tracking
- **Progress Bar**: Shows playback progress in real-time
- **Time Display**: Shows current time and total duration in milliseconds (ms)
- **NAudio Integration**: Uses NAudio library for MIDI file processing and playback

## How to Use

1. **Load a MIDI File**: 
   - Drag and drop a `.mid` or `.midi` file into the application window
   - The file will be automatically loaded and analyzed

2. **View Events**:
   - All MIDI events are displayed in the main ListView
   - Use the "Tracks" menu to view all tracks or filter by specific track

3. **Track Controls**:
   - **M (Mute)**: Click to mute/unmute a track (button turns red when muted)
   - **S (Solo)**: Click to solo/unsolo a track (button turns yellow when soloed)

4. **Playback**:
   - Click **Play** to start playback
   - Click **Pause** to pause playback
   - Progress bar shows current position
   - Time display shows current time / total time in milliseconds

## Technical Details

- Built with VB.NET (.NET 8.0) and Windows Forms
- Uses NAudio library for MIDI file processing
- Supports Microsoft GS Wavetable for audio output
- Real-time event processing and display
- Proper resource cleanup on application exit

## Requirements

- Windows OS
- .NET 8.0 Runtime
- Audio output device for MIDI playback
- Microsoft GS Wavetable Synth (usually pre-installed on Windows)

## Notes

- The application automatically detects available MIDI output devices
- If no MIDI output device is available, the application will still display events but won't play audio
- All notes are automatically turned off when stopping playback or closing the application