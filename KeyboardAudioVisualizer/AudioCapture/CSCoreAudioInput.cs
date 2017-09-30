using CSCore;
using CSCore.CoreAudioAPI;
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
            MMDevice captureDevice = MMDeviceEnumerator.DefaultAudioEndpoint(DataFlow.Render, Role.Console);
            WaveFormat deviceFormat = captureDevice.DeviceFormat;
            _capture = new WasapiLoopbackCapture(100, new WaveFormat(deviceFormat.SampleRate, deviceFormat.BitsPerSample, deviceFormat.Channels <= 2 ? deviceFormat.Channels : 2));

            //_capture = new WasapiLoopbackCapture();
            _capture.Initialize();
            _soundInSource = new SoundInSource(_capture) { FillWithZeros = false };

            _stream = _soundInSource.WaveFormat.SampleRate == 44100
                ? new SingleBlockNotificationStream(_soundInSource.ToStereo().ToSampleSource())
                : new SingleBlockNotificationStream(_soundInSource.ChangeSampleRate(44100).ToStereo().ToSampleSource());

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
