using System;
using System.Linq;
using KeyboardAudioVisualizer.AudioProcessing.Spectrum;
using KeyboardAudioVisualizer.Configuration;
using KeyboardAudioVisualizer.Helper;
using RGB.NET.Core;

namespace KeyboardAudioVisualizer.AudioProcessing.VisualizationProvider
{
    #region Configuration

    public class BeatVisualizationProviderConfiguration : AbstractConfiguration
    {
        //TODO DarthAffe 12.08.2017: Check if there is something usefull to configure here
    }

    #endregion

    // Port of https://github.com/kctess5/Processing-Beat-Detection
    public class BeatVisualizationProvider : AbstractAudioProcessor, IVisualizationProvider
    {
        #region Properties & Fields

        private readonly BeatVisualizationProviderConfiguration _configuration;
        private readonly ISpectrumProvider _specturProvider;

        private int _beatBands = 30; //Number of bands to montiter, higher for more accuracy, lower for speed
        private int _longTermAverageSamples = 60; //gets average volume over a period of time
        private int _shortTermAverageSamples = 1; //average volume over a shorter "instantanious" time
        private int _deltaArraySamples = 300; //number of energy deltas between long & short average to sum together
        private int _beatAverageSamples = 100;
        private int _beatCounterArraySamples = 400;
        private int _maxTime = 200;
        private float _predictiveInfluenceConstant = 0.1f;
        private float _predictiveInfluence;
        private int _cyclePerBeatIntensity;
        private float[][] _deltaArray;
        private float[][] _shortAverageArray;
        private float[][] _longAverageArray;
        private float[] _globalAverageArray;
        private int[] _beatCounterArray;
        private int[] _beatSpread;
        private int _beatCounterPosition;
        private int _cyclesPerBeat;
        private int _longPosition;
        private int _shortPosition;
        private int _deltaPosition;
        private int[] _count;
        private float[] _totalLong;
        private float[] _totalShort;
        private float[] _delta;
        private float[] _c;
        private int _beat;
        private int _beatCounter;
        private float[] _beatAverage;
        private float _totalBeat;
        private int _beatPosition;
        private float _totalGlobal;
        private float _threshold;
        private float _standardDeviation;

        public IConfiguration Configuration => _configuration;
        public float[] VisualizationData { get; } = new float[1];

        public string DisplayName => "Beat";
        public RGBDeviceType VisualizerFor => (RGBDeviceType)0xFF;

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
            _deltaArray = new float[_deltaArraySamples][];
            for (int i = 0; i < _deltaArray.Length; i++)
                _deltaArray[i] = new float[_beatBands];

            _shortAverageArray = new float[_shortTermAverageSamples][];
            for (int i = 0; i < _shortAverageArray.Length; i++)
                _shortAverageArray[i] = new float[_beatBands];

            _longAverageArray = new float[_longTermAverageSamples / _shortTermAverageSamples][];
            for (int i = 0; i < _longAverageArray.Length; i++)
                _longAverageArray[i] = new float[_beatBands];

            _globalAverageArray = new float[_longTermAverageSamples];
            _beatCounterArray = new int[_beatCounterArraySamples];
            _beatSpread = new int[_maxTime];
            _count = new int[_beatBands];
            _totalLong = new float[_beatBands];
            _totalShort = new float[_beatBands];
            _delta = new float[_beatBands];
            _c = new float[_beatBands]; //multiplier used to determain threshold
            _beatAverage = new float[_beatAverageSamples];
        }

