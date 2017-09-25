using System;
using System.Globalization;
using System.Windows.Data;
using KeyboardAudioVisualizer.Attributes;
using KeyboardAudioVisualizer.Helper;
using VisualizationType = KeyboardAudioVisualizer.AudioProcessing.VisualizationProvider.VisualizationType;

namespace KeyboardAudioVisualizer.Converter
{
    [ValueConversion(typeof(VisualizationType), typeof(string))]
    public class VisualizationProviderDisplayNameConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is VisualizationType visualizationType)) return null;
            return visualizationType.GetAttribute<DisplayNameAttribute>()?.DisplayName ?? visualizationType.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();

        #endregion
    }
}
