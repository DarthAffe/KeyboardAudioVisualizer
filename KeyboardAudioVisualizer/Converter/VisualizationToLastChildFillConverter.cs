using System;
using System.Globalization;
using System.Windows.Data;
using KeyboardAudioVisualizer.AudioProcessing.VisualizationProvider;
using KeyboardAudioVisualizer.UI.Visualization;

namespace KeyboardAudioVisualizer.Converter
{
    [ValueConversion(typeof(IVisualizationProvider), typeof(bool))]
    public class VisualizationToLastChildFillConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is FrequencyBarsVisualizationProvider;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();

        #endregion
    }
}
