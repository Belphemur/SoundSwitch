---
title: Command-Line Interface
description: Automate SoundSwitch from scripts and shortcuts with the bundled SoundSwitch.CLI — switch playback, recording, profiles and microphone mute from any terminal.
---

# CLI

SoundSwitch includes a command-line interface (`SoundSwitch.CLI`) for advanced users who want to control audio devices, manage profiles, and automate device switching through scripts.

## Available Commands

The CLI provides the following capabilities:

- **Switch devices** — Change the active playback or recording device by name or index.
- **Mute microphone** — Toggle or set the mute state of the default communication microphone.
- **Manage profiles** — Switch between saved audio profiles or list available profiles.
- **Access settings** — Query or modify SoundSwitch configuration values.

## Using the CLI

Run `SoundSwitch.CLI.exe --help` (or `--version`) from the command line to see all available commands and their syntax. The CLI communicates with the main SoundSwitch process via IPC, so the main application should be running.

## Examples

| Command | Description |
|---------|-------------|
| `SoundSwitch.CLI.exe --playback "Headphones"` | Switch the default playback device to "Headphones". |
| `SoundSwitch.CLI.exe --record "Microphone"` | Switch the default recording device to "Microphone". |
| `SoundSwitch.CLI.exe --mute-toggle` | Toggle the microphone mute state. |
| `SoundSwitch.CLI.exe --profile "Gaming"` | Activate the "Gaming" audio profile. |

For the full list of commands and options, run `SoundSwitch.CLI.exe --help` or see the [SoundSwitch.CLI README](https://github.com/Belphemur/SoundSwitch/blob/master/SoundSwitch.CLI/README.md) on GitHub.
