using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using KeyboardAudioVisualizer.Attributes;
using KeyboardAudioVisualizer.Helper;
using RGB.NET.Core;
using VisualizationType = KeyboardAudioVisualizer.AudioProcessing.VisualizationProvider.VisualizationType;

namespace KeyboardAudioVisualizer.Converter
{
    [ValueConversion(typeof(IEnumerable<VisualizationType>), typeof(IEnumerable<VisualizationType>))]
    public class VisualizationTypeSelectableConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is IEnumerable<VisualizationType> visualizationProviders) || !(parameter is RGBDeviceType targetDevice)) return new List<VisualizationType>();
            return visualizationProviders.Where(x => x.GetAttribute<VisualizerForAttribute>()?.VisualizerFor.HasFlag(targetDevice) ?? true).ToList();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();

        #endregion
    }
}
