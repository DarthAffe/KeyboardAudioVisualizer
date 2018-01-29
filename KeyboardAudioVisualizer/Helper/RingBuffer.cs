using System.Linq;

namespace KeyboardAudioVisualizer.Helper
{
    public class RingBuffer
    {
        #region Properties & Fields

        private readonly int _capacity;
        private readonly float[] _buffer;
        private int _currentIndex;

        public int Size => _capacity;

        public float Average => _buffer.Average();
        public float Min => _buffer.Min();
        public float Max => _buffer.Max();
        public float Sum => _buffer.Sum();

        #endregion

        #region Constructors

        public RingBuffer(int capacity)
        {
            this._capacity = capacity;

            _buffer = new float[capacity];
        }

        #endregion

        #region Methods

        public void Put(float value) => Put(new[] { value }, 0, 1);

        public void Put(float[] src, int offset, int count)
        {
            lock (_buffer)
            {
                if (count > _capacity)
                {
                    offset += count - _capacity;
                    count = _capacity;
                }

                for (int i = 0; i < count; i++)
                {
                    _currentIndex++;
                    if (_currentIndex >= _capacity) _currentIndex = 0;

                    _buffer[_currentIndex] = src[offset + i];
                }
            }
        }


        public void CopyInto(ref float[] data, int offset) => CopyInto(ref data, offset, _capacity);
        public void CopyInto(ref float[] data, int offset, int count)
        {
            lock (_buffer)
                for (int i = _capacity - count; i < count; i++)
                    data[offset + i] = _buffer[(_currentIndex + i) % _capacity];
        }

        #endregion
    }
}
