using System;
using System.Collections.Generic;
using System.Linq;

namespace KeyboardAudioVisualizer.AudioProcessing.Equalizer
{
    public class MultiBandEqualizer : IEqualizer
    {
        #region Properties & Fields

        private float[] _values;

        private readonly List<Band> _bands = new List<Band>();

        public int Bands => _bands.Count;
        public float this[int band]
        {
            get => _bands[band].Value;
            set
            {
                _bands[band].Value = value;
                RecalculateValues();
            }
        }

        public bool IsEnabled { get; set; } = true;

        #endregion

        #region Constructors

        public MultiBandEqualizer(int bands = 5)
        {
            if (bands < 2) throw new ArgumentOutOfRangeException(nameof(bands), "There must be at least two bands for an working equalizer!");

            float reference = (float)Math.Log(bands);

            for (int i = bands - 1; i >= 0; i--)
            {
                Band band = new Band((reference - (float)Math.Log(i + 1)) / reference);
                _bands.Add(band);
            }

            CalculateValues(1);
        }

        #endregion

        #region Methods

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

                Band bandBefore = _bands.Last(n => n.Offset <= offset);
                Band bandAfter = _bands.First(n => n.Offset >= offset);

                offset = bandAfter.Offset <= 0 ? 0 : (offset - bandBefore.Offset) / (bandAfter.Offset - bandBefore.Offset);

                float value = (float)((3.0 * (offset * offset)) - (2.0 * (offset * offset * offset)));
                _values[i] = bandBefore.Value + (value * (bandAfter.Value - bandBefore.Value));
            }
        }

        #endregion

        #region Data

        private class Band
        {
            #region Properties & Fields

            public float Offset { get; set; }
            public float Value { get; set; }

            #endregion

            #region Constructors

            public Band(float offset, float value = 0)
            {
                this.Offset = offset;
                this.Value = value;
            }

            #endregion
        }

        #endregion
    }
}
