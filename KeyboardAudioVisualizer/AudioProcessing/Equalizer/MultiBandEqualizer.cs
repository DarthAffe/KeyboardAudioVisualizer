using System.Collections.ObjectModel;
using System.Linq;

namespace KeyboardAudioVisualizer.AudioProcessing.Equalizer
{
    public class MultiBandEqualizer : IEqualizer
    {
        #region Properties & Fields

        public ObservableCollection<EqualizerBand> Bands { get; } = new ObservableCollection<EqualizerBand>();

        private float[] _values;

        public bool IsEnabled { get; set; } = true;

        #endregion

        #region Constructors

        public MultiBandEqualizer()
        {
            AddBand(0, 0, true);
            AddBand(1, 0, true);
        }

        #endregion

        #region Methods

        public void AddBand(float frequency, float modification, bool isFixedFrequency = false)
        {
            EqualizerBand band = new EqualizerBand(frequency, modification, isFixedFrequency);
            band.PropertyChanged += (sender, args) => InvalidateCache();
            Bands.Add(band);

            InvalidateCache();
        }

        public float[] CalculateValues(int values)
        {
            if ((_values == null) || (_values.Length != values))
            {
                _values = new float[values];
                RecalculateValues();
            }
            return _values;
        }

        private void RecalculateValues()
        {
            float width = _values.Length;
            for (int i = 0; i < _values.Length; i++)
            {
                float offset = (i / width);
                EqualizerBand bandBefore = Bands.Last(n => n.Offset <= offset);
                EqualizerBand bandAfter = Bands.First(n => n.Offset >= offset);
                offset = bandAfter.Offset <= 0 ? 0 : (offset - bandBefore.Offset) / (bandAfter.Offset - bandBefore.Offset);
                float value = (float)((3.0 * (offset * offset)) - (2.0 * (offset * offset * offset)));
                _values[i] = bandBefore.Value + (value * (bandAfter.Value - bandBefore.Value));
            }
        }

        private void InvalidateCache() => _values = null;

        #endregion
    }
}
