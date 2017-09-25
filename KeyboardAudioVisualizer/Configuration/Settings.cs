using System;
using System.Collections.Generic;
using KeyboardAudioVisualizer.AudioProcessing.VisualizationProvider;
using KeyboardAudioVisualizer.Helper;

namespace KeyboardAudioVisualizer.Configuration
{
    public class Settings
    {
        #region Properties & Fields

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
                    break;

                case VisualizationIndex.Secondary:
                    SelectedVisualization = VisualizationType.Beat;
                    break;

                case VisualizationIndex.Tertiary:
                    SelectedVisualization = VisualizationType.Level;
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
