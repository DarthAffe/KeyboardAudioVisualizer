using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KeyboardAudioVisualizer.Helper;

namespace KeyboardAudioVisualizer.AudioProcessing.Spectrum
{
    public abstract class AbstractSpectrum : ISpectrum
    {
        #region Properties & Fields

        protected Band[] Bands { get; set; }
        public int BandCount => Bands.Length;

        //TODO DarthAffe 10.08.2017: Check Frequency-Indexers - they are most likely wrong
        public Band this[int index] => Bands[index];
        public Band this[float frequency] => Bands[FrequencyHelper.GetIndexOfFrequency(frequency, BandCount)];
        public Band[] this[float minFrequency, float maxFrequency]
        {
            get
            {
                int minIndex = FrequencyHelper.GetIndexOfFrequency(minFrequency, BandCount);
                int maxIndex = FrequencyHelper.GetIndexOfFrequency(maxFrequency, BandCount);

                Band[] result = new Band[maxIndex - minIndex];
                Array.Copy(Bands, minIndex, result, 0, result.Length);
                return result;
            }
        }

        #endregion

        #region Methods

        public IEnumerator<Band> GetEnumerator() => Bands.AsEnumerable().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}
