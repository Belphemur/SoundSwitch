"""Convert Markdown files to standalone HTML documents.

Replaces the ``markdown-html`` npm package that was previously used in
the build pipeline.  Requires the ``markdown`` PyPI package::

    pip install markdown

Usage::

    python tools/markdown_to_html.py README.md -o Final/Readme.html
    python tools/markdown_to_html.py CHANGELOG.md Terms.md -d Final
"""

from __future__ import annotations

import argparse
import os
import sys
from pathlib import Path

try:
    import markdown
except ImportError:
    sys.exit(
        "The 'markdown' package is required.  Install it with:\n"
        "  pip install markdown"
    )

_HTML_TEMPLATE = """\
<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>{title}</title>
<style>
  body {{
    font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Helvetica,
                 Arial, sans-serif, "Apple Color Emoji", "Segoe UI Emoji";
    line-height: 1.6;
    max-width: 900px;
    margin: 2rem auto;
    padding: 0 1rem;
    color: #24292e;
  }}
  a {{ color: #0366d6; text-decoration: none; }}
  a:hover {{ text-decoration: underline; }}
  h1, h2, h3 {{ border-bottom: 1px solid #eaecef; padding-bottom: .3em; }}
  code {{
    background: #f6f8fa; padding: .2em .4em; border-radius: 3px;
    font-size: 85%;
  }}
  pre {{
    background: #f6f8fa; padding: 1em; border-radius: 6px;
    overflow-x: auto;
  }}
  pre code {{ background: none; padding: 0; }}
  img {{ max-width: 100%; }}
  table {{
    border-collapse: collapse; width: 100%;
  }}
  th, td {{
    border: 1px solid #dfe2e5; padding: 6px 13px;
  }}
  blockquote {{
    border-left: 4px solid #dfe2e5; margin: 0; padding: 0 1em;
    color: #6a737d;
  }}
</style>
</head>
<body>
{body}
</body>
</html>
"""

_EXTENSIONS = [
    "extra",       # tables, fenced_code, footnotes, etc.
    "codehilite",  # code highlighting (gracefully degrades without pygments)
    "toc",         # table-of-contents [toc] support
    "sane_lists",  # better list handling
]


def convert(source: str, title: str = "") -> str:
    """Return a full HTML document from *source* Markdown text."""
    md = markdown.Markdown(extensions=_EXTENSIONS, output_format="html")
    body = md.convert(source)
    return _HTML_TEMPLATE.format(title=title, body=body)


def _parse_args(argv: list[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Convert Markdown files to standalone HTML documents."
    )
    parser.add_argument(
        "files",
        nargs="+",
        help="Markdown file(s) to convert.",
    )
    group = parser.add_mutually_exclusive_group()
    group.add_argument(
        "-o",
        "--output",
        default=None,
        help="Output HTML file path (only valid with a single input file).",
    )
    group.add_argument(
        "-d",
        "--output-dir",
        default=None,
        help="Directory in which to write .html files (one per input file).",
    )
    return parser.parse_args(argv)


def main(argv: list[str] | None = None) -> None:
    args = _parse_args(argv)

    if args.output and len(args.files) > 1:
        sys.exit("Error: --output / -o can only be used with a single input file.")

    for filepath in args.files:
        path = Path(filepath)
        if not path.is_file():
            sys.exit(f"Error: '{filepath}' does not exist or is not a file.")

        source = path.read_text(encoding="utf-8")
        title = path.stem.replace("-", " ").replace("_", " ").title()
        html = convert(source, title=title)

        if args.output:
            out_path = Path(args.output)
        elif args.output_dir:
            out_path = Path(args.output_dir) / (path.stem + ".html")
        else:
            out_path = path.with_suffix(".html")

        os.makedirs(out_path.parent, exist_ok=True)
        out_path.write_text(html, encoding="utf-8")
        print(f"  {filepath} -> {out_path}")


if __name__ == "__main__":
    main()
