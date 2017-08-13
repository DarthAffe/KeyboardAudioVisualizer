using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace KeyboardAudioVisualizer.Converter
{
    public class ValueToPosYConverter : IMultiValueConverter
    {
        #region Methods

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((values.Length != 2) || (parameter == DependencyProperty.UnsetValue)
                || (values[0] == null) || (values[0] == DependencyProperty.UnsetValue)
                || (values[1] == null) || (values[1] == DependencyProperty.UnsetValue))
                return 0;

            float offset = (float)values[0];
            double height = (double)values[1];
            double correction = double.Parse(parameter.ToString());

            double halfHeight = height / 2.0;
            
            if (offset < 0)
                return (halfHeight + (-offset * halfHeight)) - correction;

            if (offset > 0)
                return (halfHeight - (offset * halfHeight)) - correction;

            return halfHeight - correction;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();

        #endregion
    }
}
