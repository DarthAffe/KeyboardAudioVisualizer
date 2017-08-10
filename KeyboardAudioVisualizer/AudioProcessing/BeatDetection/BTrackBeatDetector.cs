using System;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;

// Based on https://github.com/adamstark/BTrack
namespace KeyboardAudioVisualizer.AudioProcessing.BeatDetection
{
    public class BeatDetectorBtrack : IAudioProcessor
    {
        #region Properties & Fields

        private readonly OnsetDetector _onsetDetector;

        private double _tightness;
        private double _alpha;
        private double _beatPeriod;
        private double _tempo;
        private double _estimatedTempo;
        private double _latestCumulativeScoreValue;
        private double _tempoToLagFactor;
        private int _m0;
        private int _beatCounter;
        private int _hopSize;
        private int _onsetDFBufferSize;
        private bool _tempoFixed;
        private bool _beatDueInFrame;
        private int _fftLengthForAcfCalculation;

        private CircularBuffer _onsetDf;
        private CircularBuffer _cumulativeScore;
        private readonly double[] _resampledOnsetDf = new double[512];
        private readonly double[] _acf = new double[512];
        private readonly double[] _weightingVector = new double[128];
        private readonly double[] _combFilterBankOutput = new double[128];
        private readonly double[] _tempoObservationVector = new double[41];
        private readonly double[] _delta = new double[41];
        private readonly double[] _prevDelta = new double[41];
        private readonly double[] _prevDeltaFixed = new double[41];
        private readonly double[][] _tempoTransitionMatrix = new double[41][];
        private Complex32[] _complexBuffer;

        public bool IsBeat => _beatDueInFrame;

        #endregion

        #region Constructors

        public BeatDetectorBtrack(OnsetDetector onsetDetector)
        {
            this._onsetDetector = onsetDetector;
        }

        #endregion

        #region Methods

        public void Initialize()
        {
            _tightness = 5;
            _alpha = 0.9;
            _tempo = 120;
            _estimatedTempo = 120.0;
            _tempoToLagFactor = (60.0 * _onsetDetector.SampleRate) / 512.0;
            _m0 = 10;
            _beatCounter = -1;
            _beatDueInFrame = false;
            _fftLengthForAcfCalculation = 1024;

            const double RAYPARAM = 43;
            for (int n = 0; n < 128; n++)
                _weightingVector[n] = ((double)n / Math.Pow(RAYPARAM, 2)) * Math.Exp((-1 * Math.Pow(-n, 2)) / (2 * Math.Pow(RAYPARAM, 2)));

            for (int i = 0; i < 41; i++)
                _prevDelta[i] = 1;

            const double M_SIG = (int)(41 / 8.0);
            for (int i = 0; i < 41; i++)
            {
                _tempoTransitionMatrix[i] = new double[41];
                for (int j = 0; j < 41; j++)
                {
                    double x = j + 1;
                    _tempoTransitionMatrix[i][j] = (1 / (M_SIG * Math.Sqrt(2 * Math.PI))) * Math.Exp((-1 * Math.Pow((x - (i + 1)), 2)) / (2 * Math.Pow(M_SIG, 2)));
                }
            }
            _tempoFixed = false;
            _latestCumulativeScoreValue = 0;

            _complexBuffer = new Complex32[_fftLengthForAcfCalculation];

            _onsetDf = new CircularBuffer();
            _cumulativeScore = new CircularBuffer();

            SetHopSize(_onsetDetector.HopSize);
        }

        private void SetHopSize(int hopSize)
        {
            _hopSize = hopSize;
            _onsetDFBufferSize = (512 * 512) / hopSize;
            _beatPeriod = Math.Round(60 / ((((double)hopSize) / _onsetDetector.SampleRate) * _tempo));
            _onsetDf.Resize(_onsetDFBufferSize);
            _cumulativeScore.Resize(_onsetDFBufferSize);
            for (int i = 0; i < _onsetDFBufferSize; i++)
            {
                _onsetDf[i] = 0;
                _cumulativeScore[i] = 0;
                if ((i % ((int)Math.Round(_beatPeriod))) == 0)
                    _onsetDf[i] = 1;
            }
        }

