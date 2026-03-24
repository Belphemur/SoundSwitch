# Icon Framework — `SoundSwitch.Common/Framework/Icon`

## Overview

This folder provides a general-purpose, GDI-safe icon extraction and caching layer.
It is the **single entry point** for loading icons from executables, DLLs, and `.ico` files across
the whole application. Audio-device-specific code in `SoundSwitch.Common/Framework/Audio/Icon/` delegates
to this layer and adds DataFlow-based fallback icons on top.

---

## Key Types

### `IconExtractor` (static)

Extracts and caches `System.Drawing.Icon` objects with full GDI reference counting.

| Method | Description |
|--------|-------------|
| `Extract(file, iconIndex, largeIcon)` | Extract icon from an executable or DLL by index. |
| `ExtractFromPath(path, largeIcon)` | Extract from a Windows audio-device icon path: either a `.ico` file or a `dllPath,iconIndex` comma-separated string. |
| `CreatePermanent(icon)` | Wrap a static, application-lifetime `Icon` in a permanent `CachedEntry`. Store the result in a `static readonly` field and never dispose it; call `Acquire()` on it to hand disposable handles to callers. |

Every successful extraction is stored in an `IMemoryCache` with a 30-minute **sliding** expiration and
a size-limited eviction policy. Icons are disposed only when **all** references are released (see below).

### `IconHandle` (public, `IDisposable`)

A disposable, reference-counted pointer to a cached GDI icon.

- **Callers must dispose their `IconHandle` when done.** A finalizer provides a last-resort release.
- `Icon` property — returns the underlying `System.Drawing.Icon`; throws `ObjectDisposedException` if the handle is disposed.
- `ToBitmap()` — convenience wrapper; the caller owns and must dispose the returned `Bitmap`.
- `Acquire()` — creates a new independent handle pointing to the same cached entry (ref count +1).
  Use this on a permanent handle to hand out per-caller references.

### `IconExtractionException`

Thrown when `IconExtractor` cannot load an icon (file missing, invalid index, etc.).

---

## GDI Reference-Counting Lifecycle

```
            ┌────────────────────────────────────────────┐
            │  CachedEntry (internal)                    │
            │  • wraps System.Drawing.Icon (HICON)       │
            │  • thread-safe refCount (int)              │
            └────────────────────────────────────────────┘
                      ▲              ▲
         cache holds  │ refCount=1   │ refCount+1 per handle
         initial ref  │              │
                      │      ┌───────────────┐
                      │      │  IconHandle   │ ← caller must Dispose()
                      │      └───────────────┘
                      │
              (eviction callback)
              entry.Release() ──► refCount-- 
                                  if refCount==0 ──► icon.Dispose()
```

1. `IconExtractor` creates a `CachedEntry` with `refCount = 1` (the cache's reference).
2. Each `IconHandle` returned to a caller increments `refCount` by 1.
3. When the cache evicts the entry its eviction callback calls `Release()`, decrementing the cache's ref.
4. When a caller disposes their `IconHandle`, `Release()` is called again.
5. The underlying `Icon.Dispose()` (which destroys the HICON) is only called when `refCount` reaches **zero** —
   meaning both the cache _and_ every caller have released their references.

---

## Rules for Callers

### Short-lived use (convert to `Bitmap` immediately)

```csharp
using var handle = IconExtractor.Extract(exePath, 0, largeIcon: true);
Image bitmap = handle.ToBitmap();
// handle is released here; cache may still hold the underlying icon
```

### Long-lived use (stored in a UI control's `Tag` / property)

Store the `IconHandle` directly. Dispose it when the control is cleared or the form closes.

```csharp
// Acquiring
listViewSubItem.Tag = device.SmallIcon; // returns IconHandle

// Releasing (before clearing the list or on form close)
foreach (var subItem in listView.SubItems)
    if (subItem.Tag is IDisposable d) d.Dispose();
listView.Items.Clear();
```

### Permanent (application-lifetime) fallback icons

```csharp
// Declaration — stored in a static field; never disposed
private static readonly IconHandle DefaultIcon = IconExtractor.CreatePermanent(Resources.MyIcon);

// Hand out a disposable reference per caller
return DefaultIcon.Acquire(); // caller must dispose the returned handle
```

---

## `AudioDeviceIconExtractor` Integration

`AudioDeviceIconExtractor` (in `SoundSwitch.Common/Framework/Audio/Icon/`) wraps `IconExtractor` to
add DataFlow-specific fallback icons (speaker / microphone). Its methods also return `IconHandle`;
the same lifecycle rules apply.

`DeviceFullInfo.LargeIcon` and `DeviceFullInfo.SmallIcon` are thin wrappers over
`AudioDeviceIconExtractor` and therefore also return `IconHandle`.

---

## Change Rules

- All new icon extraction code must go through `IconExtractor` — never call `System.Drawing.Icon.ExtractIcon`
  or `Icon.ExtractAssociatedIcon` directly from other layers.
- Never dispose an `IconHandle` that was obtained from a `CreatePermanent` static field; only dispose
  handles obtained from `Extract`, `ExtractFromPath`, or `Acquire`.
- When removing items from a `ListView` or `ComboBox` that stores `IconHandle` in `Tag` /
  `DropDownItem.IconHandle`, always iterate and dispose the handles before calling `Clear()`.
- Do not add `SizeLimit`, expiration, or priority overrides to individual cache entries — the defaults
  in `IconExtractor.ConfigureEntry` are the agreed-upon policy for the whole application.

## Validation

- Build the solution.
- Manual smoke-test: open Settings, switch profiles, observe that no GDI handle count grows unboundedly
  in Task Manager / Process Explorer.
