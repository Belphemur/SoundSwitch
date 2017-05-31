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

using System;
using System.Drawing;
using System.Windows.Forms;

namespace SoundSwitch.UI.UserControls
{
    public class TextProgressBar : ProgressBar
    {
        public enum ProgressBarDisplayText
        {
            Percentage,
            CustomText,
            Both
        }

        public TextProgressBar()
        {
            // Modify the ControlStyles flags
            //http://msdn.microsoft.com/en-us/library/system.windows.forms.controlstyles.aspx
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
        }

        //Property to set to decide whether to print a % or Text
        public ProgressBarDisplayText DisplayStyle { get; set; }
        //Property to hold the custom text
        public string CustomText { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            var rect = ClientRectangle;
            var g = e.Graphics;

            // Validate that the user has enabled visual styles in the operating system
            if (ProgressBarRenderer.IsSupported)
            {
                ProgressBarRenderer.DrawHorizontalBar(g, rect);
                rect.Inflate(-3, -3);
                if (Value > 0)
                {
                    // As we doing this ourselves we need to draw the chunks on the progress bar
                    var clip = new Rectangle(rect.X, rect.Y, (int) Math.Round(((float) Value / Maximum) * rect.Width),
                        rect.Height);
                    ProgressBarRenderer.DrawHorizontalChunks(g, clip);
                }
            }

            // Set the Display text (Either a % amount or our custom text
            string text = "";
            switch (DisplayStyle)
            {
                case ProgressBarDisplayText.Both:
                    text = String.Format("{0}: {1}%", CustomText, Value);
                    break;
                case ProgressBarDisplayText.CustomText:
                    text = CustomText;
                    break;
                case ProgressBarDisplayText.Percentage:
                    text = String.Format("{0}%", Value);
                    break;
            }

            using (var f = new Font(FontFamily.GenericSansSerif, 8.25F))
            {
                var len = g.MeasureString(text, f);
                // Calculate the location of the text (the middle of progress bar)
                // Point location = new Point(Convert.ToInt32((rect.Width / 2) - (len.Width / 2)), Convert.ToInt32((rect.Height / 2) - (len.Height / 2)));
                var location = new Point(Convert.ToInt32((Width / 2) - len.Width / 2),
                    Convert.ToInt32((Height / 2) - len.Height / 2));
                // The commented-out code will centre the text into the highlighted area only. This will centre the text regardless of the highlighted area.
                // Draw the custom text
                g.DrawString(text, f, Brushes.Black, location);
            }
        }
    }
}