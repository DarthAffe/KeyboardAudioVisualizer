namespace KeyboardAudioVisualizer.AudioProcessing.BeatDetection
{
    public class CircularBuffer
    {
        #region Properties & Fields

        private int _writeIndex = 0;
        private double[] _buffer = new double[1];

        public double this[int index]
        {
            get => _buffer[(index + _writeIndex) % _buffer.Length];
            set => _buffer[(index + _writeIndex) % _buffer.Length] = value;
        }

        #endregion

        #region Methods

        public void Add(double value)
        {
            _buffer[_writeIndex] = value;
            _writeIndex = (_writeIndex + 1) % _buffer.Length;
        }

        public void Resize(int size)
        {
            _buffer = new double[size];
            _writeIndex = 0;
        }

        #endregion
    }
}
