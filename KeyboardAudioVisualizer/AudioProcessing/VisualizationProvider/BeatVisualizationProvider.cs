using KeyboardAudioVisualizer.AudioProcessing.Spectrum;
using KeyboardAudioVisualizer.Configuration;
using KeyboardAudioVisualizer.Helper;

namespace KeyboardAudioVisualizer.AudioProcessing.VisualizationProvider
{
    #region Configuration

    public class BeatVisualizationProviderConfiguration : AbstractConfiguration
    {
        //TODO DarthAffe 12.08.2017: Check if there is something usefull to configure here
    }

    #endregion

    public class BeatVisualizationProvider : AbstractAudioProcessor, IVisualizationProvider
    {
        #region Properties & Fields

        private readonly BeatVisualizationProviderConfiguration _configuration;
        private readonly ISpectrumProvider _specturProvider;

        private RingBuffer[] _history;

        public IConfiguration Configuration => _configuration;
        public float[] VisualizationData { get; } = new float[1];

        #endregion

        #region Constructors

        public BeatVisualizationProvider(BeatVisualizationProviderConfiguration configuration, ISpectrumProvider specturProvider)
        {
            this._configuration = configuration;
            this._specturProvider = specturProvider;
        }

        #endregion

        #region Methods

        public override void Initialize()
        {
            _history = new RingBuffer[64];
            for (int i = 0; i < _history.Length; i++)
                _history[i] = new RingBuffer(32);
        }

        public override void Update()
        {
            VisualizationData[0] = 0;

            ISpectrum spectrum = _specturProvider.GetLogarithmicSpectrum(64);
            for (int i = 0; i < 32; i++)
            {
                float currentEnergy = spectrum[i].Average;
                float averageEnergy = _history[i].Average;
                _history[i].Put(currentEnergy);

                if (currentEnergy > (35 * averageEnergy))
                {
                    VisualizationData[0] = 1;
                    break;
                }
            }
        }

        #endregion
    }
}
