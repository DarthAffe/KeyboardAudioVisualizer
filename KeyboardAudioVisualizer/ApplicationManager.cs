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

            surface.UpdateFrequency = 1 / 40.0; //TODO DarthAffe 03.08.2017: Settings
            surface.UpdateMode = UpdateMode.Continuous;

            surface.LoadDevices(CorsairDeviceProvider.Instance);
            //surface.LoadDevices(LogitechDeviceProvider.Instance);
            //surface.LoadDevices(CoolerMasterDeviceProvider.Instance);

            ILedGroup background = new ListLedGroup(surface.Leds);
            background.Brush = new SolidColorBrush(new Color(64, 0, 0, 0)); //TODO DarthAffe 06.08.2017: A-Channel gives some kind of blur - settings!

            //TODO DarthAffe 03.08.2017: Changeable, Settings etc.
            foreach (IRGBDevice device in surface.Devices)
                switch (device.DeviceInfo.DeviceType)
                {
                    case RGBDeviceType.Keyboard:
                        //TODO DarthAffe 05.08.2017: Lighbar-support has to be better in RGB.NET
                        if (device[new CorsairLedId(device, CorsairLedIds.Lightbar1)] != null)
                        {
                            ILedGroup lightbarLeft = new ListLedGroup(new CorsairLedId(device, CorsairLedIds.Lightbar1), new CorsairLedId(device, CorsairLedIds.Lightbar2),
                                                                      new CorsairLedId(device, CorsairLedIds.Lightbar3), new CorsairLedId(device, CorsairLedIds.Lightbar4),
                                                                      new CorsairLedId(device, CorsairLedIds.Lightbar5), new CorsairLedId(device, CorsairLedIds.Lightbar6),
                                                                      new CorsairLedId(device, CorsairLedIds.Lightbar7), new CorsairLedId(device, CorsairLedIds.Lightbar8),
                                                                      new CorsairLedId(device, CorsairLedIds.Lightbar9));
                            ILedGroup lightbarCenter = new ListLedGroup(new CorsairLedId(device, CorsairLedIds.Lightbar10));
                            ILedGroup lightbarRight = new ListLedGroup(new CorsairLedId(device, CorsairLedIds.Lightbar11), new CorsairLedId(device, CorsairLedIds.Lightbar12),
                                                                       new CorsairLedId(device, CorsairLedIds.Lightbar13), new CorsairLedId(device, CorsairLedIds.Lightbar14),
                                                                       new CorsairLedId(device, CorsairLedIds.Lightbar15), new CorsairLedId(device, CorsairLedIds.Lightbar16),
                                                                       new CorsairLedId(device, CorsairLedIds.Lightbar17), new CorsairLedId(device, CorsairLedIds.Lightbar18),
                                                                       new CorsairLedId(device, CorsairLedIds.Lightbar19));
                            ListLedGroup primary = new ListLedGroup(device);
                            primary.RemoveLeds(lightbarLeft.GetLeds());
                            primary.RemoveLeds(lightbarCenter.GetLeds());
                            primary.RemoveLeds(lightbarRight.GetLeds());

                            IGradient keyboardLevelGradient = new LinearGradient(new GradientStop(0, new Color(0, 0, 255)), new GradientStop(1, new Color(255, 0, 0)));
                            lightbarLeft.Brush = new LevelBarBrush(AudioProcessor.Instance.TertiaryVisualizationProvider, keyboardLevelGradient, LevelBarDirection.Left, 0);
                            lightbarRight.Brush = new LevelBarBrush(AudioProcessor.Instance.TertiaryVisualizationProvider, keyboardLevelGradient, LevelBarDirection.Right, 1);
                            lightbarCenter.Brush = new BeatBrush(AudioProcessor.Instance.SecondaryVisualizationProvider, new Color(255, 255, 255));

                            primary.Brush = new FrequencyBarsBrush(AudioProcessor.Instance.PrimaryVisualizationProvider, new RainbowGradient(300, -14));
                        }
                        else
                            new ListLedGroup(device).Brush = new FrequencyBarsBrush(AudioProcessor.Instance.PrimaryVisualizationProvider, new RainbowGradient(300, -14));
                        //new ListLedGroup(device).Brush = new BeatBrush(AudioProcessor.Instance.PrimaryVisualizationProvider, new Color(255, 255, 255));

                        //{
                        //    ILedGroup left1 = new RectangleLedGroup(new Rectangle(device.Location.X, device.Location.Y, device.Size.Width / 2.0, device.Size.Height));
                        //    ILedGroup right1 = new RectangleLedGroup(new Rectangle(device.Location.X + (device.Size.Width / 2.0), device.Location.Y, device.Size.Width / 2.0, device.Size.Height));

                        //    IGradient levelGradient = new LinearGradient(new GradientStop(0, new Color(0, 0, 255)), new GradientStop(1, new Color(255, 0, 0)));
                        //    left1.Brush = new LevelBarBrush(AudioProcessor.Instance.TertiaryVisualizationProvider, levelGradient, LevelBarDirection.Left, 0);
                        //    right1.Brush = new LevelBarBrush(AudioProcessor.Instance.TertiaryVisualizationProvider, levelGradient, LevelBarDirection.Right, 1);
                        //}
                        break;

                    case RGBDeviceType.Mousepad:
                        ILedGroup left = new RectangleLedGroup(new Rectangle(device.Location.X, device.Location.Y, device.Size.Width / 2.0, device.Size.Height));
                        ILedGroup right = new RectangleLedGroup(new Rectangle(device.Location.X + (device.Size.Width / 2.0), device.Location.Y, device.Size.Width / 2.0, device.Size.Height));

                        IGradient mousepadLevelGradient = new LinearGradient(new GradientStop(0, new Color(0, 0, 255)), new GradientStop(1, new Color(255, 0, 0)));
                        left.Brush = new LevelBarBrush(AudioProcessor.Instance.TertiaryVisualizationProvider, mousepadLevelGradient, LevelBarDirection.Top, 0);
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
