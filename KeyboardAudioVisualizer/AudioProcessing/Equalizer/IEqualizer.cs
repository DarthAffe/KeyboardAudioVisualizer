using System.Collections.ObjectModel;

namespace KeyboardAudioVisualizer.AudioProcessing.Equalizer
{
    public interface IEqualizer
    {
        bool IsEnabled { get; set; }

        ObservableCollection<EqualizerBand> Bands { get; }
        
        float[] CalculateValues(int count);
    }
}
