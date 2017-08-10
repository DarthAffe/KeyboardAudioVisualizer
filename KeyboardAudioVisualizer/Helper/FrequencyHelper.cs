using System;

namespace KeyboardAudioVisualizer.Helper
{
    public static class FrequencyHelper
    {
        #region Constants

        private const int SAMPLE_RATE = 44100; // Right now this is fixed

        #endregion

        #region Methods

        public static float GetFrequencyOfIndex(int index, int count) => index * ((float)SAMPLE_RATE / count);
        public static int GetIndexOfFrequency(float frequency, int count) => (int)(frequency / ((float)SAMPLE_RATE / count));

        public static double CalculatedBAForFrequency(float frequency)
        {
            double ra = (Math.Pow(12194, 2) * Math.Pow(frequency, 4)) / ((Math.Pow(frequency, 2) + Math.Pow(20.6, 2)) * Math.Sqrt((Math.Pow(frequency, 2) + Math.Pow(107.7, 2)) * (Math.Pow(frequency, 2) + Math.Pow(737.9, 2))) * (Math.Pow(frequency, 2) + Math.Pow(12194, 2)));
            return (20 * Math.Log10(ra)) + 2;
        }

        #endregion
    }
}
