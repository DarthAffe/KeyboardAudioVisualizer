using System.Collections.Generic;
using System.Windows;
using KeyboardAudioVisualizer.AudioProcessing;
using KeyboardAudioVisualizer.AudioProcessing.VisualizationProvider;
using KeyboardAudioVisualizer.Configuration;
using KeyboardAudioVisualizer.Decorators;
using KeyboardAudioVisualizer.Helper;
using KeyboardAudioVisualizer.UI;
using RGB.NET.Brushes;
using RGB.NET.Brushes.Gradients;
using RGB.NET.Core;
using RGB.NET.Devices.CoolerMaster;
using RGB.NET.Devices.Corsair;
using RGB.NET.Devices.Logitech;
using RGB.NET.Devices.Novation;
using RGB.NET.Devices.Razer;
using RGB.NET.Groups;
using Point = RGB.NET.Core.Point;
using GetDecoratorFunc = System.Func<KeyboardAudioVisualizer.AudioProcessing.VisualizationProvider.VisualizationType, KeyboardAudioVisualizer.AudioProcessing.VisualizationProvider.IVisualizationProvider, RGB.NET.Core.IBrushDecorator>;

namespace KeyboardAudioVisualizer
{
    public class ApplicationManager
    {
        #region Constants
        #endregion

        #region Properties & Fields

        public static ApplicationManager Instance { get; } = new ApplicationManager();

        private ConfigurationWindow _configurationWindow;

        public Settings Settings { get; set; }

        public ObservableDictionary<VisualizationIndex, IVisualizationProvider> Visualizations { get; } = new ObservableDictionary<VisualizationIndex, IVisualizationProvider>();

        private readonly Dictionary<VisualizationIndex, IEnumerable<(ILedGroup group, GetDecoratorFunc getDecoratorFunc)>> _groups = new Dictionary<VisualizationIndex, IEnumerable<(ILedGroup group, GetDecoratorFunc getDecoratorFunc)>>();

        public TimerUpdateTrigger UpdateTrigger { get; } = new TimerUpdateTrigger();

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

            UpdateTrigger.UpdateFrequency = 1.0 / MathHelper.Clamp(Settings.UpdateRate, 1, 60);
            surface.RegisterUpdateTrigger(UpdateTrigger);

            LoadDevices(surface, CorsairDeviceProvider.Instance);
            LoadDevices(surface, CoolerMasterDeviceProvider.Instance);
            LoadDevices(surface, NovationDeviceProvider.Instance);
            LoadDevices(surface, RazerDeviceProvider.Instance);
            LoadDevices(surface, LogitechDeviceProvider.Instance);

            surface.AlignDevices();

            ILedGroup background = new ListLedGroup(surface.Leds);
            background.Brush = new SolidColorBrush(new Color(64, 0, 0, 0)); //TODO DarthAffe 06.08.2017: A-Channel gives some kind of blur - settings!

            LinearGradient primaryGradient = Settings[VisualizationIndex.Primary].Gradient;
            LinearGradient secondaryGradient = Settings[VisualizationIndex.Secondary].Gradient;
            LinearGradient tertiaryGradient = Settings[VisualizationIndex.Tertiary].Gradient;

