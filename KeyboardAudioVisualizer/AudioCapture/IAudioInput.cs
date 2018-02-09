using System;

namespace KeyboardAudioVisualizer.AudioCapture
{
    public delegate void AudioData(float[] data, int offset, int count);

    public interface IAudioInput : IDisposable
    {
        int SampleRate { get; }
        float MasterVolume { get; }

        event AudioData DataAvailable;

        void Initialize();
    }
}
