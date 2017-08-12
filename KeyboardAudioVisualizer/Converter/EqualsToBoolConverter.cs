using System;
using System.Globalization;
using System.Windows.Data;

namespace KeyboardAudioVisualizer.Converter
{
    [ValueConversion(typeof(object), typeof(bool))]
    public class EqualsToBoolConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => Equals(value, parameter);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();

        #endregion
    }
}
