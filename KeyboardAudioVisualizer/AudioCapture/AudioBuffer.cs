using System;

namespace KeyboardAudioVisualizer.AudioCapture
{
    public class AudioBuffer
    {
        #region Properties & Fields

        private readonly int _capacity;
        private readonly float[] _bufferLeft;
        private readonly float[] _bufferRight;
        private int _currentIndex;

        public int Size => _capacity;

        public float? Prescale { get; set; } = null;

        #endregion

        #region Constructors

        public AudioBuffer(int capacity)
        {
            this._capacity = capacity;

            _bufferLeft = new float[capacity];
            _bufferRight = new float[capacity];
        }

        #endregion

        #region Methods

        public void Put(float left, float right)
        {
            _currentIndex++;
            if (_currentIndex >= _capacity) _currentIndex = 0;

            _bufferLeft[_currentIndex] = left;
            _bufferRight[_currentIndex] = right;
        }

        public void Put(float[] src, int offset, int count)
        {
            if ((count & 1) != 0) return; // we expect stereo-data to be an even amount of values

            if (count > _capacity)
            {
                offset += count - _capacity;
                count = _capacity;
            }

            for (int i = 0; i < count; i += 2)
            {
                _currentIndex++;
                if (_currentIndex >= _capacity) _currentIndex = 0;

                if (Prescale.HasValue)
                {
                    _bufferLeft[_currentIndex] = src[offset + i] / Prescale.Value;
                    _bufferRight[_currentIndex] = src[offset + i + 1] / Prescale.Value;
                }
                else
                {
                    _bufferLeft[_currentIndex] = src[offset + i];
                    _bufferRight[_currentIndex] = src[offset + i + 1];
                }
            }
        }

        public void CopyLeftInto(ref float[] data, int offset) => CopyLeftInto(ref data, offset, Math.Min(data.Length, _capacity));
        public void CopyLeftInto(ref float[] data, int offset, int count)
        {
            int bufferOffset = _capacity - count;
            for (int i = 0; i < count; i++)
                data[offset + i] = _bufferLeft[(_currentIndex + (bufferOffset + i)) % _capacity];
        }

        public void CopyRightInto(ref float[] data, int offset) => CopyRightInto(ref data, offset, Math.Min(data.Length, _capacity));
        public void CopyRightInto(ref float[] data, int offset, int count)
        {
            int bufferOffset = _capacity - count;
            for (int i = 0; i < count; i++)
                data[offset + i] = _bufferRight[(_currentIndex + (bufferOffset + i)) % _capacity];
        }

        public void CopyMixInto(ref float[] data, int offset) => CopyMixInto(ref data, offset, Math.Min(data.Length, _capacity));
        public void CopyMixInto(ref float[] data, int offset, int count)
        {
            int bufferOffset = _capacity - count;
            for (int i = 0; i < count; i++)
            {
                int index = (_currentIndex + (bufferOffset + i)) % _capacity;
                data[offset + i] = (_bufferLeft[index] + _bufferRight[index]) / 2f;
            }
        }

        #endregion
    }
}
