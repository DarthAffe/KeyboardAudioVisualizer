using System;
using KeyboardAudioVisualizer.AudioProcessing.Spectrum;
using KeyboardAudioVisualizer.Configuration;

namespace KeyboardAudioVisualizer.AudioProcessing.VisualizationProvider
{
    #region Configuration

    public class BeatVisualizationProviderConfiguration : AbstractConfiguration
    {
    }

    #endregion

    public class BeatVisualizationProvider : IVisualizationProvider
    {
        public IConfiguration Configuration { get; }
        public float[] VisualizationData { get; }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            throw new NotImplementedException();
        }
    }
}
