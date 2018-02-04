using System;
using System.Collections.Generic;
using KeyboardAudioVisualizer.AudioProcessing.VisualizationProvider;
using KeyboardAudioVisualizer.Helper;
using RGB.NET.Brushes.Gradients;
using RGB.NET.Core;

namespace KeyboardAudioVisualizer.Configuration
{
    public class Settings
    {
        #region Constants

        public const int CURRENT_VERSION = 1;

        #endregion

        #region Properties & Fields

        public int Version { get; set; } = 0;

        public double UpdateRate { get; set; } = 40.0;

        public Dictionary<VisualizationIndex, VisualizationSettings> Visualizations { get; set; } = new Dictionary<VisualizationIndex, VisualizationSettings>();

        public VisualizationSettings this[VisualizationIndex visualizationIndex]
        {
            get
            {
                if (!Visualizations.TryGetValue(visualizationIndex, out VisualizationSettings settings))
                    Visualizations[visualizationIndex] = (settings = new VisualizationSettings(visualizationIndex));
                return settings;
            }
        }

        #endregion
    }

    public class VisualizationSettings
    {
        #region Properties & Fields

        public VisualizationType SelectedVisualization { get; set; }

        public LinearGradient Gradient { get; set; }

        public EqualizerConfiguration EqualizerConfiguration { get; set; } = new EqualizerConfiguration();

        public FrequencyBarsVisualizationProviderConfiguration FrequencyBarsConfiguration { get; set; } = new FrequencyBarsVisualizationProviderConfiguration();
        public LevelVisualizationProviderConfiguration LevelConfiguration { get; set; } = new LevelVisualizationProviderConfiguration();
        public BeatVisualizationProviderConfiguration BeatConfiguration { get; set; } = new BeatVisualizationProviderConfiguration();

        public IConfiguration this[VisualizationType visualizationType]
        {
            get
            {
                switch (visualizationType)
                {
                    case VisualizationType.None:
                        return null;

                    case VisualizationType.FrequencyBars:
                        return FrequencyBarsConfiguration;

                    case VisualizationType.Level:
                        return LevelConfiguration;

                    case VisualizationType.Beat:
                        return BeatConfiguration;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(visualizationType), visualizationType, null);
                }
            }
        }

        #endregion

        #region Constructors

        public VisualizationSettings(VisualizationIndex visualizationIndex)
        {
            switch (visualizationIndex)
            {
                case VisualizationIndex.Primary:
                    SelectedVisualization = VisualizationType.FrequencyBars;
                    Gradient = new LinearGradient(new GradientStop(0, Color.FromHSV(300, 1, 1)),
                                                  new GradientStop(0.20, Color.FromHSV(225, 1, 1)),
                                                  new GradientStop(0.35, Color.FromHSV(180, 1, 1)),
                                                  new GradientStop(0.50, Color.FromHSV(135, 1, 1)),
                                                  new GradientStop(0.65, Color.FromHSV(90, 1, 1)),
                                                  new GradientStop(0.80, Color.FromHSV(45, 1, 1)),
                                                  new GradientStop(0.95, Color.FromHSV(0, 1, 1)));
                    break;

                case VisualizationIndex.Secondary:
                    SelectedVisualization = VisualizationType.Beat;
                    Gradient = new LinearGradient(new GradientStop(0.5, new Color(255, 255, 255)));
                    break;

                case VisualizationIndex.Tertiary:
                    SelectedVisualization = VisualizationType.Level;
                    Gradient = new LinearGradient(new GradientStop(0, new Color(0, 0, 255)),
                                                  new GradientStop(1, new Color(255, 0, 0)));
                    break;
            }
        }

        #endregion

        #region Methods

        public T GetConfiguration<T>(VisualizationType visualizationType)
            where T : IConfiguration, new() => (T)this[visualizationType];

        #endregion
    }
}
