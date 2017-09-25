using KeyboardAudioVisualizer.AudioProcessing.VisualizationProvider;
using KeyboardAudioVisualizer.Configuration;

namespace KeyboardAudioVisualizer.Legacy
{
    public class Settings
    {
        #region General

        public double UpdateRate { get; set; } = 40.0;

        #endregion

        #region AudioProcessing

        public EqualizerConfiguration EqualizerConfiguration { get; set; } = new EqualizerConfiguration();

        public FrequencyBarsVisualizationProviderConfiguration FrequencyBarsVisualizationProviderConfiguration { get; set; } = new FrequencyBarsVisualizationProviderConfiguration();

        public LevelVisualizationProviderConfiguration LevelVisualizationProviderConfiguration { get; set; } = new LevelVisualizationProviderConfiguration();

        public BeatVisualizationProviderConfiguration BeatVisualizationProviderConfiguration { get; set; } = new BeatVisualizationProviderConfiguration();

        #endregion
    }
}
