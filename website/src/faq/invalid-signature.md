# "Invalid Digital Signature" when updating

If the auto-updater fails with:

> Invalid Digital Signature — The downloaded update isn't signed with a valid signature.

it means **Windows itself** could not validate that the installer was signed by a trusted certificate authority. SoundSwitch deliberately refuses to run an installer that fails signature verification.

## Most common cause

Your version of Windows no longer receives the **root certificate updates** required to validate the signature.

This typically happens on:

- Windows 7 and earlier (end of life).
- Windows 10 below **21H1** (also out of support for certificate updates).

## How to fix it

1. **Update Windows** to a supported release. Windows 10 21H1+ or Windows 11 will receive the certificate updates needed to validate the installer.
2. As a workaround, you can also **download the installer manually** from the [SoundSwitch website](https://soundswitch.aaflalo.me) and run it. If Windows still complains, the underlying certificate trust problem must be resolved first.

> SoundSwitch supports **Windows 10 or newer (x64 or ARM64)**. Older systems are no longer covered.

---

_Source: [#952](https://github.com/Belphemur/SoundSwitch/discussions/952)_
