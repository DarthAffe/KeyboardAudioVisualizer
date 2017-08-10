namespace KeyboardAudioVisualizer.AudioProcessing.Spectrum
{
    public interface ISpectrumProvider : IAudioProcessor
    {
        ISpectrum GetLinearSpectrum(int bands = 64, float minFrequency = -1, float maxFrequency = -1);
        ISpectrum GetLogarithmicSpectrum(int bands = 1, float minFrequency = -1, float maxFrequency = -1);
        ISpectrum GetGammaSpectrum(int bands = 1, float gamma = 2, float minFrequency = -1, float maxFrequency = -1);
        ISpectrum GetRawSpectrum();
    }
}
