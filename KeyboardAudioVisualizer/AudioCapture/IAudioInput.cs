using CSCore.CoreAudioAPI;
using System;

namespace KeyboardAudioVisualizer.AudioCapture
{
    public delegate void AudioData(float left, float right);

    public interface IAudioInput : IDisposable
    {
        int SampleRate { get; }
        float MasterVolume { get; }

        event AudioData DataAvailable;

        void Initialize(MMDevice captureDevice);
    }
}
