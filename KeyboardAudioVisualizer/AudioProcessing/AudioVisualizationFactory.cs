using System;
using System.Collections.Generic;
using System.Linq;
using CSCore.CoreAudioAPI;
using KeyboardAudioVisualizer.AudioCapture;
using KeyboardAudioVisualizer.AudioProcessing.Equalizer;
using KeyboardAudioVisualizer.AudioProcessing.Spectrum;
using KeyboardAudioVisualizer.AudioProcessing.VisualizationProvider;
using KeyboardAudioVisualizer.Helper;

namespace KeyboardAudioVisualizer.AudioProcessing
{
    public class AudioVisualizationFactory : IDisposable
    {
        #region Properties & Fields

        public static AudioVisualizationFactory Instance { get; private set; }

        private IAudioInput _audioInput;
        private AudioBuffer _audioBuffer;
        private readonly List<IAudioProcessor> _processors = new List<IAudioProcessor>();

        #endregion

        #region Constructors

        private AudioVisualizationFactory() { }

        #endregion

        #region Methods

        public void Update()
        {
            if (ApplicationManager.Instance.Settings.EnableAudioPrescale)
                _audioBuffer.Prescale = _audioInput.MasterVolume;
            else
                _audioBuffer.Prescale = null;

            foreach (IAudioProcessor processor in _processors.Where(x => x.IsActive))
                processor.Update();
        }

        public static void Initialize(MMDevice captureDevice)
        {
            if (Instance != null) return;

            Instance = new AudioVisualizationFactory();
            Instance.InitializeInstance(captureDevice);
        }

        private void InitializeInstance(MMDevice captureDevice)
        {
            _audioInput = new CSCoreAudioInput();
            _audioInput.Initialize(captureDevice);

            _audioBuffer = new AudioBuffer(4096); // Working with ~93ms - 
            _audioInput.DataAvailable += (left, right) => _audioBuffer.Put(left, right);

            _processors.Add(new FourierSpectrumProvider(_audioBuffer));

            foreach (IAudioProcessor processor in _processors)
                processor.Initialize();
        }

        //BLARG 01.14.2020: Added a method to change the Audio device without 1. Restarting the application 2. Crashing the Application 3. The Visualization Stopping
        public void ChangeAudioDevice(MMDevice newDevice)
        {
            _audioInput.Dispose();
            _audioInput.Initialize(newDevice);
        }

        private T GetAudioProcessor<T>() => (T)_processors.FirstOrDefault(x => x.GetType() == typeof(T));

        public IVisualizationProvider CreateVisualizationProvider(VisualizationIndex visualizationIndex, VisualizationType visualizationType)
        {
            IVisualizationProvider visualizationProvider = default;
            switch (visualizationType)
            {
                case VisualizationType.FrequencyBars:
                    MultiBandEqualizer equalizer = new MultiBandEqualizer();
                    ApplicationManager.Instance.Settings[visualizationIndex].EqualizerConfiguration.LoadInto(equalizer);
                    equalizer.PropertyChanged += (sender, args) => ApplicationManager.Instance.Settings[visualizationIndex].EqualizerConfiguration.SaveFrom(equalizer);
                    visualizationProvider = new FrequencyBarsVisualizationProvider(ApplicationManager.Instance.Settings[visualizationIndex].GetConfiguration<FrequencyBarsVisualizationProviderConfiguration>(visualizationType), GetAudioProcessor<FourierSpectrumProvider>()) { Equalizer = equalizer };
                    break;

                case VisualizationType.Level:
                    visualizationProvider = new LevelVisualizationProvider(ApplicationManager.Instance.Settings[visualizationIndex].GetConfiguration<LevelVisualizationProviderConfiguration>(visualizationType), _audioBuffer);
                    break;

                case VisualizationType.Beat:
                    visualizationProvider = new BeatVisualizationProvider(ApplicationManager.Instance.Settings[visualizationIndex].GetConfiguration<BeatVisualizationProviderConfiguration>(visualizationType), GetAudioProcessor<FourierSpectrumProvider>());
                    break;
            }

            visualizationProvider?.Initialize();
            return visualizationProvider;
        }

        public void Dispose() => _audioInput.Dispose();

        #endregion
    }
}
