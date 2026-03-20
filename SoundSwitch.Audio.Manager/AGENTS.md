# SoundSwitch.Audio.Manager Agents

## Scope

This project owns Windows audio device switching, endpoint interaction, policy config, and audio-related interop.

## Responsibilities

- Keep Windows audio behavior isolated here.
- Wrap NAudio and native interop details behind focused APIs.
- Expose stable device operations to the main app.

## Change Rules

- Do not add UI or localization concerns in this project.
- Preserve behavior across playback and recording device flows.
- Be careful with COM/native resource lifetime, device IDs, and Windows-version-specific behavior.
- When exposing device display data, prefer `NameClean` over raw device names.

## Validation

- Build the solution.
- Run `SoundSwitch.Audio.Manager.Tests` when changing switching, policy, or interop logic.
