using System;
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
        private AudioEndpointVolume _audioEndpointVolume;

        private readonly float[] _readBuffer = new float[2048];

        public int SampleRate => _soundInSource?.WaveFormat?.SampleRate ?? -1;
        public float MasterVolume => _audioEndpointVolume.MasterVolumeLevelScalar;

        #endregion

        #region Event

        public event AudioData DataAvailable;

        #endregion

        #region Methods

        public void Initialize()
        {
            MMDevice captureDevice = MMDeviceEnumerator.DefaultAudioEndpoint(DataFlow.Render, Role.Console);
            WaveFormat deviceFormat = captureDevice.DeviceFormat;
            _audioEndpointVolume = AudioEndpointVolume.FromDevice(captureDevice);

            //DarthAffe 07.02.2018: This is a really stupid workaround to (hopefully) finally fix the surround driver issues
            for (int i = 1; i < 13; i++)
            {
                try
                {
                    _capture = new WasapiLoopbackCapture(100, new WaveFormat(deviceFormat.SampleRate, deviceFormat.BitsPerSample, i));
                }
                catch
                { }
            }

            if (_capture == null)
                throw new NullReferenceException("Failed to initialize WasapiLoopbackCapture");

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
