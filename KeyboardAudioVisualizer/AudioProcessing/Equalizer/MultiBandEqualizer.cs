using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RGB.NET.Core;

namespace KeyboardAudioVisualizer.AudioProcessing.Equalizer
{
    public class MultiBandEqualizer : AbstractBindable, IEqualizer
    {
        #region Properties & Fields

        public ObservableCollection<EqualizerBand> Bands { get; } = new ObservableCollection<EqualizerBand>();

        private readonly Dictionary<int, float[]> _values = new Dictionary<int, float[]>();

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        #endregion

        #region Constructors

        public MultiBandEqualizer()
        {
            Reset();
        }

        #endregion

        #region Methods

        public EqualizerBand AddBand(float offset, float modification) => AddBand(offset, modification, false);

        public EqualizerBand AddBand(float offset, float modification, bool isFixedFrequency)
        {
            EqualizerBand band = new EqualizerBand(offset, modification, isFixedFrequency);
            band.PropertyChanged += (sender, args) => InvalidateCache();
            Bands.Add(band);

            InvalidateCache();

            return band;
        }

        public void RemoveBandBand(EqualizerBand band)
        {
            if (!band.IsFixedOffset)
                Bands.Remove(band);

            InvalidateCache();
        }

        public void Reset()
        {
            Bands.Clear();
            AddBand(0, 0, true);
            AddBand(1, 0, true);
        }

        public float[] CalculateValues(int count)
        {
            if (!_values.TryGetValue(count, out float[] values))
            {
                values = RecalculateValues(count);
                _values[count] = values;
            }
            return values;
        }

        private float[] RecalculateValues(int count)
        {
            float[] values = new float[count];

            List<EqualizerBand> orderedBands = Bands.OrderBy(x => x.Offset).ToList();
            if (orderedBands.Count < 2) return values;

            for (int i = 0; i < count; i++)
            {
                float offset = (i / (float)count);
                EqualizerBand bandBefore = orderedBands.Last(n => n.Offset <= offset);
                EqualizerBand bandAfter = orderedBands.First(n => n.Offset >= offset);
                offset = (bandAfter.Offset <= 0) || (Math.Abs(bandAfter.Offset - bandBefore.Offset) < 0.0001)
                    ? 0 : (offset - bandBefore.Offset) / (bandAfter.Offset - bandBefore.Offset);
                float value = (float)((3.0 * (offset * offset)) - (2.0 * (offset * offset * offset)));
                values[i] = bandBefore.Value + (value * (bandAfter.Value - bandBefore.Value));
            }

            return values;
        }

        private void InvalidateCache()
        {
            _values.Clear();

            // ReSharper disable once ExplicitCallerInfoArgument
            OnPropertyChanged(nameof(Bands));
        }

        #endregion
    }
}
