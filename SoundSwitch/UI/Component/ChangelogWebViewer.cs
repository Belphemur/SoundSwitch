/********************************************************************
* Copyright (C) 2015-2017 Antoine Aflalo
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of the GNU General Public License
* as published by the Free Software Foundation; either version 2
* of the License, or (at your option) any later version.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
********************************************************************/

using System.Collections.Generic;
using System.Windows.Forms;

using Markdig;

using SoundSwitch.Framework.WinApi;
using SoundSwitch.Util.Url;

namespace SoundSwitch.UI.Component;

public class ChangelogWebViewer : WebBrowser
{
    public ChangelogWebViewer()
    {
        IsWebBrowserContextMenuEnabled = false;
        WebBrowserShortcutsEnabled = false;
        Navigating += OnNavigating;
    }

    private void OnNavigating(object sender, WebBrowserNavigatingEventArgs e)
    {
        if (e.Url.ToString() == "about:blank")
            return;

        e.Cancel = true;
        var url = e.Url.ToString();
        BrowserUtil.OpenUrl(url);
    }

    // The WebBrowser control (Trident) does not honor prefers-color-scheme,
    // so the style sheet is selected explicitly based on the app theme.
    internal const string LightStyleSheet = @"
                    body {
                        background: #fff; margin: 0 auto;
                        font-family: ""Segoe UI"", Helvetica, Arial, sans-serif;
                    }
                    h1 {
                        padding: 0.3em 0em 0.3em;
                        font-size: 1.2em;
                        border-bottom: 1px solid #eaecef;
                    }
                    h2 {
                        padding: 0.3em 0em 0.3em;
                        font-size: 1em;
                        border-bottom: 1px solid #eaecef;
                    }
                    .center {
                        text-align: center
                    }
                ";

    internal const string DarkStyleSheet = @"
                    body {
                        background: #202020; color: #e0e0e0; margin: 0 auto;
                        font-family: ""Segoe UI"", Helvetica, Arial, sans-serif;
                    }
                    h1 {
                        padding: 0.3em 0em 0.3em;
                        font-size: 1.2em;
                        border-bottom: 1px solid #3f3f3f;
                    }
                    h2 {
                        padding: 0.3em 0em 0.3em;
                        font-size: 1em;
                        border-bottom: 1px solid #3f3f3f;
                    }
                    a {
                        color: #6cb2f5;
                    }
                    .center {
                        text-align: center
                    }
                ";

    internal static string GetStyleSheet(bool isDarkMode)
    {
        return isDarkMode ? DarkStyleSheet : LightStyleSheet;
    }

    private static List<string> HtmlHeaders => new()
    {
        $@"<!doctype html>
            <html>
            <head>
                <meta charset=""utf-8"">
                <style>{GetStyleSheet(WindowsThemeHelper.IsDarkModeEnabled())}</style>
            </head>"
    };

    /// <summary>
    /// Set the changelog in the WebBrowser
    /// </summary>
    /// <param name="changelogLines"></param>
    public void SetChangelog(IEnumerable<string> changelogLines)
    {
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var lines = HtmlHeaders;
        lines.Add("<body>");
        lines.Add(Markdown.ToHtml(string.Join("\n", changelogLines), pipeline));
        lines.Add("</body>");
        lines.Add("</html>");
        DocumentText = string.Join("\n", lines);
    }
}
