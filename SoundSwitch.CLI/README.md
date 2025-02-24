# SoundSwitch CLI

A command-line interface for controlling SoundSwitch.

## Commands

### Switch Audio Device
Switch between recording or playback devices:
```shell
soundswitch switch --type Recording
soundswitch switch --type Playback
```

### Microphone Control
Manage microphone mute state:
```shell
soundswitch mute              # Show current mute state
soundswitch mute --state true # Mute the microphone
soundswitch mute -s false     # Unmute the microphone
soundswitch mute --toggle     # Toggle mute state
```

### Profile Management
List all available profiles:
```shell
soundswitch profile --list
```

Trigger a specific profile:
```shell
soundswitch profile --name "Profile Name"
```

### Settings
Open SoundSwitch settings:
```shell
soundswitch settings
```

## Examples

1. Switch to next playback device:
```shell
soundswitch switch --type Playback
```

2. Switch to next recording device:
```shell
soundswitch switch --type Recording
```

3. List all available profiles:
```shell
soundswitch profile --list
```

4. Trigger a specific profile:
```shell
soundswitch profile --name "Headphones + Mic"
```

5. Open settings window:
```shell
soundswitch settings
```

6. Mute/Unmute microphone:
```shell
soundswitch mute -t          # Toggle mute
soundswitch mute -s true     # Mute
soundswitch mute --state false # Unmute
```

## Error Handling

The CLI provides clear error messages when:
- SoundSwitch is not running
- Profile names are invalid
- Connection issues occur
- Invalid commands or options are provided
- No default microphone is set (for mute commands)

All commands support the `--help` flag for additional information.