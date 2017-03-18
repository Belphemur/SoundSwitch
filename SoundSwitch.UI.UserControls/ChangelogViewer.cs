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
            "<!doctype html>",
            "<html>",
            "<head>",
            "<style>" +
            "body { background: #fff; margin: 0 auto; } " +
            "h1 { font-size: 15px; color: #1562b6; padding-top: 5px; border: 0px !important; border-bottom: 2px solid #1562b6 !important; }" +
            "h2 { font-size: 13px; color: #1562b6; padding-top: 5px; border: 0px !important; border-bottom: 1px solid #1562b6 !important; }" +
            ".center {text-align: center}" +
            "</style>",
            "</head>",
            "<body>"
        };

        /// <summary>
        ///     Set the changelog in the WebBrowser
        /// </summary>
        /// <param name="changelogLines"></param>
        public void SetChangelog(IEnumerable<string> changelogLines)
        {
            var lines = HtmlHeaders;
            lines.Add(CommonMarkConverter.Convert(string.Join("\n", changelogLines)));
            lines.Add("</body>");
            lines.Add("</html>");
            DocumentText = string.Join("\n", lines);
        }
    }
}