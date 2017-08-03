namespace KeyboardAudioVisualizer.AudioProcessing.Equalizer
{
    public interface IEqualizer
    {
        bool IsEnabled { get; set; }
        float[] CalculateValues(int values);
    }
}
