using KeyboardAudioVisualizer.Configuration;

namespace KeyboardAudioVisualizer.AudioProcessing.VisualizationProvider
{
    public interface IVisualizationProvider
    {
        IConfiguration Configuration { get; }
        float[] VisualizationData { get; }

        void Initialize();
        void Update();
    }
}
