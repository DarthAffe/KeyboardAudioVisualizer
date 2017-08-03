namespace KeyboardAudioVisualizer.AudioProcessing.VisualizationPRovider
{
    public interface IVisualizationProvider : IAudioProcessor
    {
        float[] VisualizationData { get; }
    }
}
