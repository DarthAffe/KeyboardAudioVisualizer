using System;
using System.Collections.Generic;
using System.Linq;
using KeyboardAudioVisualizer.AudioProcessing.Equalizer;

namespace KeyboardAudioVisualizer.Configuration
{
    public class EqualizerConfiguration : AbstractConfiguration
    {
        #region Properties & Fields

        public bool IsEnabled { get; set; } = false;

        public List<EqualizerBand> Bands { get; set; } = new List<EqualizerBand>();

        #endregion

        #region Methods

        public void LoadInto(IEqualizer equalizer)
        {
            equalizer.IsEnabled = IsEnabled;

            foreach (EqualizerBand band in Bands)
            {
                if (band.IsFixedOffset)
                {
                    EqualizerBand bandToUpdate = equalizer.Bands.FirstOrDefault(b => b.IsFixedOffset && (Math.Abs(b.Offset - band.Offset) < 0.01));
                    if (bandToUpdate != null)
                        bandToUpdate.Value = band.Value;
                }
                else
                    equalizer.AddBand(band.Offset, band.Value);
            }
        }

        public void SaveFrom(IEqualizer equalizer)
        {
            IsEnabled = equalizer.IsEnabled;

            Bands.Clear();
            foreach (EqualizerBand band in equalizer.Bands)
                Bands.Add(new EqualizerBand(band.Offset, band.Value, band.IsFixedOffset));
        }

        #endregion
    }
}
