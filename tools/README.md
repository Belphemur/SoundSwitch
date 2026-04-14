# Tools

- [`vswhere.exe`](https://github.com/Microsoft/vswhere) - Locate Visual Studio installations.
- [`markdown_to_html.py`](markdown_to_html.py) - Convert Markdown files to standalone HTML documents. Requires Python 3 with the `markdown` package (`pip install markdown`). Replaces the previously used `markdown-html` npm package.
- [`Download-Release.ps1`](Download-Release.ps1) - PowerShell script to download the latest release or beta build artifact from GitHub, extract it to `Final\`, and prepare for local signing and installer creation.
- [`upload_nightly_r2.py`](upload_nightly_r2.py) - Upload nightly build archives to Cloudflare R2 and notify Discord.
