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
        private IWaveSource _source;
        private SingleBlockNotificationStream _stream;
        private AudioEndpointVolume _audioEndpointVolume;

        public int SampleRate => _soundInSource?.WaveFormat?.SampleRate ?? -1;
        public float MasterVolume => _audioEndpointVolume.MasterVolumeLevelScalar;

        #endregion

        #region Event

        public event AudioData DataAvailable;

        #endregion

        #region Methods

        public void Initialize(MMDevice captureDevice)
        {
            //BLARG 01.14.2020: Don't need the default when we're given an Audio Enpoint
            //MMDevice captureDevice = MMDeviceEnumerator.DefaultAudioEndpoint(DataFlow.Render, Role.Console);
            WaveFormat deviceFormat = captureDevice.DeviceFormat;
            _audioEndpointVolume = AudioEndpointVolume.FromDevice(captureDevice);

            //DarthAffe 07.02.2018: This is a really stupid workaround to (hopefully) finally fix the surround driver issues
            for (int i = 1; i < 13; i++)
                try { _capture = new WasapiLoopbackCapture(100, new WaveFormat(deviceFormat.SampleRate, deviceFormat.BitsPerSample, i)); } catch { /* We're just trying ... */ }

            if (_capture == null)
                throw new NullReferenceException("Failed to initialize WasapiLoopbackCapture");

            //BLARG: Actually setting the Device
            _capture.Device = captureDevice;
            _capture.Initialize();

            _soundInSource = new SoundInSource(_capture) { FillWithZeros = false };
            _source = _soundInSource.WaveFormat.SampleRate == 44100
                          ? _soundInSource.ToStereo()
                          : _soundInSource.ChangeSampleRate(44100).ToStereo();

            _stream = new SingleBlockNotificationStream(_source.ToSampleSource());
            _stream.SingleBlockRead += StreamOnSingleBlockRead;

            _source = _stream.ToWaveSource();

            byte[] buffer = new byte[_source.WaveFormat.BytesPerSecond / 2];
            _soundInSource.DataAvailable += (s, aEvent) =>
                                            {
                                                while ((_source.Read(buffer, 0, buffer.Length)) > 0) ;
                                            };

            _capture.Start();
        }

        //BLARG 01.14.2020: Added the rest of the disposables since we'll be calling this method a lot more
        public void Dispose()
        {
            //_capture?.Stop(); //Don't need this, Dispose() takes care of it
            _capture?.Dispose();
            _soundInSource?.Dispose();
            _source?.Dispose();
            _stream?.Dispose();
            _audioEndpointVolume?.Dispose();
        }

        private void StreamOnSingleBlockRead(object sender, SingleBlockReadEventArgs singleBlockReadEventArgs)
            => DataAvailable?.Invoke(singleBlockReadEventArgs.Left, singleBlockReadEventArgs.Right);

        #endregion
    }
}
