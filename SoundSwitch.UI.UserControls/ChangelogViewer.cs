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
using CommonMark;

namespace SoundSwitch.UI.UserControls
{
    public class ChangelogWebViewer : WebBrowser
    {
        public ChangelogWebViewer()
        {
            IsWebBrowserContextMenuEnabled = false;
            WebBrowserShortcutsEnabled = false;
        }

        protected static List<string> HtmlHeaders => new List<string>
        {
            @"<!doctype html>
            <html>
            <head>
                <style>
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
                </style>
            </head>"
        };

        /// <summary>
        /// Set the changelog in the WebBrowser
        /// </summary>
        /// <param name="changelogLines"></param>
        public void SetChangelog(IEnumerable<string> changelogLines)
        {
            var lines = HtmlHeaders;
            lines.Add("<body>");
            lines.Add(CommonMarkConverter.Convert(string.Join("\n", changelogLines)));
            lines.Add("</body>");
            lines.Add("</html>");
            DocumentText = string.Join("\n", lines);
        }
    }
}