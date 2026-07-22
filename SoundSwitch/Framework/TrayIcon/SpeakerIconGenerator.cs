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
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Serilog;

namespace SoundSwitch.Framework.TrayIcon;

/// <summary>
/// Generates theme-aware speaker tray icons using Segoe font glyphs.
/// Icons are generated on-demand and cached until DPI changes.
/// </summary>
public static class SpeakerIconGenerator
{
    private static readonly ILogger Log = Log.ForContext(typeof(SpeakerIconGenerator));

    // Font priority: Fluent (Win11) first, MDL2 (Win10+) fallback
    private static readonly string[] IconFonts = { "Segoe Fluent Icons", "Segoe MDL2 Assets" };
    private const char SpeakerGlyph = '\uE767';

    private static readonly object _lock = new object();

    private static string _cachedFontName;
    private static Icon _cachedLightIcon;
    private static Icon _cachedDarkIcon;
    private static int _cachedSize;

    [DllImport("user32.dll")]
    private static extern bool DestroyIcon(IntPtr handle);

    /// <summary>
    /// Finds the best available icon font on this system.
    /// Caches the result after first call.
    /// </summary>
    private static string GetAvailableIconFont()
    {
        lock (_lock)
        {
            if (_cachedFontName != null)
                return _cachedFontName;

            foreach (var fontName in IconFonts)
            {
                using var testFont = new Font(fontName, 16, FontStyle.Regular, GraphicsUnit.Pixel);
                // A missing font falls back to a different family; check Name to detect fallback
                if (testFont.Name == fontName)
                {
                    _cachedFontName = fontName;
                    Log.Information("Using icon font: {Font}", fontName);
                    return fontName;
                }
            }

            // Guaranteed on Windows 10+ — this should never be reached
            _cachedFontName = "Segoe MDL2 Assets";
            Log.Warning("No preferred icon font found; falling back to Segoe MDL2 Assets");
            return _cachedFontName;
        }
    }

    /// <summary>
    /// Generates and caches a speaker icon for the given taskbar theme.
    /// </summary>
    /// <param name="isDarkTaskbar">true = dark taskbar → white icon; false = light taskbar → dark icon</param>
    /// <returns>A disposable Icon suitable for NotifyIcon.Icon.</returns>
    public static Icon GenerateSpeakerIcon(bool isDarkTaskbar)
    {
        int size = SystemInformation.SmallIconSize.Width;

        lock (_lock)
        {
            if (_cachedSize == size)
                return isDarkTaskbar ? _cachedDarkIcon : _cachedLightIcon;

            // Size changed (DPI change) — regenerate both variants
            _cachedDarkIcon?.Dispose();
            _cachedLightIcon?.Dispose();

            _cachedDarkIcon = CreateIcon(isDarkTaskbar: true, size);
            _cachedLightIcon = CreateIcon(isDarkTaskbar: false, size);
            _cachedSize = size;

            Log.Debug("Generated speaker icons at {Size}px for theme-based tray icon", size);
            return isDarkTaskbar ? _cachedDarkIcon : _cachedLightIcon;
        }
    }

    /// <summary>
    /// Invalidates the icon cache. Call when DPI changes.
    /// </summary>
    public static void InvalidateCache()
    {
        lock (_lock)
        {
            _cachedDarkIcon?.Dispose();
            _cachedLightIcon?.Dispose();
            _cachedDarkIcon = null;
            _cachedLightIcon = null;
            _cachedSize = 0;
        }
    }

    private static Icon CreateIcon(bool isDarkTaskbar, int iconSize)
    {
        var fontName = GetAvailableIconFont();
        var foreground = isDarkTaskbar
            ? Color.FromArgb(240, 240, 240)
            : Color.FromArgb(30, 30, 30);

        // Font at ~85% of icon size leaves natural padding
        float fontSize = iconSize * 0.85f;
        int bitmapSize = iconSize + 2; // 1px padding on each side

        using var font = new Font(fontName, fontSize, FontStyle.Regular, GraphicsUnit.Pixel);
        using var brush = new SolidBrush(foreground);
        using var bitmap = new Bitmap(bitmapSize, bitmapSize);
        using var g = Graphics.FromImage(bitmap);

        g.Clear(Color.Transparent);
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

        var glyphString = SpeakerGlyph.ToString();
        var measuredSize = g.MeasureString(glyphString, font, new PointF(0, 0),
            StringFormat.GenericTypographic);
        float x = (bitmapSize - measuredSize.Width) / 2f;
        float y = (bitmapSize - measuredSize.Height) / 2f;

        g.DrawString(glyphString, font, brush, x, y);
        g.Flush();

        // Clone-and-destroy pattern: detach from bitmap's GDI handle
        IntPtr hIcon = bitmap.GetHicon();
        var icon = (Icon)Icon.FromHandle(hIcon).Clone();
        DestroyIcon(hIcon);
        return icon;
    }
}
