using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using CSCore.CoreAudioAPI;
/*BLARG 01.14.2020:
    MMDevices don't really play nice with xaml on their own so they need some conversion.
    Fortunately MMDevice.FriendlyName is fantastic.
    Also when the ComboBox goes through all members of MMDeviceCollection they keep their order so we can handle the convertBack in the ConfigurationWindow.xaml.cs instead of here.
     */
namespace KeyboardAudioVisualizer.Converter
{
    [ValueConversion(typeof(IEnumerable<MMDeviceCollection>), typeof(IEnumerable<MMDeviceCollection>))]
    class CaptureDeviceConverter : IValueConverter
    {
        private MMDeviceCollection DeviceCollection = ApplicationManager.Instance.Settings.DeviceCollection;
        private MMDevice CaptureDevice = ApplicationManager.Instance.Settings.CaptureDevice;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MMDeviceCollection) return (value as MMDeviceCollection).Select(x => x.FriendlyName);
            else return (value as MMDevice).FriendlyName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
