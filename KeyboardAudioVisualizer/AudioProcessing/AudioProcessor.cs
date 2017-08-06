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

            _audioBuffer = new AudioBuffer(CalculateSampleSize(_audioInput.SampleRate, MAXIMUM_UPDATE_RATE));
            _audioInput.DataAvailable += (data, offset, count) => _audioBuffer.Put(data, offset, count);

            _spectrumProvider = new FourierSpectrumProvider(_audioBuffer, _audioInput.SampleRate);
            _spectrumProvider.Initialize();

            //TODO DarthAffe 03.08.2017: Initialize correctly; Settings
            MultiBandEqualizer equalizer = new MultiBandEqualizer { [0] = -3, [1] = -1, [2] = -1, [3] = 1, [4] = 3 };
            PrimaryVisualizationProvider = new FrequencyBarsVisualizationProvider(new FrequencyBarsVisualizationProviderConfiguration { Scale = 34 }, _spectrumProvider) { Equalizer = equalizer };
            PrimaryVisualizationProvider.Initialize();

            SecondaryVisualizationProvider = new LevelVisualizationProvider(new LevelVisualizationProviderConfiguration(), _audioBuffer);
            SecondaryVisualizationProvider.Initialize();
        }

        private int CalculateSampleSize(int sampleRate, int maximumUpdateRate)
        {
            int sampleSize = 2;
            while ((sampleSize * maximumUpdateRate) < sampleRate)
                sampleSize <<= 1;

            return sampleSize;
        }

        public void Dispose() => _audioInput.Dispose();

        #endregion
    }
}
