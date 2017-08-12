using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace KeyboardAudioVisualizer.AudioProcessing.Spectrum
{
    public abstract class AbstractSpectrum : ISpectrum
    {
        #region Properties & Fields

        protected Band[] Bands { get; set; }
        public int BandCount => Bands.Length;

        public Band this[int index] => Bands[index];
        public Band this[float frequency] => Bands.FirstOrDefault(band => (band.LowerFrequency <= frequency) && (band.UpperFrequency >= frequency));
        public Band[] this[float minFrequency, float maxFrequency] => Bands.Where(band => (band.LowerFrequency > minFrequency) && (band.UpperFrequency < maxFrequency)).ToArray();

        #endregion

        #region Methods

        public IEnumerator<Band> GetEnumerator() => Bands.AsEnumerable().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}
