# Default "Switch Default Communication Device" to ON for new installs

**Status:** Accepted
**Author:** pi agent (comms-default-on campaign)
**Branch:** `comms-default-on`
**Base:** `origin/dev` (`6c1e679e`)
**Issue:** [#2612](https://github.com/Belphemur/SoundSwitch/issues/2612) - Communication device not switching by default confuses users

---

## 1. Problem

The **Switch Default Communication Device** option defaults to OFF
(`SoundSwitch/Framework/Configuration/SoundSwitchConfiguration.cs:59`,
`public bool ChangeCommunications { get; set; } = false;`). Users who cycle
playback devices expect the Windows **Default Device** — and apps following it,
e.g. Discord — to move; because communication-role switching never happens out
of the box, the switch appears to "not work" (#2612).

## 2. Goals

1. Every **new** install gets communication-device switching enabled by default.
2. Existing installs keep their configured behaviour — no behavior change without
   consent, no migration.

## 3. Non-goals

- Migrating existing users' setting value.
- Any UI, hotkey or switching-logic change. The Settings checkbox
  (`SoundSwitch/UI/Forms/Settings.cs:221`) already binds to the config value via
  `AppModel.SetCommunications`; no UI code changes.

## 4. Design

### 4.1 Behaviour

Flip the property initializer to `true`. Only fresh configurations are affected:
`ConfigurationManager.SaveConfiguration`
(`SoundSwitch/Framework/Configuration/ConfigurationManager.cs`) serializes the
**full** config object with `NullValueHandling.Ignore` — nulls are dropped, but
booleans are always written. Therefore every existing install's
`SoundSwitchConfiguration.json` already contains an explicit
`"ChangeCommunications": <value>` line, which is deserialized verbatim on load;
the initializer is only consulted when the config file does not exist yet.

### 4.2 Migration

None. Deliberate decision: existing installs keep whatever they have.

## 5. Telemetry

No new events.

## 6. Localization

No new user-facing strings — the existing checkbox label and tooltip are reused.

## 7. Files touched

| File | Change |
|---|---|
| `SoundSwitch/Framework/Configuration/SoundSwitchConfiguration.cs` | `ChangeCommunications` default `false` → `true` |
| `SoundSwitch.Tests/…` | Regression test pinning the fresh-config default |
| `website/src/usage/communications.md` | Default now ON; "How to Disable" |
| `website/src/configuration/general/behavior.md` | Default-state wording |
| `README.md` | Default-state wording |

## 8. Test plan

### Automated

- New test in `SoundSwitch.Tests` asserting a fresh `SoundSwitchConfiguration`
  has `ChangeCommunications == true` (execution requires Windows; Linux box can
  only compile it).

### Manual

1. Fresh install (no config file) → General tab shows the checkbox checked;
   switching playback also switches the communication device.
2. Existing install with the option off → still off after upgrade.

## 9. Decisions

| # | Decision | Rationale |
|---|---|---|
| 1 | Default flip only, no migration | Owner decision: new installs get ON; existing users keep their choice. Persistence writes explicit booleans, so the initializer is fresh-install-only. |
| 2 | Rejected: migrate existing users to ON too | Changes behavior without consent. |
| 3 | Rejected: leave OFF and improve notifications | Owner rejected; the surprise is the missing switch itself, not the messaging. |