        public void Update()
        {
            ProcessOnsetDetectionFunctionSample(_onsetDetector.Onset);
        }

        private void ProcessOnsetDetectionFunctionSample(double newSample)
        {
            newSample = Math.Abs(newSample);
            newSample = newSample + 0.0001;

            _m0--;
            _beatCounter--;
            _beatDueInFrame = false;

            _onsetDf.Add(newSample);
            UpdateCumulativeScore(newSample);

            if (_m0 == 0)
                PredictBeat();

            if (_beatCounter == 0)
            {
                _beatDueInFrame = true;
                ResampleOnsetDetectionFunction();
                CalculateTempo();
            }
        }

        public void ResampleOnsetDetectionFunction()
        {
            for (int i = 0; i < _onsetDFBufferSize; i++)
                _resampledOnsetDf[i] = _onsetDf[i];

            //float[] output = new float[512];
            //float[] input = new float[_onsetDFBufferSize];

            //for (int i = 0; i < _onsetDFBufferSize; i++)
            //{
            //    input[i] = (float)_onsetDf[i];
            //}
            //double src_ratio = 512.0 / ((double)_onsetDFBufferSize);
            //int BUFFER_LEN = _onsetDFBufferSize;
            //int output_len;
            //SRC_DATA src_data;
            ////output_len = (int) floor (((double) BUFFER_LEN) * src_ratio) ;
            //output_len = 512;
            //src_data.data_in = input;
            //src_data.input_frames = BUFFER_LEN;
            //src_data.src_ratio = src_ratio;
            //src_data.data_out = output;
            //src_data.output_frames = output_len;
            //src_simple(&src_data, SRC_SINC_BEST_QUALITY, 1);

            //for (int i = 0; i < output_len; i++)
            //    _resampledOnsetDf[i] = (double)src_data.data_out[i];
        }

        private void CalculateTempo()
        {
            AdaptiveThreshold(_resampledOnsetDf);
            CalculateBalancedAcf(_resampledOnsetDf);
            CalculateOutputOfCombFilterBank();
            AdaptiveThreshold(_combFilterBankOutput);

            int t_index;
            int t_index2;
            // calculate tempo observation vector from beat period observation vector
            for (int i = 0; i < 41; i++)
            {
                t_index = (int)Math.Round(_tempoToLagFactor / ((double)((2 * i) + 80)));
                t_index2 = (int)Math.Round(_tempoToLagFactor / ((double)((4 * i) + 160)));
                _tempoObservationVector[i] = _combFilterBankOutput[t_index - 1] + _combFilterBankOutput[t_index2 - 1];
            }
            double maxval;
            double maxind;
            double curval;
            // if tempo is fixed then always use a fixed set of tempi as the previous observation probability function
            if (_tempoFixed)
            {
                for (int k = 0; k < 41; k++)
                {
                    _prevDelta[k] = _prevDeltaFixed[k];
                }
            }
            for (int j = 0; j < 41; j++)
            {
                maxval = -1;
                for (int i = 0; i < 41; i++)
                {
                    curval = _prevDelta[i] * _tempoTransitionMatrix[i][j];
                    if (curval > maxval)
                    {
                        maxval = curval;
                    }
                }
                _delta[j] = maxval * _tempoObservationVector[j];
            }
            NormalizeArray(_delta, 41);
            maxind = -1;
            maxval = -1;
            for (int j = 0; j < 41; j++)
            {
                if (_delta[j] > maxval)
                {
                    maxval = _delta[j];
                    maxind = j;
                }
                _prevDelta[j] = _delta[j];
            }
            _beatPeriod = Math.Round((60.0 * 44100.0) / (((2 * maxind) + 80) * ((double)_hopSize)));
            if (_beatPeriod > 0)
            {
                _estimatedTempo = 60.0 / ((((double)_hopSize) / 44100.0) * _beatPeriod);
            }
        }