        public override void Update()
        {
            ISpectrum spectrum = _specturProvider.GetLogarithmicSpectrum(60, minFrequency: 60);

            if (_shortPosition >= _shortTermAverageSamples) _shortPosition = 0; //Resets incremental variables
            if (_longPosition >= (_longTermAverageSamples / _shortTermAverageSamples)) _longPosition = 0;
            if (_deltaPosition >= _deltaArraySamples) _deltaPosition = 0;
            if (_beatPosition >= _beatAverageSamples) _beatPosition = 0;

            /////////////////////////////////////Calculate short and long term array averages///////////////////////////////////////////////////////////////////////////////////////////////////////////
            for (int i = 0; i < _beatBands; i++)
            {
                _shortAverageArray[_shortPosition][i] = spectrum[i].Average; //stores the average intensity between the freq. bounds to the short term array
                _totalLong[i] = 0;
                _totalShort[i] = 0;
                for (int j = 0; j < (_longTermAverageSamples / _shortTermAverageSamples); j++)
                    _totalLong[i] += _longAverageArray[j][i]; //adds up all the values in both of these arrays, for averaging
                for (int j = 0; j < _shortTermAverageSamples; j++)
                    _totalShort[i] += _shortAverageArray[j][i];
            }

            ///////////////////////////////////////////Find wideband frequency average intensity/////////////////////////////////////////////////////////////////////////////////////////////////////
            _totalGlobal = 0;
            _globalAverageArray[_longPosition] = spectrum[0, 2000].Average(x => x.Average);
            for (int j = 0; j < _longTermAverageSamples; j++)
                _totalGlobal += _globalAverageArray[j];
            _totalGlobal = _totalGlobal / _longTermAverageSamples;

            //////////////////////////////////Populate long term average array//////////////////////////////////////////////////////////////////////////////////////////////////////////////
            if ((_shortPosition % _shortTermAverageSamples) == 0)
            { //every time the short array is completely new it is added to long array
                for (int i = 0; i < _beatBands; i++)
                    _longAverageArray[_longPosition][i] = _totalShort[i]; //increases speed of program, but is the same as if each individual value was stored in long array
                _longPosition++;
            }

            /////////////////////////////////////////Find index of variation for each band///////////////////////////////////////////////////////////////////////////////////////////////////////
            for (int i = 0; i < _beatBands; i++)
            {
                _totalLong[i] = _totalLong[i] / (float)_longTermAverageSamples / (float)_shortTermAverageSamples;
                _delta[i] = 0;
                _deltaArray[_deltaPosition][i] = (float)Math.Pow(Math.Abs(_totalLong[i] - _totalShort[i]), 2);

                for (int j = 0; j < _deltaArraySamples; j++)
                    _delta[i] += _deltaArray[j][i];
                _delta[i] /= _deltaArraySamples;

                ///////////////////////////////////////////Find local beats/////////////////////////////////////////////////////////////////////////////////////////////////////
                _c[i] = (float)((1.3 + MathHelper.Clamp(Map(_delta[i], 0, 3000, 0, 0.4), 0, 0.4) //delta is usually bellow 2000
                        + Map(MathHelper.Clamp(Math.Pow(_totalLong[i], 0.5), 0, 6), 0, 20, 0.3, 0) //possibly comment this out, adds weight to the lower end
                        + Map(MathHelper.Clamp(_count[i], 0, 15), 0, 15, 1, 0))
                       - Map(MathHelper.Clamp(_count[i], 30, 200), 30, 200, 0, 0.75));

                if ((_cyclePerBeatIntensity / _standardDeviation) > 3.5)
                {
                    _predictiveInfluence = (float)(_predictiveInfluenceConstant * (1 - (Math.Cos(_beatCounter * (Math.PI * Math.PI)) / _cyclesPerBeat)));
                    _predictiveInfluence *= (float)Map(MathHelper.Clamp(_cyclePerBeatIntensity / _standardDeviation, 3.5, 20), 3.5, 15, 1, 6);
                    if (_cyclesPerBeat > 10)
                        _c[i] += _predictiveInfluence;
                }
            }
            _beat = 0;
            for (int i = 0; i < _beatBands; i++)
            {
                if ((_totalShort[i] > (_totalLong[i] * _c[i])) & (_count[i] > 7))
                { //If beat is detected
                    if ((_count[i] > 12) & (_count[i] < 200))
                    {
                        _beatCounterArray[_beatCounterPosition % _beatCounterArraySamples] = _count[i];
                        _beatCounterPosition++;
                    }
                    _count[i] = 0; //resets counter
                }
            }

            /////////////////////////////////////////Figure out # of beats, and average///////////////////////////////////////////////////////////////////////////////////////////////////////
            for (int i = 0; i < _beatBands; i++)
                if (_count[i] < 2)
                    _beat++; //If there has been a recent beat in a band add to the global beat value
            _beatAverage[_beatPosition] = _beat;

            for (int j = 0; j < _beatAverageSamples; j++)
                _totalBeat += _beatAverage[j];
            _totalBeat = _totalBeat / _beatAverageSamples;

            /////////////////////////////////////////////////find global beat///////////////////////////////////////////////////////////////////////////////////////////////
            _c[0] = (float)(3.25 + Map(MathHelper.Clamp(_beatCounter, 0, 5), 0, 5, 5, 0));
            if (_cyclesPerBeat > 10)
                _c[0] = (float)(_c[0] + (0.75 * (1 - (Math.Cos(_beatCounter * (Math.PI * Math.PI)) / _cyclesPerBeat))));

            _threshold = (float)MathHelper.Clamp((_c[0] * _totalBeat) + Map(MathHelper.Clamp(_totalGlobal, 0, 2), 0, 2, 4, 0), 5, 1000);

            if ((_beat > _threshold) & (_beatCounter > 5))
            {
                VisualizationData[0] = 1;
                _beatCounter = 0;
            }
            else
                VisualizationData[0] = 0;

            /////////////////////////////////////////////////////Calculate beat spreads///////////////////////////////////////////////////////////////////////////////////////////
            //average = beatCounterArraySamples/200 !!!
            for (int i = 0; i < _maxTime; i++)
                _beatSpread[i] = 0;

            for (int i = 0; i < _beatCounterArraySamples; i++)
                _beatSpread[_beatCounterArray[i]]++;

            _cyclesPerBeat = Mode(_beatCounterArray);
            if (_cyclesPerBeat < 20)
                _cyclesPerBeat *= 2;

            _cyclePerBeatIntensity = _beatSpread.Max();
            _standardDeviation = 0;

            for (int i = 0; i < _maxTime; i++)
                _standardDeviation += (float)Math.Pow((_beatCounterArraySamples / _maxTime) - _beatSpread[i], 2);

            _standardDeviation = (float)Math.Pow(_standardDeviation / _maxTime, 0.5);

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            _shortPosition++;
            _deltaPosition++;
            for (int i = 0; i < _beatBands; i++) _count[i]++;
            _beatCounter++;
            _beatPosition++;
        }

        private int Mode(int[] array)
        {
            int[] modeMap = new int[array.Length];
            int maxEl = array[0];
            int maxCount = 1;
            for (int i = 0; i < array.Length; i++)
            {
                int el = array[i];
                if (modeMap[el] == 0)
                    modeMap[el] = 1;
                else
                    modeMap[el]++;

                if (modeMap[el] > maxCount)
                {
                    maxEl = el;
                    maxCount = modeMap[el];
                }
            }
            return maxEl;
        }

        private static double Map(double value, double oldMin, double oldMax, double newMin, double newMax) => newMin + ((newMax - newMin) * ((value - oldMin) / (oldMax - oldMin)));

        #endregion
    }
}
