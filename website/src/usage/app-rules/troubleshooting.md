---
title: App Rule troubleshooting
description: Fix App Rules that do not apply, hotkeys that do nothing, duplicate targets, unavailable devices and cycles that do not advance.
---

# App Rule troubleshooting

## The rule does not apply automatically

Automatic mode applies when a matching process starts or comes to the foreground.

- **Is the rule enabled?** The checkbox in the App Rules list must be checked.
- **Is the mode Automatic?** A [Hotkey rule](./hotkey.md) never applies on its own.
- **Does the pattern match?** Open the rule and check **Process Path** and **Window Title**. A blank field is a wildcard; a non-blank field is a regular expression. Use the process picker to avoid escaping mistakes.
- **Is the window title wrong?** If **Window Title** is set, the rule only matches windows whose title matches. Clear it to match every window of the process.
- **Is it a child process?** Some applications launch the audio-owning process separately. Add a rule for that process, or use a broader pattern such as `.*game.*`.
- **Is the device available?** An unplugged or disabled device is skipped. Reconnect it, or edit the pair to use an active device.
- **Still stuck?** Open the **Troubleshooting** tab and click **Reset** under **Reset Per App Audio**. Windows reassigns applications to their default devices. An **Automatic** rule then re-applies on the next process or window event; a **Hotkey** rule needs another press of its shortcut.

## The hotkey does nothing

- **Is the mode Hotkey?** Automatic rules ignore their hotkey (and the editor clears it when saved).
- **Is a hotkey set?** Open the rule and check the **Hotkey** field.
- **Is the rule enabled?** Disabled Hotkey rules are not registered.
- **Is the application running?** If no process matches, SoundSwitch shows "The application isn't running — nothing switched." and does not cycle. Start the application and press again.
- **Is the hotkey registered?** Another App Rule, a Profile, another SoundSwitch feature, or another application may own it. Try a different combination.
- **Did the cycle reset?** Editing, enabling, disabling, or changing the rule's mode resets the cycle to the first pair. That is expected.

## "This hotkey is already used by another rule, profile, or application"

SoundSwitch checks the shortcut system-wide when you save. Pick a combination that no other rule, profile, or application uses. Avoid shortcuts reserved by Windows or your keyboard software.

## "Another Hotkey rule already targets this application and window"

Two Hotkey rules cannot have the same **Process Path** and **Window Title**. Merge them into one rule with more pairs, or change the target of one rule.

An Automatic rule and a Hotkey rule may share a target. If the Automatic rule keeps overriding the Hotkey rule when the application gains focus, disable or delete the Automatic rule.

## The cycle does not advance

The cycle advances only when:

1. at least one running process matches the rule, **and**
2. the current pair has at least one device that resolves to an active device.

If the pair's devices are all unplugged or unavailable, the press is ignored and the cycle stays on that pair. Reconnect a device or edit the pair.

Pressing the hotkey with the application closed also leaves the cycle unchanged.

## The wrong device is applied

- **Automatic mode** always uses the **first pair**. Edit the single playback/recording combos, or switch the rule to Hotkey mode if you need several pairs.
- **Hotkey mode** uses the current pair in the cycle. Check the pair order with **Move up** / **Move down**.
- **The list shows the first pair**, not the pair that is currently active. Hover the row to see the full list.
- If a pair uses **Keep current**, that side is intentionally left untouched.

## "Keep current" is not what I expected

**Keep current** means SoundSwitch does not change that side for this pair. It does not restore the previous device. If you want to move a side back later, add a pair that explicitly selects a device.

## A device is marked "(ambiguous name)"

Two active devices share the same display name, so SoundSwitch refuses to guess which one a rule meant. Rename one of the devices, or edit the rule and re-select the device.

## A rule stopped working after a device change

SoundSwitch resolves devices by id, then by device-instance path, then by unique name, and updates the stored identity when it finds a match. If the device was renamed and is no longer unique, re-select it in the rule. See [Device pairs](./device-pairs.md).

## Collect logs

If the problem persists, open the **Troubleshooting** tab and click **Export** under **Export Log Files**. The zip archive contains the application logs and can be shared in the GitHub discussions or Discord.

## Related

- [Automatic mode](./automatic.md)
- [Hotkey mode](./hotkey.md)
- [Device pairs](./device-pairs.md)
- [Examples](./examples.md)