        private void AdaptiveThreshold(double[] x)
        {
            int n = x.Length;
            int i = 0;
            int k, t = 0;
            double[] xThresh = new double[n];
            int p_post = 7;
            int p_pre = 8;
            t = Math.Min(x.Length, p_post); // what is smaller, p_post of df size. This is to avoid accessing outside of arrays
            // find threshold for first 't' samples, where a full average cannot be computed yet
            for (i = 0; i <= t; i++)
            {
                k = Math.Min((i + p_pre), n);
                xThresh[i] = CalculateMeanOfArray(x, 1, k);
            }
            // find threshold for bulk of samples across a moving average from [i-p_pre,i+p_post]
            for (i = t + 1; i < n - p_post; i++)
            {
                xThresh[i] = CalculateMeanOfArray(x, i - p_pre, i + p_post);
            }
            // for last few samples calculate threshold, again, not enough samples to do as above
            for (i = n - p_post; i < n; i++)
            {
                k = Math.Max((i - p_post), 1);
                xThresh[i] = CalculateMeanOfArray(x, k, n);
            }
            // subtract the threshold from the detection function and check that it is not less than 0
            for (i = 0; i < n; i++)
            {
                x[i] = x[i] - xThresh[i];
                if (x[i] < 0)
                {
                    x[i] = 0;
                }
            }
        }

        private void CalculateOutputOfCombFilterBank()
        {
            int numelem;
            for (int i = 0; i < 128; i++)
                _combFilterBankOutput[i] = 0;

            numelem = 4;
            for (int i = 2; i <= 127; i++) // max beat period
            {
                for (int a = 1; a <= numelem; a++) // number of comb elements
                {
                    for (int b = 1 - a; b <= a - 1; b++) // general state using normalisation of comb elements
                    {
                        _combFilterBankOutput[i - 1] = _combFilterBankOutput[i - 1] + (_acf[(a * i + b) - 1] * _weightingVector[i - 1]) / (2 * a - 1); // calculate value for comb filter row
                    }
                }
            }
        }

        private void CalculateBalancedAcf(double[] onsetDetectionFunction)
        {
            int onsetDetectionFunctionLength = 512;
            for (int i = 0; i < _fftLengthForAcfCalculation; i++)
            {
                if (i < onsetDetectionFunctionLength)
                    _complexBuffer[i] = new Complex32((float)onsetDetectionFunction[i], 0);
                else
                    _complexBuffer[i] = new Complex32(0, 0);
            }

            Fourier.Forward(_complexBuffer);

            for (int i = 0; i < _fftLengthForAcfCalculation; i++)
                _complexBuffer[i] = new Complex32((_complexBuffer[i].Real * _complexBuffer[i].Real) + (_complexBuffer[i].Imaginary * _complexBuffer[i].Imaginary), 0);

            Fourier.Inverse(_complexBuffer);

            double lag = 512;
            for (int i = 0; i < 512; i++)
            {
                double absValue = Math.Sqrt((_complexBuffer[i].Real * _complexBuffer[i].Real) + (_complexBuffer[i].Imaginary * _complexBuffer[i].Imaginary));

                _acf[i] = absValue / lag;
                _acf[i] /= 1024.0;
                lag = lag - 1.0;
            }
        }

        private double CalculateMeanOfArray(double[] array, int startIndex, int endIndex)
        {
            int i;
            double sum = 0;
            int length = endIndex - startIndex;
            // find sum
            for (i = startIndex; i < endIndex; i++)
            {
                sum = sum + array[i];
            }
            if (length > 0)
            {
                return sum / length; // average and return
            }
            else
            {
                return 0;
            }
        }

