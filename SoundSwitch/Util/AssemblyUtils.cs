using System.Linq;
using System.Reflection;

namespace SoundSwitch.Util
{
    public static class AssemblyUtils
    {
        public enum ReleaseState
        {
            Stable,
            Beta
        }
        /// <summary>
        /// Get Current Assembly configuration
        /// </summary>
        /// <returns></returns>
        public static AssemblyConfigurationAttribute GetAssemblyConfigurationAttribute()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var attributes = assembly.GetCustomAttributes(true);
            var config = attributes.OfType<AssemblyConfigurationAttribute>().FirstOrDefault();
            return config;
        }
        /// <summary>
        /// Get the current state of the application
        /// </summary>
        /// <returns></returns>
        public static ReleaseState GetReleaseState()
        {
            return GetAssemblyConfigurationAttribute().Configuration == "Beta" ? ReleaseState.Beta : ReleaseState.Stable;
        }
    }
}