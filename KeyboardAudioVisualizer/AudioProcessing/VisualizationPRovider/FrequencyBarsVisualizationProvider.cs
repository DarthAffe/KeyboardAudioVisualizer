using System;
using System.Linq;
using KeyboardAudioVisualizer.AudioProcessing.Equalizer;
using KeyboardAudioVisualizer.AudioProcessing.Spectrum;
using KeyboardAudioVisualizer.Configuration;
using KeyboardAudioVisualizer.Helper;

namespace KeyboardAudioVisualizer.AudioProcessing.VisualizationPRovider
{
    #region Configuration

    public class FrequencyBarsVisualizationProviderConfiguration : AbstractConfiguration
    {
        private int _bars = 48;
        public int Bars
        {
            get => _bars;
            set => SetProperty(ref _bars, value);
        }

        private double _smoothing = 8;
        public double Smoothing
        {
            get => _smoothing;
            set => SetProperty(ref _smoothing, value);
        }

        private double _minFrequency = 100;
        public double MinFrequency
        {
            get => _minFrequency;
            set => SetProperty(ref _minFrequency, value);
        }

        private double _maxFrequency = 15000;
        public double MaxFrequency
        {
            get => _maxFrequency;
            set => SetProperty(ref _maxFrequency, value);
        }

        private double _scale = 20;
        public double Scale
        {
            get => _scale;
            set => SetProperty(ref _scale, value);
        }

        private int _emphasisePeaks = 1;
        public int EmphasisePeaks
        {
            get => _emphasisePeaks;
            set => SetProperty(ref _emphasisePeaks, value);
        }

        private double _gamma = 3;
        public double Gamma
        {
            get => _gamma;
            set => SetProperty(ref _gamma, value);
        }
    }

    #endregion

    public class FrequencyBarsVisualizationProvider : IVisualizationProvider
    {
        #region Properties & Fields

        private readonly FrequencyBarsVisualizationProviderConfiguration _configuration;
        private readonly ISpectrumProvider _spectrumProvider;

        private int _frequencySkipCount;
        private int _frequencyCount;
        private double _smoothingFactor;
        private double _scalingValue;

        public IEqualizer Equalizer { get; set; }
        public float[] VisualizationData { get; private set; }

        #endregion

        #region Constructors

        public FrequencyBarsVisualizationProvider(FrequencyBarsVisualizationProviderConfiguration configuration, ISpectrumProvider spectrumProvider)
        {
            this._configuration = configuration;
            this._spectrumProvider = spectrumProvider;

            configuration.PropertyChanged += (sender, args) => RecalculateConfigValues(args.PropertyName);
        }

        #endregion

        #region Methods

        public void Initialize() => RecalculateConfigValues(null);

        private void RecalculateConfigValues(string changedPropertyName)
        {
            if ((changedPropertyName == null) || (changedPropertyName == nameof(FrequencyBarsVisualizationProviderConfiguration.Bars)))
                VisualizationData = new float[_configuration.Bars];

            if ((changedPropertyName == null) || (changedPropertyName == nameof(FrequencyBarsVisualizationProviderConfiguration.Smoothing)))
                _smoothingFactor = Math.Pow((0.0000025 * Math.Pow(2, _configuration.Smoothing)), ((double)_spectrumProvider.Spectrum.Length * 2) / (double)_spectrumProvider.SampleRate);

            if ((changedPropertyName == null)
                || (changedPropertyName == nameof(FrequencyBarsVisualizationProviderConfiguration.MinFrequency))
                || (changedPropertyName == nameof(FrequencyBarsVisualizationProviderConfiguration.MaxFrequency)))
                CalculateFrequencyCount(MathHelper.Clamp(_configuration.MinFrequency, 0, _spectrumProvider.SampleRate / 2.0),
                                        MathHelper.Clamp(_configuration.MaxFrequency, 0, _spectrumProvider.SampleRate / 2.0),
                                        _spectrumProvider.SampleRate, _spectrumProvider.Spectrum.Length * 2);

            if ((changedPropertyName == null) || (changedPropertyName == nameof(FrequencyBarsVisualizationProviderConfiguration.Scale)))
                _scalingValue = _configuration.Scale * 0.0001;
        }

        private void CalculateFrequencyCount(double minFrequency, double maxFrequency, int sampleRate, int sampleSize)
        {
            int firstFrequency = Math.Max(0, (int)Math.Ceiling((minFrequency / sampleRate) * sampleSize) - 1);
            int lastFrequency = Math.Max(firstFrequency, (int)Math.Ceiling((maxFrequency / sampleRate) * sampleSize));

            if (firstFrequency == lastFrequency)
            {
                if (firstFrequency == 0) lastFrequency++;
                else firstFrequency--;
            }

            _frequencyCount = lastFrequency - firstFrequency;
            _frequencySkipCount = firstFrequency;
        }

        public void Update()
        {
            float[] spectrum = _spectrumProvider.Spectrum.Skip(_frequencySkipCount).Take(_frequencyCount).ToArray();

            int startFrequency = 0;
            for (int i = 0; i < VisualizationData.Length; i++)
            {
                int endFrequency = Math.Max(startFrequency + 1, Math.Min(_frequencyCount, (int)Math.Round(Math.Pow((i + 1f) / VisualizationData.Length, _configuration.Gamma) * _frequencyCount)));
                int bandWidth = endFrequency - startFrequency;
                double binPower = 0;

                for (int j = 0; j < bandWidth; j++)
                {
                    float power = spectrum[Math.Min(spectrum.Length - 1, startFrequency + j)];
                    binPower = Math.Max(power, binPower);
                }

                if (Equalizer?.IsEnabled == true)
                {
                    float value = Equalizer.CalculateValues(VisualizationData.Length)[i];
                    if (Math.Abs(value) > 0.000001)
                    {
                        bool lower = value < 0;
                        value = 1 + (value * value);
                        binPower *= lower ? 1f / value : value;
                    }
                }

                binPower = Math.Log(binPower);
                binPower = Math.Max(0, binPower);

                if (_configuration.EmphasisePeaks == 1)
                {
                    binPower *= binPower;
                    binPower *= 0.15;
                }
                if (_configuration.EmphasisePeaks == 2)
                    binPower *= binPower * binPower;
                else
                    binPower *= 40;

                VisualizationData[i] = (float)((VisualizationData[i] * _smoothingFactor) + (binPower * _scalingValue * (1.0 - _smoothingFactor)));
                if (double.IsNaN(VisualizationData[i])) VisualizationData[i] = 0;

                startFrequency = endFrequency;
            }
        }

        #endregion
    }
}
