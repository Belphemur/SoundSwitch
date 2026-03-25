using NAudio.CoreAudioApi;
using Serilog;
using SoundSwitch.Common.Framework.Audio.Icon;
using SoundSwitch.Common.Framework.Icon;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System;

namespace SoundSwitch.Banner
{
    /// <summary>
    /// Utility for resolving various string inputs into a <see cref="System.Drawing.Image"/>.
    /// Supports URLs, File Paths, EXE/ICO extractions, Base64 strings, and Audio Device IDs.
    /// </summary>
    public static class ImageResolver
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        private static readonly ILogger Logger = Log.ForContext(typeof(ImageResolver));

        public static async Task<Image?> ResolveAsync(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            try
            {
                // 1. Base64 Check
                if (input.StartsWith("data:image", StringComparison.OrdinalIgnoreCase) || input.Length > 100)
                {
                    try
                    {
                        var base64Data = input;
                        if (input.Contains(","))
                            base64Data = input.Split(',')[1];

                        var bytes = Convert.FromBase64String(base64Data);
                        using var ms = new MemoryStream(bytes);
                        return Image.FromStream(ms);
                    }
                    catch (FormatException) { /* Not base64, continue */ }
                }

                // 2. URL Check
                if (Uri.TryCreate(input, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                {
                    var bytes = await HttpClient.GetByteArrayAsync(uri);
                    using var ms = new MemoryStream(bytes);
                    return Image.FromStream(ms);
                }

                // 3. Audio Device ID Check (GUID pattern)
                if (input.StartsWith("{", StringComparison.Ordinal) && input.Contains("}.{", StringComparison.Ordinal))
                {
                    try
                    {
                        var enumerator = new MMDeviceEnumerator();
                        var device = enumerator.GetDevice(input);
                        if (device != null)
                        {
                            using var handle = AudioDeviceIconExtractor.ExtractIconFromAudioDevice(device, true);
                            return handle.Icon.ToBitmap();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning(ex, "Could not resolve audio device icon for {Id}", input);
                    }
                }

                // 4. File Path Check
                if (File.Exists(input))
                {
                    var extension = Path.GetExtension(input).ToLowerInvariant();
                    if (extension == ".exe" || extension == ".dll" || extension == ".ico")
                    {
                        using var handle = IconExtractor.ExtractFromPath(input, true);
                        return handle.Icon.ToBitmap();
                    }

                    return Image.FromFile(input);
                }

                // 5. Advanced Path Check (e.g. "shell32.dll,3")
                if (input.Contains(","))
                {
                    try
                    {
                        using var handle = IconExtractor.ExtractFromPath(input, true);
                        return handle.Icon.ToBitmap();
                    }
                    catch { /* Ignore and return null */ }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error resolving image from input: {Input}", input);
            }

            return null;
        }
    }
}
