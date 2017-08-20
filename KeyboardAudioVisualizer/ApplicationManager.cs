using System.Windows;
using KeyboardAudioVisualizer.AudioProcessing;
using KeyboardAudioVisualizer.Brushes;
using KeyboardAudioVisualizer.Configuration;
using KeyboardAudioVisualizer.Helper;
using KeyboardAudioVisualizer.UI;
using RGB.NET.Brushes;
using RGB.NET.Brushes.Gradients;
using RGB.NET.Core;
using RGB.NET.Devices.CoolerMaster;
using RGB.NET.Devices.Corsair;
using RGB.NET.Devices.Corsair.SpecialParts;
using RGB.NET.Devices.Logitech;
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

            surface.UpdateFrequency = 1 / MathHelper.Clamp(Settings.UpdateRate, 1, 40);
            surface.UpdateMode = UpdateMode.Continuous;

            surface.LoadDevices(CorsairDeviceProvider.Instance);
            surface.LoadDevices(LogitechDeviceProvider.Instance);
            surface.LoadDevices(CoolerMasterDeviceProvider.Instance);

            ILedGroup background = new ListLedGroup(surface.Leds);
            background.Brush = new SolidColorBrush(new Color(64, 0, 0, 0)); //TODO DarthAffe 06.08.2017: A-Channel gives some kind of blur - settings!

            //TODO DarthAffe 03.08.2017: Changeable, Settings etc.
            foreach (IRGBDevice device in surface.Devices)
                switch (device.DeviceInfo.DeviceType)
                {
                    case RGBDeviceType.Keyboard:
                    case RGBDeviceType.LedMatrix:
                        LightbarSpecialPart lightbar = device.GetSpecialDevicePart<LightbarSpecialPart>();
                        if (lightbar != null)
                        {
                            ListLedGroup primary = new ListLedGroup(device);
                            primary.RemoveLeds(lightbar.Leds);
                            primary.Brush = new FrequencyBarsBrush(AudioProcessor.Instance.PrimaryVisualizationProvider, new RainbowGradient(300, -14));

                            IGradient keyboardLevelGradient = new LinearGradient(new GradientStop(0, new Color(0, 0, 255)), new GradientStop(1, new Color(255, 0, 0)));

                            ILedGroup lightbarLeft = new ListLedGroup(lightbar.Left);
                            lightbarLeft.Brush = new LevelBarBrush(AudioProcessor.Instance.TertiaryVisualizationProvider, keyboardLevelGradient, LevelBarDirection.Left, 0);

                            ILedGroup lightbarRight = new ListLedGroup(lightbar.Right);
                            lightbarRight.Brush = new LevelBarBrush(AudioProcessor.Instance.TertiaryVisualizationProvider, keyboardLevelGradient, LevelBarDirection.Right, 1);

                            ILedGroup lightbarCenter = new ListLedGroup(lightbar.Center);
                            lightbarCenter.Brush = new BeatBrush(AudioProcessor.Instance.SecondaryVisualizationProvider, new Color(255, 255, 255));
                        }
                        else
                            new ListLedGroup(device).Brush = new FrequencyBarsBrush(AudioProcessor.Instance.PrimaryVisualizationProvider, new RainbowGradient(300, -14));
                        break;

                    case RGBDeviceType.Mousepad:
                    case RGBDeviceType.LedStripe:

                        IGradient mousepadLevelGradient = new LinearGradient(new GradientStop(0, new Color(0, 0, 255)), new GradientStop(1, new Color(255, 0, 0)));

                        ILedGroup left = new RectangleLedGroup(new Rectangle(device.Location.X, device.Location.Y, device.Size.Width / 2.0, device.Size.Height));
                        left.Brush = new LevelBarBrush(AudioProcessor.Instance.TertiaryVisualizationProvider, mousepadLevelGradient, LevelBarDirection.Top, 0);

                        ILedGroup right = new RectangleLedGroup(new Rectangle(device.Location.X + (device.Size.Width / 2.0), device.Location.Y, device.Size.Width / 2.0, device.Size.Height));
                        right.Brush = new LevelBarBrush(AudioProcessor.Instance.TertiaryVisualizationProvider, mousepadLevelGradient, LevelBarDirection.Top, 1);
                        break;

                    case RGBDeviceType.Mouse:
                    case RGBDeviceType.Headset:
                        ILedGroup deviceGroup = new ListLedGroup(device);
                        deviceGroup.Brush = new BeatBrush(AudioProcessor.Instance.SecondaryVisualizationProvider, new Color(255, 255, 255));
                        break;
                }

            surface.Updating += args => AudioProcessor.Instance.Update();
        }

        private void OpenConfiguration()
        {
            if (_configurationWindow == null) _configurationWindow = new ConfigurationWindow();
            _configurationWindow.Show();
        }

        private void Exit()
        {
            RGBSurface.Instance.Dispose();
            AudioProcessor.Instance.Dispose();
            Application.Current.Shutdown();
        }

        #endregion
    }
}
