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

        private float[] _spectrum;
        public float[] Spectrum => _spectrum;

        public int SampleRate { get; private set; }
        public float Resolution { get; private set; }

        #endregion

        #region Constructors

        public FourierSpectrumProvider(AudioBuffer audioBuffer, int sampleRate)
        {
            this._audioBuffer = audioBuffer;
            this.SampleRate = sampleRate;
        }

        #endregion

        #region Methods

        public void Initialize()
        {
            _hamming = Window.Hamming(_audioBuffer.Size);
            _sampleData = new float[_audioBuffer.Size];
            _spectrum = new float[_audioBuffer.Size / 2];
            Resolution = (float)SampleRate / (float)_audioBuffer.Size;
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
            Complex32[] complexData = CreateComplexData(ref data);
            Fourier.Forward(complexData, FourierOptions.NoScaling);
            for (int i = 0; i < _spectrum.Length; i++)
            {
                Complex32 fourierData = complexData[i];
                _spectrum[i] = (fourierData.Real * fourierData.Real) + (fourierData.Imaginary * fourierData.Imaginary);
            }
        }

        private static Complex32[] CreateComplexData(ref float[] data)
        {
            Complex32[] complexData = new Complex32[data.Length];
            for (int i = 0; i < data.Length; i++)
                complexData[i] = new Complex32(data[i], 0);

            return complexData;
        }

        #endregion   
    }
}
