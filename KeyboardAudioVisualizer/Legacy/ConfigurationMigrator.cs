using System.IO;
using KeyboardAudioVisualizer.AudioProcessing.VisualizationProvider;
using KeyboardAudioVisualizer.Helper;

namespace KeyboardAudioVisualizer.Legacy
{
    public static class ConfigurationMigrator
    {
        #region Constants

        private const string PATH_V1_SETTINGS = "Settings.xml";

        #endregion

        #region Constructors

        #endregion

        #region Methods

        public static Configuration.Settings MigrateOldConfig()
        {
            if (!File.Exists(PATH_V1_SETTINGS)) return null;

            try
            {
                Settings oldSettings = SerializationHelper.LoadObjectFromFile<Settings>(PATH_V1_SETTINGS);
                Configuration.Settings settings = new Configuration.Settings { UpdateRate = oldSettings.UpdateRate };

                settings[VisualizationIndex.Primary].SelectedVisualization = VisualizationType.FrequencyBars;
                settings[VisualizationIndex.Primary].FrequencyBarsConfiguration = oldSettings.FrequencyBarsVisualizationProviderConfiguration;
                settings[VisualizationIndex.Primary].EqualizerConfiguration = oldSettings.EqualizerConfiguration;

                settings[VisualizationIndex.Secondary].SelectedVisualization = VisualizationType.Beat;
                settings[VisualizationIndex.Secondary].BeatConfiguration = oldSettings.BeatVisualizationProviderConfiguration;

                settings[VisualizationIndex.Tertiary].SelectedVisualization = VisualizationType.Level;
                settings[VisualizationIndex.Tertiary].LevelConfiguration = oldSettings.LevelVisualizationProviderConfiguration;

                return settings;
            }
            catch
            {
                return null;
            }
        }

        public static void CleanupOldConfigs()
        {
            if (File.Exists(PATH_V1_SETTINGS))
                File.Delete(PATH_V1_SETTINGS);
        }

        #endregion
    }
}
