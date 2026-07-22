---
title: Can I control SoundSwitch from the command line?
description: Yes — SoundSwitch ships with an official CLI bundled with the installer and added to your PATH so you can script device switching and profile changes.
---

# Is there a way to control SoundSwitch from the command line?

Yes — SoundSwitch ships with an official **CLI** (`SoundSwitch.CLI`). It is bundled with the installer and added to your `PATH` so you can call it from any terminal:

```shell
SoundSwitch.CLI.exe switch --type Playback
SoundSwitch.CLI.exe switch --type Recording
SoundSwitch.CLI.exe mute --toggle
SoundSwitch.CLI.exe profile --list --json
SoundSwitch.CLI.exe status
SoundSwitch.CLI.exe devices
```

All commands accept `--json` for machine-readable output; failures in JSON mode print a JSON object with an `error` field and exit with code 1.

See the [CLI usage page](../usage/cli.md) for the full command reference.

## Third-party tools

Historically, before SoundSwitch shipped with its own CLI, the recommended workaround was [**NirCMD**](https://www.nirsoft.net/utils/nircmd.html), which can change the default playback device from a script. It is still useful for very advanced scenarios that the SoundSwitch CLI doesn't cover.

---

_Source: [#1081](https://github.com/Belphemur/SoundSwitch/discussions/1081)_
