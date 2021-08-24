using System.Drawing;

namespace SoundSwitch.Util
{
    public static class ColorExtensions
    {
        /// <summary>
        /// Set an opacity to a <see cref="Color"/>
        /// </summary>
        /// <param name="color"></param>
        /// <param name="opacity"></param>
        /// <returns></returns>
        public static Color WithOpacity(this Color color, int opacity) => Color.FromArgb(opacity, color.R, color.G, color.B);
    }
}