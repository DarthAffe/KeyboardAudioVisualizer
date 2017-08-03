using System.Windows;
using KeyboardAudioVisualizer.AudioProcessing;
using KeyboardAudioVisualizer.Brushes;
using KeyboardAudioVisualizer.Helper;
using KeyboardAudioVisualizer.UI;
using RGB.NET.Brushes;
using RGB.NET.Brushes.Gradients;
using RGB.NET.Core;
using RGB.NET.Devices.Corsair;
using RGB.NET.Groups;

namespace KeyboardAudioVisualizer
{
    public class ApplicationManager
    {
        #region Properties & Fields

        public static ApplicationManager Instance { get; } = new ApplicationManager();

        private ConfigurationWindow _configurationWindow;

        public Settings Settings { get; set; }

        #endregion

        #region Commands

        private ActionCommand _openConfiguration;
        public ActionCommand OpenConfigurationCommand => _openConfiguration ?? (_openConfiguration = new ActionCommand(OpenConfiguration));

        private ActionCommand _exitCommand;
        public ActionCommand ExitCommand => _exitCommand ?? (_exitCommand = new ActionCommand(Exit));

        #endregion

        #region Constructors

        private ApplicationManager() { }

        #endregion

        #region Methods

        public void InitializeDevices()
        {
            RGBSurface surface = RGBSurface.Instance;
            //surface.Exception += args =>;

            surface.UpdateFrequency = 1 / 30.0; //TODO DarthAffe 03.08.2017: Settings
            surface.UpdateMode = UpdateMode.Continuous;

            surface.LoadDevices(CorsairDeviceProvider.Instance);
            //surface.LoadDevices(LogitechDeviceProvider.Instance);
            //surface.LoadDevices(CoolerMasterDeviceProvider.Instance);

            ILedGroup background = new ListLedGroup(surface.Leds);
            background.Brush = new SolidColorBrush(new Color(0, 0, 0));

            //TODO DarthAffe 03.08.2017: Changeable, Settings etc.
            foreach (IRGBDevice device in surface.Devices)
            {
                if (device.DeviceInfo.DeviceType == RGBDeviceType.Keyboard)
                    new ListLedGroup(device).Brush = new FrequencyBarsBrush(AudioProcessor.Instance.PrimaryVisualizationProvider, new RainbowGradient(300, -14));
            }

            surface.Updating += args => AudioProcessor.Instance.Update();
        }

        private void OpenConfiguration()
        {
            if (_configurationWindow == null) _configurationWindow = new ConfigurationWindow();
            _configurationWindow.Show();
        }

        private void Exit() => Application.Current.Shutdown();

        #endregion
    }
}
