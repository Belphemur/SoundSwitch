---
title: Device pairs
description: Build the ordered playback/recording combinations an App Rule uses, with "Keep current" for either side.
---

# Device pairs

A **device pair** is one playback device and one recording device. Either side may be **Keep current**, which leaves that side of the application's audio untouched.

Pairs are the unit an App Rule applies:

- **Automatic** mode uses the **first pair only**.
- **Hotkey** mode cycles through **every pair in order**.

## A pair can be one-sided

| Playback | Recording | Result |
|----------|-----------|--------|
| Headset | Microphone | Switch both sides. |
| Speakers | **Keep current** | Switch playback only; recording stays where it is. |
| **Keep current** | Line In | Switch recording only; playback stays where it is. |
| **Keep current** | **Keep current** | Invalid. A pair must set at least one side. |

A rule must contain at least one valid pair. Automatic rules are normalized to exactly one pair when saved.

## The pair builder

In **Hotkey** mode, the **Hotkey** section contains the shortcut field and a device-pair builder:

- A playback combo and a recording combo, both including **Keep current**.
- **Add pair** / **Update pair**.
- **New pair**.
- **Remove**, **Move up**, and **Move down**.
- An ordered list with `#`, **Playback**, and **Recording** columns.

### Add a pair

1. Choose a playback device, a recording device, or both in the builder combos.
2. Click **Add pair**.

The pair is appended to the list and selected.

### Edit a pair

1. Select the pair in the list. Its devices load into the builder.
2. Change either combo.
3. Click **Update pair**.

### Start a new pair

Click **New pair** to clear the selection and reset both builder combos to **Keep current**. The button then reads **Add pair** again.

### Remove or reorder

- Select a pair and click **Remove**.
- Use **Move up** / **Move down** to change the cycle order. The first row is the pair Automatic mode would use.

## How the modes use pairs

### Automatic

Automatic mode applies `DevicePairs[0]` whenever the application matches. The editor shows only the single playback and recording combos, not the pair list. If you switch a Hotkey rule with several pairs to Automatic, SoundSwitch asks for confirmation and keeps only the first pair.

### Hotkey

Every press applies the next pair in list order and wraps after the last one. The current pair is shown in the notification (if the rule's **Notify when triggered** option is on). See [Hotkey mode](./hotkey.md) for the full cycling rules.

## Mode switching

- **Automatic → Hotkey**: the current single pair seeds the list. You can then add more pairs.
- **Hotkey → Automatic**: the first pair is copied into the single-device combos. If the rule has more than one pair, a confirmation explains that Automatic uses only the first pair and the others will be removed. Cancelling keeps Hotkey mode.

## How pairs appear in the App Rules list

The list has one row per rule:

- **Playback** and **Recording** show the **first pair**.
- **Mode** shows `Automatic` or `Hotkey`. A Hotkey rule with several pairs shows the count, for example `Hotkey (3)`.
- Hover the row to see **every** pair in a tooltip.

## Device resolution

Pairs store a device identity, not just a display name. When applying a pair, SoundSwitch resolves each side against the currently active devices:

1. exact device id,
2. the same device-instance path,
3. a unique device name.

If a device gets a new id after a driver update, the stored identity is updated automatically. If a device is unplugged, that side is skipped until it is available again. Ambiguously named devices are never guessed; the affected side is skipped and the rule is flagged in the App Rules list.

## Related

- [Automatic mode](./automatic.md)
- [Hotkey mode](./hotkey.md)
- [Examples](./examples.md)
- [Troubleshooting](./troubleshooting.md)
