using System;
using System.Linq;
using KeyboardAudioVisualizer.AudioCapture;
using KeyboardAudioVisualizer.Configuration;
using KeyboardAudioVisualizer.Helper;

namespace KeyboardAudioVisualizer.AudioProcessing.VisualizationProvider
{
    #region Configuration

    public enum ConversionMode
    {
        Linear, Logarithmic, Exponential
    }

    public class LevelVisualizationProviderConfiguration : AbstractConfiguration
    {
        private double _smoothing = 8;
        public double Smoothing
        {
            get => _smoothing;
            set => SetProperty(ref _smoothing, value);
        }

        private double _scale = 4;
        public double Scale
        {
            get => _scale;
            set => SetProperty(ref _scale, value);
        }

        private ConversionMode _conversionMode = ConversionMode.Exponential;
        public ConversionMode ConversionMode
        {
            get => _conversionMode;
            set => SetProperty(ref _conversionMode, value);
        }
    }

    #endregion

    public class LevelVisualizationProvider : IVisualizationProvider
    {
        #region Constants

        private const int DB_THRESHOLD = 90;

        #endregion

        #region Properties & Fields

        private readonly LevelVisualizationProviderConfiguration _configuration;
        private readonly AudioBuffer _audioBuffer;

        private float[] _sampleDataLeft;
        private float[] _sampleDataRight;
        private float[] _sampleDataMix;
        private double _smoothingFactor;

        public IConfiguration Configuration => _configuration;
        public float[] VisualizationData { get; } = new float[3];

        #endregion

        #region Constructors

        public LevelVisualizationProvider(LevelVisualizationProviderConfiguration configuration, AudioBuffer audioBuffer)
        {
            this._configuration = configuration;
            this._audioBuffer = audioBuffer;

            configuration.PropertyChanged += (sender, args) => RecalculateConfigValues(args.PropertyName);
        }

        #endregion

        #region Methods

        public void Initialize()
        {
            _sampleDataLeft = new float[_audioBuffer.Size];
            _sampleDataRight = new float[_audioBuffer.Size];
            _sampleDataMix = new float[_audioBuffer.Size];

            RecalculateConfigValues(null);
        }

        private void RecalculateConfigValues(string changedPropertyName)
        {
            if ((changedPropertyName == null) || (changedPropertyName == nameof(LevelVisualizationProviderConfiguration.Smoothing)))
                _smoothingFactor = MathHelper.Clamp(0.000015 * Math.Pow(2, _configuration.Smoothing), 0, 0.99);
        }

        public void Update()
        {
            _audioBuffer.CopyLeftInto(ref _sampleDataLeft, 0);
            _audioBuffer.CopyRightInto(ref _sampleDataRight, 0);
            _audioBuffer.CopyMixInto(ref _sampleDataMix, 0);

            float levelLeft = Convert(GetRms(ref _sampleDataLeft));
            float levelRight = Convert(GetRms(ref _sampleDataRight));
            float levelMix = Convert(GetRms(ref _sampleDataMix));

            UpdateData(0, levelLeft);
            UpdateData(1, levelRight);
            UpdateData(2, levelMix);
        }

        private float GetRms(ref float[] data) => (float)Math.Sqrt(data.Average(x => x * x));

        private float Convert(float level)
        {
            switch (_configuration.ConversionMode)
            {
                case ConversionMode.Exponential:
                    return level * level;

                case ConversionMode.Logarithmic:
                    return (float)Math.Max(0, (DB_THRESHOLD + (Math.Log10(level) * 20)) / DB_THRESHOLD);

                default: return level;
            }
        }

        private void UpdateData(int index, float level)
        {
            VisualizationData[index] = (float)((VisualizationData[index] * _smoothingFactor) + (level * _configuration.Scale * (1.0 - _smoothingFactor)));
            if (double.IsNaN(VisualizationData[index])) VisualizationData[index] = 0;
        }

        #endregion
    }
}
