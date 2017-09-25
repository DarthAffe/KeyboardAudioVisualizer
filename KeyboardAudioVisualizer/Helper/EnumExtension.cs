using System;
using System.Linq;

namespace KeyboardAudioVisualizer.Helper
{
    public static class EnumExtension
    {
        #region Methods

        public static T GetAttribute<T>(this Enum e)
            where T : Attribute => e.GetType().GetMember(e.ToString()).FirstOrDefault()?.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;

        #endregion
    }
}
