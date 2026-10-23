---
title: App Rule examples
description: Ready-to-adapt App Rule configurations for browsers, games, voice chat, music players and virtual audio cables.
---

# App Rule examples

These examples use generic device names. Replace them with the devices that exist on your machine. Patterns are regular expressions; the process picker fills them in for you.

## Automatic rules

### Send a browser to the speakers

| Field | Value |
|-------|-------|
| Process Path | `.*chrome\.exe.*` |
| Window Title | *(blank)* |
| Mode | Automatic |
| Pair | Speakers / *(not set)* |

Every Chrome process is routed to the speakers. Recording is left alone.

### Route only YouTube to headphones

| Field | Value |
|-------|-------|
| Process Path | `.*chrome\.exe.*` |
| Window Title | `.*YouTube.*` |
| Mode | Automatic |
| Pair | Headphones / *(not set)* |

Chrome windows whose title contains "YouTube" use the headphones when they are the foreground window. Because Chrome shares one process for all its windows, the process's audio then follows that rule — other Chrome windows in the same process are routed with it, so they do not keep the speakers.

### Give a game its own microphone

| Field | Value |
|-------|-------|
| Process Path | `.*game\.exe.*` |
| Window Title | *(blank)* |
| Mode | Automatic |
| Pair | *(not set)* / Headset microphone |

Only the recording side is changed. The game's playback keeps using whatever device it already had.

### Keep voice chat on a separate output

| Field | Value |
|-------|-------|
| Process Path | `.*discord\.exe.*` |
| Window Title | *(blank)* |
| Mode | Automatic |
| Pair | Chat headset / Chat microphone |

Discord's playback and recording both move to the chat devices. The system default is unchanged, so the game can stay on the main headset.

## Hotkey rules

### Cycle a game between headset and speakers

| Field | Value |
|-------|-------|
| Process Path | `.*game\.exe.*` |
| Window Title | *(blank)* |
| Mode | Hotkey |
| Hotkey | `Ctrl + Shift + F1` |
| Pair 1 | Headset / Headset microphone |
| Pair 2 | Speakers / *(not set)* |

Press the hotkey once to play through the headset with the headset microphone. Press again to move game playback to the speakers while leaving the microphone alone. The third press returns to the headset.

### Cycle a music player through three outputs

| Field | Value |
|-------|-------|
| Process Path | `.*spotify\.exe.*` |
| Window Title | *(blank)* |
| Mode | Hotkey |
| Hotkey | `Ctrl + Alt + M` |
| Pair 1 | Speakers / *(not set)* |
| Pair 2 | Headphones / *(not set)* |
| Pair 3 | Virtual cable / *(not set)* |

Each press moves Spotify to the next output without touching the system default or any other application.

### Toggle a browser between speakers and a virtual cable

| Field | Value |
|-------|-------|
| Process Path | `.*msedge\.exe.*` |
| Window Title | *(blank)* |
| Mode | Hotkey |
| Hotkey | `Ctrl + Alt + B` |
| Pair 1 | Speakers / *(not set)* |
| Pair 2 | Virtual cable / *(not set)* |

Useful when recording browser audio or feeding it to another application.

### Route a recording-only source

| Field | Value |
|-------|-------|
| Process Path | `.*audacity\.exe.*` |
| Window Title | *(blank)* |
| Mode | Hotkey |
| Hotkey | `Ctrl + Alt + R` |
| Pair 1 | *(not set)* / Line In |
| Pair 2 | *(not set)* / Virtual cable |

The playback side is never changed; each press selects a different recording source.

## Combining rules

You can run several rules at once as long as they target different applications:

- Chrome → speakers (Automatic)
- Game → headset (Automatic)
- Discord → chat headset (Automatic)

The system default stays wherever it was. Each application is routed independently.

## Tips

- Use the process picker instead of typing patterns when possible. It fills in escaped file names and, when available, the current window title.
- Keep **Window Title** blank unless you need to distinguish windows of the same process.
- Start with **Notify when triggered** enabled while setting up a rule. Turn it off once the routing works.
- For a Hotkey rule, press the hotkey with the application closed once to confirm the "isn't running" message, then start the application and press again.
- Use [Device pairs](./device-pairs.md) to combine playback and recording changes in one step.

## Related

- [Automatic mode](./automatic.md)
- [Hotkey mode](./hotkey.md)
- [Troubleshooting](./troubleshooting.md)
