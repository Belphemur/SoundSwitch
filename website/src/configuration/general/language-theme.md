---
title: Language and theme
description: Set the SoundSwitch interface language and choose between following the Windows app theme or forcing light or dark on the General tab.
---

# Language and theme

The bottom-right group of the **General** tab sets how SoundSwitch speaks and looks.

## Language

Sets the display language for the SoundSwitch interface. Changing this requires restarting the application for the new language to take effect.

## Theme

Controls whether SoundSwitch follows the Windows light/dark app mode or forces its own palette.

| Option | Behavior |
|--------|----------|
| **Follow Windows** | SoundSwitch follows the Windows **Apps use light/dark theme** setting (the default). |
| **Light** | Force the light palette, regardless of the Windows setting. |
| **Dark** | Force the dark palette *(Windows 11)*. |

::: tip Windows 10 users
On Windows 10, Windows Forms applications cannot render the dark palette completely — the theme previously appeared as a broken mix of light and dark areas ([issue #2567](https://github.com/Belphemur/SoundSwitch/issues/2567)). SoundSwitch now falls back to a consistent light palette on Windows 10. If you want a predictable look, pick **Light** (or keep **Follow Windows**).
:::

Changing the theme requires restarting the application for the new palette to apply; SoundSwitch asks whether to restart when you pick a different value. The selection shown by SoundSwitch until that restart stays the one it was started with, so the window is never drawn with two competing palettes.
