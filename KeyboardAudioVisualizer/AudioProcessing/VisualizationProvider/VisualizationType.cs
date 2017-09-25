using KeyboardAudioVisualizer.Attributes;
using RGB.NET.Core;

namespace KeyboardAudioVisualizer.AudioProcessing.VisualizationProvider
{
    public enum VisualizationType
    {
        None,

        [VisualizerFor(RGBDeviceType.Keyboard | RGBDeviceType.LedMatrix)]
        [DisplayName("Frequency Bars")]
        FrequencyBars,

        [VisualizerFor(RGBDeviceType.Keyboard | RGBDeviceType.LedMatrix | RGBDeviceType.LedStripe | RGBDeviceType.Mousepad)]
        Level,

        Beat,
    }
}
