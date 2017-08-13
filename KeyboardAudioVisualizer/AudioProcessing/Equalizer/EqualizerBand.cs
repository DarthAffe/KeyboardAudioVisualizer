using KeyboardAudioVisualizer.Helper;
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
                    SetProperty(ref _offset, float.IsNaN(value) ? 0 : MathHelper.Clamp(value, 0, 1));
            }
        }

        private float _value;
        public float Value
        {
            get => _value;
            set => SetProperty(ref _value, float.IsNaN(value) ? 0 : MathHelper.Clamp(value, -1, 1));
        }

        public bool IsFixedOffset { get; set; }

        #endregion

        #region Constructors

        public EqualizerBand() : this(0) { }

        public EqualizerBand(float offset, float value = 0, bool fixedOffset = false)
        {
            this.Offset = offset;
            this.Value = value;
            this.IsFixedOffset = fixedOffset;
        }

        #endregion
    }
}
