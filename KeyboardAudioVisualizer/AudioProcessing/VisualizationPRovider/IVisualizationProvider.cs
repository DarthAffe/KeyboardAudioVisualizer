using KeyboardAudioVisualizer.Configuration;

namespace KeyboardAudioVisualizer.AudioProcessing.VisualizationProvider
{
    public interface IVisualizationProvider : IAudioProcessor
    {
        IConfiguration Configuration { get; }
        float[] VisualizationData { get; }
    }
}
