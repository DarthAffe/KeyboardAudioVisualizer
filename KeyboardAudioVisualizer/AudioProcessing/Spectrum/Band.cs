using System.Linq;

namespace KeyboardAudioVisualizer.AudioProcessing.Spectrum
{
    public class Band
    {
        #region Properties & Fields

        private readonly float[] _data;
        private readonly float _resolution;

        public float LowerFrequency { get; }
        public float UpperFrequency { get; }

        private float? _average = null;
        public float Average => _average ?? (_average = _data.Average()).Value;

        private float? _min = null;
        public float Min => _min ?? (_min = _data.Min()).Value;

        private float? _max = null;
        public float Max => _max ?? (_max = _data.Max()).Value;

        private float? _sum = null;
        public float Sum => _sum ?? (_sum = _data.Sum()).Value;

        public float this[int index] => _data[index];
        public float this[float frequency] => _data[(int)((frequency - LowerFrequency) / _resolution)];

        #endregion

        #region Constructors

        public Band(float lowerFrequency, float upperFrequency, float[] data)
        {
            this.LowerFrequency = lowerFrequency;
            this.UpperFrequency = upperFrequency;
            this._data = data;

            _resolution = (UpperFrequency - LowerFrequency) / data.Length;
        }

        #endregion
    }
}
