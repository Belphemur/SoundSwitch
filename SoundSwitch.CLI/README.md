# SoundSwitch CLI

A command-line interface for controlling SoundSwitch.

All commands accept `--json` for machine-readable output; failures in JSON mode print a JSON object with an `error` field and exit with code 1:

```json
{ "error": "Failed to retrieve status" }
```

## Commands

### Switch Audio Device
Switch between recording or playback devices:
```shell
SoundSwitch.CLI.exe switch --type Recording
SoundSwitch.CLI.exe switch --type Playback
SoundSwitch.CLI.exe switch --type Playback --json
```

The `--json` variant prints `{ "success": true, "type": "Playback" }` on success and `{ "error": "..." }` on failure (exit code 1).

```json
{
  "success": true,
  "type": "Playback"
}
```

### Microphone Control
Manage microphone mute state:
```shell
SoundSwitch.CLI.exe mute              # Show current mute state
SoundSwitch.CLI.exe mute --state true # Mute the microphone
SoundSwitch.CLI.exe mute -s false     # Unmute the microphone
SoundSwitch.CLI.exe mute --toggle     # Toggle mute state
SoundSwitch.CLI.exe mute --json       # Query current state as JSON
```

The `--json` variant always prints `{ "deviceName": "...", "isMuted": true|false }`; on failure it prints `{ "error": "..." }` (exit code 1).

```json
{
  "deviceName": "Microphone (USB Audio Device)",
  "isMuted": false
}
```

### Profile Management
List all available profiles:
```shell
SoundSwitch.CLI.exe profile --list
SoundSwitch.CLI.exe profile --list --json
```

The `--json` variant prints a JSON array of profile objects (`name`, `playbackDevice`, `playbackCommunicationDevice`, `recordingDevice`, `recordingCommunicationDevice`).

```json
[
  {
    "name": "Gaming",
    "playbackDevice": "Speakers (Realtek(R) Audio)",
    "playbackCommunicationDevice": "Headset Earphone (HyperX Cloud II)",
    "recordingDevice": "Microphone (USB Audio Device)",
    "recordingCommunicationDevice": "Headset Microphone (HyperX Cloud II)"
  }
]
```

Trigger a specific profile:
```shell
SoundSwitch.CLI.exe profile --name "Profile Name"
```

With `--name` and `--json`, on success the command prints `{ "success": true, "profile": "Profile Name" }`; on failure it prints `{ "error": "..." }` (exit code 1).

```json
{
  "success": true,
  "profile": "Gaming"
}
```

### Settings
Open SoundSwitch settings:
```shell
SoundSwitch.CLI.exe settings
SoundSwitch.CLI.exe settings --json
```

The `--json` variant prints `{ "success": true }` on success and `{ "error": "..." }` on failure (exit code 1).

```json
{
  "success": true
}
```

### Status
Show active profile and current default audio devices:
```shell
SoundSwitch.CLI.exe status              # Human-readable table output
SoundSwitch.CLI.exe status --json       # Machine-readable JSON output
```

The JSON output includes: `activeProfile`, `playbackDevice`, `recordingDevice`, `playbackCommunicationDevice`, `recordingCommunicationDevice`. Devices that are not present are serialized as empty strings (`""`). On failure, `status --json` prints a JSON object with an `error` field and exits with code 1.

```json
{
  "activeProfile": "Gaming",
  "playbackDevice": "Speakers (Realtek(R) Audio)",
  "recordingDevice": "Microphone (USB Audio Device)",
  "playbackCommunicationDevice": "Headset Earphone (HyperX Cloud II)",
  "recordingCommunicationDevice": "Headset Microphone (HyperX Cloud II)"
}
```

**Note:** "Active Profile" shows the last profile explicitly triggered by a user action or trigger. It may not reflect changes made outside SoundSwitch (e.g., via Windows sound settings).

### Devices
List devices that are both currently active and selected in SoundSwitch settings (i.e. devices SoundSwitch will cycle through):
```shell
SoundSwitch.CLI.exe devices
SoundSwitch.CLI.exe devices --json
```

The `--json` variant prints `{ "playbackDevices": [...], "recordingDevices": [...] }`; on failure it prints `{ "error": "..." }` (exit code 1).

```json
{
  "playbackDevices": [
    "Speakers (Realtek(R) Audio)",
    "Headset Earphone (HyperX Cloud II)"
  ],
  "recordingDevices": [
    "Microphone (USB Audio Device)"
  ]
}
```

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

8. List devices selected for switching:
```shell
SoundSwitch.CLI.exe devices
SoundSwitch.CLI.exe devices --json
```

## Error Handling

The CLI provides clear error messages when:
- SoundSwitch is not running
- Profile names are invalid
- Connection issues occur
- Invalid commands or options are provided
- No default microphone is set (for mute commands)

All commands support the `--help` flag for additional information.
