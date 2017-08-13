using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace KeyboardAudioVisualizer.Converter
{
    public class OffsetToPosXConverter : IMultiValueConverter
    {
        #region Methods

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((values.Length != 2) || (parameter == DependencyProperty.UnsetValue)
                || (values[0] == null) || (values[0] == DependencyProperty.UnsetValue)
                || (values[1] == null) || (values[1] == DependencyProperty.UnsetValue))
                return 0;

            float offset = (float)values[0];
            double width = (double)values[1];
            double correction = double.Parse(parameter.ToString());

            return (offset * width) - correction;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();

        #endregion
    }
}
