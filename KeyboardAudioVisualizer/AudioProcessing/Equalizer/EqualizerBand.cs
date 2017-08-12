using RGB.NET.Core;

namespace KeyboardAudioVisualizer.AudioProcessing.Equalizer
{
    public class EqualizerBand : AbstractBindable
    {
        #region Properties & Fields

        private float _offset;
        public float Offset
        {
            get => _offset;
            set
            {
                if (!IsFixedOffset)
                    SetProperty(ref _offset, value);
            }
        }

        private float _value;
        public float Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public bool IsFixedOffset { get; }

        #endregion

        #region Constructors

        public EqualizerBand(float offset, float value = 0, bool fixedOffset = false)
        {
            this.Offset = offset;
            this.Value = value;
            this.IsFixedOffset = fixedOffset;
        }

        #endregion
    }
}
