using CSCore;
using CSCore.SoundIn;
using CSCore.Streams;

namespace KeyboardAudioVisualizer.AudioCapture
{
    public class CSCoreAudioInput : IAudioInput
    {
        #region Properties & Fields

        private WasapiCapture _capture;
        private SoundInSource _soundInSource;
        private SingleBlockNotificationStream _stream;

        private readonly float[] _readBuffer = new float[2048];

        public int SampleRate => _soundInSource?.WaveFormat?.SampleRate ?? -1;

        #endregion

        #region Event

        public event AudioData DataAvailable;

        #endregion

        #region Methods

        public void Initialize()
        {
            _capture = new WasapiLoopbackCapture();
            _capture.Initialize();
            _soundInSource = new SoundInSource(_capture) { FillWithZeros = false };
            _stream = new SingleBlockNotificationStream(_soundInSource.ToStereo().ToSampleSource());

            _soundInSource.DataAvailable += OnSoundDataAvailable;

            _capture.Start();
        }

        public void Dispose()
        {
            _capture?.Stop();
            _capture?.Dispose();
        }

        private void OnSoundDataAvailable(object sender, DataAvailableEventArgs dataAvailableEventArgs)
        {
            int readCount;
            while ((readCount = _stream.Read(_readBuffer, 0, _readBuffer.Length)) > 0)
                DataAvailable?.Invoke(_readBuffer, 0, readCount);
        }

        #endregion
    }
}
