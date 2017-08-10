using System;
using KeyboardAudioVisualizer.AudioCapture;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;

namespace KeyboardAudioVisualizer.AudioProcessing.BeatDetection
{
    public class OnsetDetector : IAudioProcessor
    {
        #region Properties & Fields

        private readonly AudioBuffer _audioBuffer;
        public int SampleRate => _audioBuffer.Size;
        public int HopSize => _hopSize;
        public double Onset { get; private set; }

        private int _frameSize;
        private int _hopSize;
        private double _prevEnergySum;

        private float[] _dataBuffer;
        private double[] _frame;
        private double[] _window;
        private double[] _magSpec;
        private double[] _prevMagSpec;
        private double[] _phase;
        private double[] _prevPhase;
        private double[] _prevPhase2;
        private Complex32[] _complexBuffer;

        #endregion

        #region Constructors

        public OnsetDetector(AudioBuffer audioBuffer)
        {
            this._audioBuffer = audioBuffer;
        }

        #endregion

        #region Methods

        public void Initialize()
        {
            _hopSize = 512;
            _frameSize = SampleRate;

            _dataBuffer = new float[_audioBuffer.Size];
            _frame = new double[_frameSize];
            _window = new double[_frameSize];
            _magSpec = new double[_frameSize];
            _prevMagSpec = new double[_frameSize];
            _phase = new double[_frameSize];
            _prevPhase = new double[_frameSize];
            _prevPhase2 = new double[_frameSize];
            _complexBuffer = new Complex32[_frameSize];

            _window = Window.Hann(_frameSize);

            for (int i = 0; i < _frameSize; i++)
            {
                _prevMagSpec[i] = 0.0;
                _prevPhase[i] = 0.0;
                _prevPhase2[i] = 0.0;
                _frame[i] = 0.0;
            }
            _prevEnergySum = 0.0;
        }

        public void Update()
        {
            _audioBuffer.CopyMixInto(ref _dataBuffer, 0);
            Onset = CalculateOnsetDetectionFunctionSample(_dataBuffer);
        }

        private double CalculateOnsetDetectionFunctionSample(float[] buffer)
        {
            for (int i = 0; i < (_frameSize - _hopSize); i++)
            {
                _frame[i] = _frame[i + _hopSize];
            }

            int j = 0;
            for (int i = (_frameSize - _hopSize); i < _frameSize; i++)
            {
                _frame[i] = buffer[j];
                j++;
            }

            return ComplexSpectralDifferenceHWR();
        }

        private double ComplexSpectralDifferenceHWR()
        {
            double phaseDeviation;
            double sum;
            double magnitudeDifference;
            double csd;

            PerformFFT();

            sum = 0;
            for (int i = 0; i < _frameSize; i++)
            {
                // calculate phase value
                _phase[i] = Math.Atan2(_complexBuffer[i].Imaginary, _complexBuffer[i].Real);
                // calculate magnitude value
                _magSpec[i] = Math.Sqrt(Math.Pow(_complexBuffer[i].Real, 2) + Math.Pow(_complexBuffer[i].Imaginary, 2));
                // phase deviation
                phaseDeviation = _phase[i] - (2 * _prevPhase[i]) + _prevPhase2[i];
                // calculate magnitude difference (real part of Euclidean distance between complex frames)
                magnitudeDifference = _magSpec[i] - _prevMagSpec[i];
                // if we have a positive change in magnitude, then include in sum, otherwise ignore (half-wave rectification)
                if (magnitudeDifference > 0)
                {
                    // calculate complex spectral difference for the current spectral bin
                    csd = Math.Sqrt(Math.Pow(_magSpec[i], 2) + Math.Pow(_prevMagSpec[i], 2) - 2 * _magSpec[i] * _prevMagSpec[i] * Math.Cos(phaseDeviation));
                    // add to sum
                    sum = sum + csd;
                }
                // store values for next calculation
                _prevPhase2[i] = _prevPhase[i];
                _prevPhase[i] = _phase[i];
                _prevMagSpec[i] = _magSpec[i];
            }
            return sum;
        }

        private void PerformFFT()
        {
            int fsize2 = _frameSize / 2;

            for (int i = 0; i < fsize2; i++)
            {
                _complexBuffer[i] = new Complex32((float)(_frame[i + fsize2] * _window[i + fsize2]), 0);
                _complexBuffer[i + fsize2] = new Complex32((float)(_frame[i] * _window[i]), 0);
            }

            Fourier.Forward(_complexBuffer);
        }

        #endregion
    }
}
