using System;

namespace KeyboardAudioVisualizer.AudioProcessing
{
    public interface IAudioProcessor : IDisposable
    {
        bool IsActive { get; set; }

        void Initialize();
        void Update();
    }
}
