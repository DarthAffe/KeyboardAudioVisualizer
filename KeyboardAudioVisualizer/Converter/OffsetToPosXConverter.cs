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
            if ((values.Length != 3) || (values[0] == null) || (values[0] == DependencyProperty.UnsetValue)
                                     || (values[1] == null) || (values[1] == DependencyProperty.UnsetValue)
                                     || (values[2] == null) || (values[2] == DependencyProperty.UnsetValue))
                return 0;

            float val = (float)values[0];
            double height = (double)values[1];
            double reference = (double)values[2];

            double halfHeight = height / 2.0;

            double offset = val / reference;
            if (offset < 0)
                return halfHeight + (-offset * halfHeight);

            if (offset > 0)
                return halfHeight - (offset * halfHeight);

            return halfHeight;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();

        #endregion
    }
}
