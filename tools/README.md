# Tools

- [`vswhere.exe`](https://github.com/Microsoft/vswhere) - Locate Visual Studio installations.
- [`markdown_to_html.py`](markdown_to_html.py) - Convert Markdown files to standalone HTML documents. Requires Python 3 with the `markdown` package (`pip install markdown`). Replaces the previously used `markdown-html` npm package.
- [`Download-Release.ps1`](Download-Release.ps1) - PowerShell script to download the latest release or beta build artifact from GitHub, extract it to `Final\`, and prepare for local signing and installer creation.
- [`Install-BuildTools.ps1`](Install-BuildTools.ps1) - PowerShell script that uses winget to install Inno Setup 6, Certum SimplySign Desktop (cloud certificate provider for code signing), and Python 3 (with the markdown package), and downloads `signtool.exe` from the lightweight `Microsoft.Windows.SDK.BuildTools` NuGet package (~22 MB) instead of the full Windows SDK. Run once on a fresh machine.
- [`Sign-Binary.ps1`](Sign-Binary.ps1) - PowerShell script for code signing executables using signtool with SHA-256 digest and RFC 3161 timestamping. Configurable certificate subject name (defaults to "OpenSource Developer, Antoine Aflalo"). Called automatically by Build-Installer.ps1 when signtool is available.
- [`Build-Installer.ps1`](Build-Installer.ps1) - PowerShell script to build the SoundSwitch installer. Supports building from source or from a downloaded release artifact. Handles HTML documentation generation, asset bundling, code signing (via Sign-Binary.ps1), and Inno Setup compilation.
- [`upload_nightly_r2.py`](upload_nightly_r2.py) - Upload nightly build archives to Cloudflare R2 and notify Discord.
