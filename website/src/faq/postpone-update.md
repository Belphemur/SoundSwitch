---
title: How does postponing a SoundSwitch update work?
description: Choose Later when SoundSwitch finds a new release and learn exactly how long the auto-updater waits before prompting again.
---

# How does postponing an update work?

When SoundSwitch finds a new release it asks whether you want to install it now or **Later**.

![Update available dialog](/images/faq/update-postpone.png)

## What "Later" does

Each time you click **Later**, SoundSwitch waits longer before reminding you again:

| Click # | Reminder delay |
| ------- | -------------- |
| 1       | 3 days         |
| 2       | 7 days         |
| 3       | 12 days        |
| 4+      | 28 days        |

After the fourth postponement, every additional **Later** click reminds you in another 28 days.

![Reminder options](/images/faq/update-later-options.png)

## Triggering the update yourself

You don't have to wait for the next reminder — right-click the tray icon and choose **Update** at any time to install immediately. See [How do I manually check for an update?](./manually-check-update.md).

---

_Source: [#651](https://github.com/Belphemur/SoundSwitch/discussions/651)_
