using System;
using KeyboardAudioVisualizer.AudioCapture;
using KeyboardAudioVisualizer.AudioProcessing.Equalizer;
using KeyboardAudioVisualizer.AudioProcessing.Spectrum;
using KeyboardAudioVisualizer.AudioProcessing.VisualizationProvider;

namespace KeyboardAudioVisualizer.AudioProcessing
{
    public class AudioProcessor : IDisposable
    {
        #region Constants

        private const int MAXIMUM_UPDATE_RATE = 40; // We won't allow to change the FPS beyond this

        #endregion

        #region Properties & Fields

        public static AudioProcessor Instance { get; private set; }

        private AudioBuffer _audioBuffer;
        private IAudioInput _audioInput;
        private ISpectrumProvider _spectrumProvider;

        public IVisualizationProvider PrimaryVisualizationProvider { get; private set; }
        public IVisualizationProvider SecondaryVisualizationProvider { get; private set; }

        #endregion

        #region Constructors

        private AudioProcessor() { }

        #endregion

        #region Methods

        public void Update()
        {
            _spectrumProvider.Update();
            PrimaryVisualizationProvider.Update();
            SecondaryVisualizationProvider.Update();
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
            MultiBandEqualizer equalizer = new MultiBandEqualizer { [0] = -3, [1] = -1, [2] = 1, [3] = 2, [4] = 3 };
            PrimaryVisualizationProvider = new FrequencyBarsVisualizationProvider(new FrequencyBarsVisualizationProviderConfiguration(), _spectrumProvider) { Equalizer = equalizer };
            //PrimaryVisualizationProvider = new BeatVisualizationProvider(new BeatVisualizationProviderConfiguration(), _spectrumProvider);
            PrimaryVisualizationProvider.Initialize();

            SecondaryVisualizationProvider = new LevelVisualizationProvider(new LevelVisualizationProviderConfiguration(), _audioBuffer);
            SecondaryVisualizationProvider.Initialize();
        }

        public void Dispose() => _audioInput.Dispose();

        #endregion
    }
}
