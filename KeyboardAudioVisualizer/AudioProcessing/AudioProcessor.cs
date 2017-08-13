using System;
using KeyboardAudioVisualizer.AudioCapture;
using KeyboardAudioVisualizer.AudioProcessing.Equalizer;
using KeyboardAudioVisualizer.AudioProcessing.Spectrum;
using KeyboardAudioVisualizer.AudioProcessing.VisualizationProvider;

namespace KeyboardAudioVisualizer.AudioProcessing
{
    public class AudioProcessor : IDisposable
    {
        #region Properties & Fields

        public static AudioProcessor Instance { get; private set; }

        private AudioBuffer _audioBuffer;
        private IAudioInput _audioInput;
        private ISpectrumProvider _spectrumProvider;

        public IVisualizationProvider PrimaryVisualizationProvider { get; private set; }
        public IVisualizationProvider SecondaryVisualizationProvider { get; private set; }
        public IVisualizationProvider TertiaryVisualizationProvider { get; private set; }

        #endregion

        #region Constructors

        private AudioProcessor() { }

        #endregion

        #region Methods

        public void Update()
        {
            _spectrumProvider.Update();

            PrimaryVisualizationProvider?.Update();
            SecondaryVisualizationProvider?.Update();
            TertiaryVisualizationProvider?.Update();
        }

        public static void Initialize()
        {
            if (Instance != null) return;

            Instance = new AudioProcessor();
            Instance.InitializeInstance();
        }

        private void InitializeInstance()
        {
            _audioInput = new CSCoreAudioInput();
            _audioInput.Initialize();

            _audioBuffer = new AudioBuffer(4096); // Working with ~93ms - 
            _audioInput.DataAvailable += (data, offset, count) => _audioBuffer.Put(data, offset, count);

            _spectrumProvider = new FourierSpectrumProvider(_audioBuffer);
            _spectrumProvider.Initialize();

            //TODO DarthAffe 03.08.2017: Initialize correctly; Settings
            MultiBandEqualizer equalizer = new MultiBandEqualizer { IsEnabled = false };
            PrimaryVisualizationProvider = new FrequencyBarsVisualizationProvider(new FrequencyBarsVisualizationProviderConfiguration(), _spectrumProvider) { Equalizer = equalizer };
            //PrimaryVisualizationProvider = new BeatVisualizationProvider(new BeatVisualizationProviderConfiguration(), _spectrumProvider);
            PrimaryVisualizationProvider.Initialize();

            SecondaryVisualizationProvider = new BeatVisualizationProvider(new BeatVisualizationProviderConfiguration(), _spectrumProvider);
            SecondaryVisualizationProvider.Initialize();

            TertiaryVisualizationProvider = new LevelVisualizationProvider(new LevelVisualizationProviderConfiguration(), _audioBuffer);
            TertiaryVisualizationProvider.Initialize();
        }

        public void Dispose() => _audioInput.Dispose();

        #endregion
    }
}
