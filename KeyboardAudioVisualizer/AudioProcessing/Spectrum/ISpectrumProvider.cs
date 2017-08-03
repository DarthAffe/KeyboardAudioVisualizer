namespace KeyboardAudioVisualizer.AudioProcessing.Spectrum
{
    public interface ISpectrumProvider : IAudioProcessor
    {
        float[] Spectrum { get; }
        int SampleRate { get; }
        float Resolution { get; }
    }
}