        private void NormalizeArray(double[] array, int n)
        {
            double sum = 0;
            for (int i = 0; i < n; i++)
            {
                if (array[i] > 0)
                {
                    sum = sum + array[i];
                }
            }
            if (sum > 0)
            {
                for (int i = 0; i < n; i++)
                {
                    array[i] = array[i] / sum;
                }
            }
        }

        private void UpdateCumulativeScore(double odfSample)
        {
            int start, end, winsize;
            double max;
            start = _onsetDFBufferSize - (int)Math.Round(2 * _beatPeriod);
            end = _onsetDFBufferSize - (int)Math.Round(_beatPeriod / 2);
            winsize = end - start + 1;
            double[] w1 = new double[winsize];
            double v = -2 * _beatPeriod;
            double wcumscore;
            // create window
            for (int i = 0; i < winsize; i++)
            {
                w1[i] = Math.Exp((-1 * Math.Pow(_tightness * Math.Log(-v / _beatPeriod), 2)) / 2);
                v = v + 1;
            }
            // calculate new cumulative score value
            max = 0;
            int n = 0;
            for (int i = start; i <= end; i++)
            {
                wcumscore = _cumulativeScore[i] * w1[n];
                if (wcumscore > max)
                {
                    max = wcumscore;
                }
                n++;
            }
            _latestCumulativeScoreValue = ((1 - _alpha) * odfSample) + (_alpha * max);
            _cumulativeScore.Add(_latestCumulativeScoreValue);
        }

        private void PredictBeat()
        {
            int windowSize = (int)_beatPeriod;
            double[] futureCumulativeScore = new double[_onsetDFBufferSize + windowSize];
            double[] w2 = new double[windowSize];
            // copy cumscore to first part of fcumscore
            for (int i = 0; i < _onsetDFBufferSize; i++)
            {
                futureCumulativeScore[i] = _cumulativeScore[i];
            }
            // create future window
            double v = 1;
            for (int i = 0; i < windowSize; i++)
            {
                w2[i] = Math.Exp((-1 * Math.Pow((v - (_beatPeriod / 2)), 2)) / (2 * Math.Pow((_beatPeriod / 2), 2)));
                v++;
            }
            // create past window
            v = -2 * _beatPeriod;
            int start = _onsetDFBufferSize - (int)Math.Round(2 * _beatPeriod);
            int end = _onsetDFBufferSize - (int)Math.Round(_beatPeriod / 2);
            int pastwinsize = end - start + 1;
            double[] w1 = new double[pastwinsize];
            for (int i = 0; i < pastwinsize; i++)
            {
                w1[i] = Math.Exp((-1 * Math.Pow(_tightness * Math.Log(-v / _beatPeriod), 2)) / 2);
                v = v + 1;
            }
            // calculate future cumulative score
            double max;
            int n;
            double wcumscore;
            for (int i = _onsetDFBufferSize; i < (_onsetDFBufferSize + windowSize); i++)
            {
                start = i - (int)Math.Round(2 * _beatPeriod);
                end = i - (int)Math.Round(_beatPeriod / 2);
                max = 0;
                n = 0;
                for (int k = start; k <= end; k++)
                {
                    wcumscore = futureCumulativeScore[k] * w1[n];
                    if (wcumscore > max)
                    {
                        max = wcumscore;
                    }
                    n++;
                }
                futureCumulativeScore[i] = max;
            }
            // predict beat
            max = 0;
            n = 0;
            for (int i = _onsetDFBufferSize; i < (_onsetDFBufferSize + windowSize); i++)
            {
                wcumscore = futureCumulativeScore[i] * w2[n];
                if (wcumscore > max)
                {
                    max = wcumscore;
                    _beatCounter = n;
                }
                n++;
            }
            // set next prediction time
            _m0 = _beatCounter + (int)Math.Round(_beatPeriod / 2);
        }

        #endregion
    }
}