            List<(ILedGroup, GetDecoratorFunc)> primaryGroups = new List<(ILedGroup, GetDecoratorFunc)>();
            List<(ILedGroup, GetDecoratorFunc)> secondaryGroups = new List<(ILedGroup, GetDecoratorFunc)>();
            List<(ILedGroup, GetDecoratorFunc)> tertiaryGroups = new List<(ILedGroup, GetDecoratorFunc)>();
            foreach (IRGBDevice device in RGBSurface.Instance.Devices)
                switch (device.DeviceInfo.DeviceType)
                {
                    case RGBDeviceType.Keyboard:
                    case RGBDeviceType.Keypad:
                    case RGBDeviceType.LedMatrix:
                        ListLedGroup primary = new ListLedGroup(device);

                        LightbarSpecialPart lightbar = device.GetSpecialDevicePart<LightbarSpecialPart>();
                        if (lightbar != null)
                        {
                            primary.RemoveLeds(lightbar.Leds);

                            ILedGroup lightbarLeft = new ListLedGroup(lightbar.Left);
                            lightbarLeft.Brush = new LinearGradientBrush(new Point(1.0, 0.5), new Point(0.0, 0.5), tertiaryGradient);
                            tertiaryGroups.Add((lightbarLeft, (visualizationType, visualizer) => CreateDecorator(visualizationType, visualizer, LevelBarDirection.Left, 0)));

                            ILedGroup lightbarRight = new ListLedGroup(lightbar.Right);
                            lightbarRight.Brush = new LinearGradientBrush(tertiaryGradient);
                            tertiaryGroups.Add((lightbarRight, (visualizationType, visualizer) => CreateDecorator(visualizationType, visualizer, LevelBarDirection.Right, 1)));

                            ILedGroup lightbarCenter = new ListLedGroup(lightbar.Center);
                            lightbarCenter.Brush = new LinearGradientBrush(secondaryGradient);
                            secondaryGroups.Add((lightbarCenter, (visualizationType, visualizer) => CreateDecorator(visualizationType, visualizer)));
                        }

                        primary.Brush = new LinearGradientBrush(primaryGradient);
                        primaryGroups.Add((primary, (visualizationType, visualizer) => CreateDecorator(visualizationType, visualizer, LevelBarDirection.Horizontal, 0, primaryGradient)));
                        break;

                    case RGBDeviceType.Mousepad:
                    case RGBDeviceType.LedStripe:
                    case RGBDeviceType.HeadsetStand:
                        ILedGroup left = new RectangleLedGroup(new Rectangle(device.Location.X, device.Location.Y, device.Size.Width / 2.0, device.Size.Height));
                        left.Brush = new LinearGradientBrush(new Point(0.5, 1), new Point(0.5, 0), tertiaryGradient);
                        tertiaryGroups.Add((left, (visualizationType, visualizer) => CreateDecorator(visualizationType, visualizer, LevelBarDirection.Top, 0)));

                        ILedGroup right = new RectangleLedGroup(new Rectangle(device.Location.X + (device.Size.Width / 2.0), device.Location.Y, device.Size.Width / 2.0, device.Size.Height));
                        right.Brush = new LinearGradientBrush(new Point(0.5, 1), new Point(0.5, 0), tertiaryGradient);
                        tertiaryGroups.Add((right, (visualizationType, visualizer) => CreateDecorator(visualizationType, visualizer, LevelBarDirection.Top, 1)));
                        break;

                    case RGBDeviceType.Mouse:
                    case RGBDeviceType.Headset:
                    case RGBDeviceType.Speaker:
                    case RGBDeviceType.Fan:
                    case RGBDeviceType.GraphicsCard:
                    case RGBDeviceType.DRAM:
                    case RGBDeviceType.Mainboard:
                        ILedGroup deviceGroup = new ListLedGroup(device);
                        deviceGroup.Brush = new LinearGradientBrush(secondaryGradient);
                        secondaryGroups.Add((deviceGroup, (visualizationType, visualizer) => CreateDecorator(visualizationType, visualizer)));
                        break;
                }

            _groups[VisualizationIndex.Primary] = primaryGroups;
            _groups[VisualizationIndex.Secondary] = secondaryGroups;
            _groups[VisualizationIndex.Tertiary] = tertiaryGroups;

            ApplyVisualization(VisualizationIndex.Primary, Settings[VisualizationIndex.Primary].SelectedVisualization);
            ApplyVisualization(VisualizationIndex.Secondary, Settings[VisualizationIndex.Secondary].SelectedVisualization);
            ApplyVisualization(VisualizationIndex.Tertiary, Settings[VisualizationIndex.Tertiary].SelectedVisualization);

            surface.Updating += args => AudioVisualizationFactory.Instance.Update();
        }

        private void LoadDevices(RGBSurface surface, IRGBDeviceProvider deviceProvider)
        {
            surface.LoadDevices(deviceProvider, RGBDeviceType.Keyboard | RGBDeviceType.LedMatrix
                                              | RGBDeviceType.Mousepad | RGBDeviceType.LedStripe
                                              | RGBDeviceType.Mouse | RGBDeviceType.Headset
                                              | RGBDeviceType.HeadsetStand);
        }

        //TODO DarthAffe 12.09.2017: This is just a big mess - is this worth to rework before arge?
        public void ApplyVisualization(VisualizationIndex visualizationIndex, VisualizationType visualizationType)
        {
            IVisualizationProvider visualizer = AudioVisualizationFactory.Instance.CreateVisualizationProvider(visualizationIndex, visualizationType);
            Visualizations[visualizationIndex] = visualizer;

            foreach ((ILedGroup group, GetDecoratorFunc getDecoratorFunc) in _groups[visualizationIndex])
            {
                group.Brush.RemoveAllDecorators();

                if (visualizer != null)
                {
                    IBrushDecorator decorator = getDecoratorFunc(visualizationType, visualizer);
                    if (decorator != null)
                        group.Brush.AddDecorator(decorator);
                }
            }
        }

        private IBrushDecorator CreateDecorator(VisualizationType visualizationType, IVisualizationProvider visualizationProvider, LevelBarDirection direction = LevelBarDirection.Top, int dataIndex = 0, LinearGradient gradient = null)
        {
            if (visualizationType == VisualizationType.FrequencyBars)
                return new FrequencyBarsDecorator(visualizationProvider);

            if (visualizationType == VisualizationType.Level)
                return new LevelBarDecorator(visualizationProvider, direction, dataIndex, gradient);

            if (visualizationType == VisualizationType.Beat)
                return new BeatDecorator(visualizationProvider);

            return null;
        }

        private void OpenConfiguration()
        {
            if (_configurationWindow == null) _configurationWindow = new ConfigurationWindow();
            _configurationWindow.Show();
        }

        private void Exit()
        {
            try { AudioVisualizationFactory.Instance?.Dispose(); } catch { }
            try { RGBSurface.Instance?.Dispose(); } catch { }
            Application.Current.Shutdown();
        }

        #endregion
    }
}
