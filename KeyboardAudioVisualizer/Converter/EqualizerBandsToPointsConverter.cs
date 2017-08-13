using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using KeyboardAudioVisualizer.AudioProcessing.Equalizer;

namespace KeyboardAudioVisualizer.Converter
{
    public class EqualizerBandsToPointsConverter : IMultiValueConverter
    {
        #region Methods

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            PointCollection points = new PointCollection();

            if ((values.Length != 4) || (parameter == DependencyProperty.UnsetValue)
                || (values[0] == null) || (values[0] == DependencyProperty.UnsetValue)
                || (values[1] == null) || (values[1] == DependencyProperty.UnsetValue) //HACK DarthAffe 13.08.2017: I need this only to update the binding
                || (values[2] == null) || (values[2] == DependencyProperty.UnsetValue)
                || (values[3] == null) || (values[3] == DependencyProperty.UnsetValue))
                return points;

            IEqualizer equalizer = (IEqualizer)values[0];
            double width = (double)values[2];
            double height = (double)values[3];
            int valueCount = int.Parse(parameter.ToString());
            double halfHeight = height / 2.0;

            List<(float offset, float value)> pointValues = equalizer.Bands.Select(b => (b.Offset, b.Value)).ToList();
            float[] calculatedValues = equalizer.CalculateValues(valueCount);
            for (int i = 0; i < calculatedValues.Length; i++)
                pointValues.Add(((float)i / calculatedValues.Length, calculatedValues[i]));

            foreach ((float offset, float value) in pointValues.OrderBy(x => x.offset))
                points.Add(new Point(offset * width, GetPosY(value, halfHeight)));

            return points;
        }

        private double GetPosY(float offset, double halfHeight)
        {
            if (offset < 0)
                return halfHeight + (-offset * halfHeight);

            if (offset > 0)
                return halfHeight - (offset * halfHeight);

            return halfHeight;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();

        #endregion
    }
}
