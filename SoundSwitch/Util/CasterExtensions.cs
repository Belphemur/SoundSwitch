using System;

namespace SoundSwitch.Util
{
    public static class CasterExtensions
    {
        public static T CastTo<T>(this Object value, T targetType)
        {
            // targetType above is just for compiler magic
            // to infer the type to cast value to
            return (T)value;
        }
    }
}