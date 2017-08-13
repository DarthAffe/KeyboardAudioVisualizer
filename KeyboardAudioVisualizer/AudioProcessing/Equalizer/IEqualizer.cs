using System.Collections.ObjectModel;

namespace KeyboardAudioVisualizer.AudioProcessing.Equalizer
{
    public interface IEqualizer
    {
        bool IsEnabled { get; set; }

        ObservableCollection<EqualizerBand> Bands { get; }
        
        float[] CalculateValues(int count);

        EqualizerBand AddBand(float offset, float modification);
        void RemoveBandBand(EqualizerBand band);

        void Reset();
    }
}
