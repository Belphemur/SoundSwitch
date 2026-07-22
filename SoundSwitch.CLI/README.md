# SoundSwitch CLI

A command-line interface for controlling SoundSwitch.

## Commands

### Switch Audio Device
Switch between recording or playback devices:
```shell
SoundSwitch.CLI.exe switch --type Recording
SoundSwitch.CLI.exe switch --type Playback
```

### Microphone Control
Manage microphone mute state:
```shell
SoundSwitch.CLI.exe mute              # Show current mute state
SoundSwitch.CLI.exe mute --state true # Mute the microphone
SoundSwitch.CLI.exe mute -s false     # Unmute the microphone
SoundSwitch.CLI.exe mute --toggle     # Toggle mute state
```

### Profile Management
List all available profiles:
```shell
SoundSwitch.CLI.exe profile --list
```

Trigger a specific profile:
```shell
SoundSwitch.CLI.exe profile --name "Profile Name"
```

### Settings
Open SoundSwitch settings:
```shell
SoundSwitch.CLI.exe settings
```

### Status
Show active profile and current default audio devices:
```shell
SoundSwitch.CLI.exe status              # Human-readable table output
SoundSwitch.CLI.exe status --json       # Machine-readable JSON output
```

The JSON output includes: `activeProfile`, `playbackDevice`, `recordingDevice`, `playbackCommunicationDevice`, `recordingCommunicationDevice`. On failure, `status --json` prints a JSON object with an `error` field and exits with code 1.

**Note:** "Active Profile" shows the last profile explicitly triggered by a user action or trigger. It may not reflect changes made outside SoundSwitch (e.g., via Windows sound settings).

## Examples

1. Switch to next playback device:
```shell
SoundSwitch.CLI.exe switch --type Playback
```

2. Switch to next recording device:
```shell
SoundSwitch.CLI.exe switch --type Recording
```

3. List all available profiles:
```shell
SoundSwitch.CLI.exe profile --list
```

4. Trigger a specific profile:
```shell
SoundSwitch.CLI.exe profile --name "Headphones + Mic"
```

5. Open settings window:
```shell
SoundSwitch.CLI.exe settings
```

6. Mute/Unmute microphone:
```shell
SoundSwitch.CLI.exe mute -t          # Toggle mute
SoundSwitch.CLI.exe mute -s true     # Mute
SoundSwitch.CLI.exe mute --state false # Unmute
```

7. Show current audio status:
```shell
SoundSwitch.CLI.exe status
SoundSwitch.CLI.exe status --json
```

## Error Handling

The CLI provides clear error messages when:
- SoundSwitch is not running
- Profile names are invalid
- Connection issues occur
- Invalid commands or options are provided
- No default microphone is set (for mute commands)

All commands support the `--help` flag for additional information.
