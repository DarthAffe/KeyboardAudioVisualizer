using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace KeyboardAudioVisualizer.Converter
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (value as bool?) == true ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value as Visibility? == Visibility.Visible;

        #endregion
    }
}
