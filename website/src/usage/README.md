---
title: Usage
description: Master SoundSwitch — global hotkeys, profiles, App Sound Lock per-app routing, communication device handling and the command-line interface.
---

# Usage

Learn how to use SoundSwitch effectively, from hotkeys to profiles and per-app audio routing.

---

## Profiles vs App Rules

SoundSwitch offers two distinct mechanisms for controlling audio routing. Understanding the difference is key to using the application effectively.

### Profiles — Change the System Default

**What it does**: Profiles switch the **Windows Default Audio Device**. When a profile activates, it tells Windows to use a different device as the default for all applications.

**When it activates**: A profile activates when one of its configured **triggers** fires — a hotkey press, a specific application gaining focus, Steam Big Picture launching, etc.

**Scope**: **System-wide**. Every application that uses the default playback or recording device is affected.

**Best for**:
- Switching between headphones and speakers when you sit down or stand up
- Setting a dedicated audio configuration for gaming sessions
- Automatically changing devices when you launch or focus a specific app
- Ensuring a preferred device is always active (force profile)

**Key behavior**: When an Application or Window trigger ends (the app closes or loses focus), SoundSwitch **restores the previous device configuration** if restoration is enabled on the profile.

### App Rules (App Sound Lock) — Route Individual Apps

**What it does**: App Rules route **specific applications** to a chosen audio device using Windows Audio Session APIs. The system default device **does not change**.

**When it activates**: Rules are evaluated continuously — whenever a new process starts or when the foreground window changes. If a rule's process pattern and window title match, the application's audio stream is immediately redirected.

**Scope**: **Per-application**. Only the matched application is affected; all other apps continue using the system default device.

**Best for**:
- Playing music through speakers while gaming on headphones
- Sending Discord audio to one device while browser audio goes to another
- Routing a specific app's microphone input to a virtual cable
- Fine-grained control over which app uses which device

**Key behavior**: Routing persists as long as the matched application is running. Multiple rules can match different apps and route each to a different device simultaneously.

### Can I Use Both Together?

Yes. Profiles and App Rules complement each other:

- A **Profile** can set your headphones as the system default when you launch a game.
- An **App Rule** can simultaneously route your music player to your speakers — so game audio goes to headphones (via the profile default) and music goes to speakers (via the rule).

### Quick Comparison

| Feature | Profiles | App Rules |
|---------|----------|-----------|
| **Changes system default** | Yes | No |
| **Affects all apps** | Yes | No (per-app only) |
| **Hotkey activation** | Yes | No |
| **App trigger** | Yes (Application path / Name of the program) | Yes (Process Path, Window Title with regex) |
| **Regex matching** | No (exact path match) | Yes (standard regex) |
| **Device restoration** | Yes | No |
| **Multiple simultaneous rules** | No (one active) | Yes |

---

## Pages in This Section

- **[Profiles](profiles.md)** — Automatically switch the system default audio device based on hotkeys, applications, and other triggers.
- **[App Sound Lock](app-rules.md)** — Route individual applications to specific audio devices without changing the system default.
