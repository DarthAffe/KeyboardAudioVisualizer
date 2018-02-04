using KeyboardAudioVisualizer.Helper;
using RGB.NET.Brushes.Gradients;
using RGB.NET.Core;

namespace KeyboardAudioVisualizer.Legacy
{
    public static class ConfigurationUpdates
    {
        #region Methods

        public static void PerformOn(Configuration.Settings settings)
        {
            if (settings.Version < 1)
                UpdateTo1(settings);
        }

        private static void UpdateTo1(Configuration.Settings settings)
        {
            settings.Visualizations[VisualizationIndex.Primary].Gradient = new LinearGradient(new GradientStop(0, Color.FromHSV(300, 1, 1)),
                                                                                              new GradientStop(0.20, Color.FromHSV(225, 1, 1)),
                                                                                              new GradientStop(0.35, Color.FromHSV(180, 1, 1)),
                                                                                              new GradientStop(0.50, Color.FromHSV(135, 1, 1)),
                                                                                              new GradientStop(0.65, Color.FromHSV(90, 1, 1)),
                                                                                              new GradientStop(0.80, Color.FromHSV(45, 1, 1)),
                                                                                              new GradientStop(0.95, Color.FromHSV(0, 1, 1)));

            settings.Visualizations[VisualizationIndex.Secondary].Gradient = new LinearGradient(new GradientStop(0.5, new Color(255, 255, 255)));

            settings.Visualizations[VisualizationIndex.Tertiary].Gradient = new LinearGradient(new GradientStop(0, new Color(0, 0, 255)),
                                                                                               new GradientStop(1, new Color(255, 0, 0)));

            settings.Version = 1;
        }

        #endregion
    }
}
