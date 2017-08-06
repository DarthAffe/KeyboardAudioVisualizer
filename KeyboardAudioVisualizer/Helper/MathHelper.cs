using System;

namespace KeyboardAudioVisualizer.Helper
{
    public static class MathHelper
    {
        #region Methods

        public static double Clamp(double value, double min, double max) => Math.Max(min, Math.Min(max, value));
        public static float Clamp(float value, float min, float max) => (float)Clamp((double)value, min, max);

        #endregion
    }
}
