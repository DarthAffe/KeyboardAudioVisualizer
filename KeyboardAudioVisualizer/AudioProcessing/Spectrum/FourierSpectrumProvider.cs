using System;
using KeyboardAudioVisualizer.AudioCapture;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;

namespace KeyboardAudioVisualizer.AudioProcessing.Spectrum
{
    public class FourierSpectrumProvider : ISpectrumProvider
    {
        #region Properties & Fields

        private readonly AudioBuffer _audioBuffer;

        private float[] _sampleData;
        private double[] _hamming;
        private Complex32[] _complexBuffer;

        private float[] _spectrum;
        private int _usableDataLength;

        #endregion

        #region Constructors

        public FourierSpectrumProvider(AudioBuffer audioBuffer)
        {
            this._audioBuffer = audioBuffer;
        }

        #endregion

        #region Methods

        public void Initialize()
        {
            _hamming = Window.Hamming(_audioBuffer.Size);
            _sampleData = new float[_audioBuffer.Size];
            _complexBuffer = new Complex32[_audioBuffer.Size];
            _usableDataLength = (_audioBuffer.Size / 2) + 1;
            _spectrum = new float[_usableDataLength];
        }

        public void Update()
        {
            _audioBuffer.CopyMixInto(ref _sampleData, 0);

            ApplyHamming(ref _sampleData);
            CreateSpectrum(ref _sampleData);
        }

        private void ApplyHamming(ref float[] data)
        {
            for (int i = 0; i < data.Length; i++)
                data[i] = (float)(data[i] * _hamming[i]);
        }

        private void CreateSpectrum(ref float[] data)
        {
            for (int i = 0; i < data.Length; i++)
                _complexBuffer[i] = new Complex32(data[i], 0);

            Fourier.Forward(_complexBuffer, FourierOptions.NoScaling);

            for (int i = 0; i < _spectrum.Length; i++)
            {
                Complex32 fourierData = _complexBuffer[i];
                _spectrum[i] = (float)Math.Sqrt(fourierData.Real * fourierData.Real) + (fourierData.Imaginary * fourierData.Imaginary);
            }
        }

        public ISpectrum GetLinearSpectrum(int bands = 64, float minFrequency = -1, float maxFrequency = -1) => new LinearSpectrum(_spectrum, bands, minFrequency, maxFrequency);

        public ISpectrum GetLogarithmicSpectrum(int bands = 12, float minFrequency = -1, float maxFrequency = -1) => new LogarithmicSpectrum(_spectrum, bands, minFrequency, maxFrequency);

        public ISpectrum GetGammaSpectrum(int bands = 64, float gamma = 2, float minFrequency = -1, float maxFrequency = -1) => new GammaSpectrum(_spectrum, bands, gamma, minFrequency, maxFrequency);

        public ISpectrum GetRawSpectrum() => new RawSpectrumProvider(_spectrum);

        #endregion
    }
}
