using System.Windows;
using KeyboardAudioVisualizer.AudioProcessing;
using KeyboardAudioVisualizer.Configuration;
using KeyboardAudioVisualizer.Decorators;
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
using Point = RGB.NET.Core.Point;

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
                        ListLedGroup primary = new ListLedGroup(device);

                        LightbarSpecialPart lightbar = device.GetSpecialDevicePart<LightbarSpecialPart>();
                        if (lightbar != null)
                        {
                            primary.RemoveLeds(lightbar.Leds);

                            IGradient keyboardLevelGradient = new LinearGradient(new GradientStop(0, new Color(0, 0, 255)), new GradientStop(1, new Color(255, 0, 0)));

                            ILedGroup lightbarLeft = new ListLedGroup(lightbar.Left);
                            lightbarLeft.Brush = new LinearGradientBrush(keyboardLevelGradient);
                            lightbarLeft.Brush.AddDecorator(new LevelBarDecorator(AudioProcessor.Instance.TertiaryVisualizationProvider, LevelBarDirection.Left, 0));

                            ILedGroup lightbarRight = new ListLedGroup(lightbar.Right);
                            lightbarRight.Brush = new LinearGradientBrush(keyboardLevelGradient);
                            lightbarRight.Brush.AddDecorator(new LevelBarDecorator(AudioProcessor.Instance.TertiaryVisualizationProvider, LevelBarDirection.Right, 1));

                            ILedGroup lightbarCenter = new ListLedGroup(lightbar.Center);
                            lightbarCenter.Brush = new SolidColorBrush(new Color(255, 255, 255));
                            lightbarCenter.Brush.AddDecorator(new BeatDecorator(AudioProcessor.Instance.SecondaryVisualizationProvider));
                        }

                        primary.Brush = new LinearGradientBrush(new RainbowGradient(300, -14));
                        primary.Brush.AddDecorator(new FrequencyBarsDecorator(AudioProcessor.Instance.PrimaryVisualizationProvider));
                        break;

                    case RGBDeviceType.Mousepad:
                    case RGBDeviceType.LedStripe:

                        IGradient mousepadLevelGradient = new LinearGradient(new GradientStop(0, new Color(0, 0, 255)), new GradientStop(1, new Color(255, 0, 0)));

                        ILedGroup left = new RectangleLedGroup(new Rectangle(device.Location.X, device.Location.Y, device.Size.Width / 2.0, device.Size.Height));
                        left.Brush = new LinearGradientBrush(new Point(0.5, 1), new Point(0.5, 0), mousepadLevelGradient);
                        left.Brush.AddDecorator(new LevelBarDecorator(AudioProcessor.Instance.TertiaryVisualizationProvider, LevelBarDirection.Top, 0));

                        ILedGroup right = new RectangleLedGroup(new Rectangle(device.Location.X + (device.Size.Width / 2.0), device.Location.Y, device.Size.Width / 2.0, device.Size.Height));
                        right.Brush = new LinearGradientBrush(new Point(0.5, 1), new Point(0.5, 0), mousepadLevelGradient);
                        right.Brush.AddDecorator(new LevelBarDecorator(AudioProcessor.Instance.TertiaryVisualizationProvider, LevelBarDirection.Top, 1));
                        break;

                    case RGBDeviceType.Mouse:
                    case RGBDeviceType.Headset:
                        ILedGroup deviceGroup = new ListLedGroup(device);
                        deviceGroup.Brush = new SolidColorBrush(new Color(255, 255, 255));
                        deviceGroup.Brush.AddDecorator(new BeatDecorator(AudioProcessor.Instance.SecondaryVisualizationProvider));
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
