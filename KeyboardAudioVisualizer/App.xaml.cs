﻿using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;
using KeyboardAudioVisualizer.AudioProcessing;
using KeyboardAudioVisualizer.Configuration;
using KeyboardAudioVisualizer.Helper;
using KeyboardAudioVisualizer.Legacy;
using Newtonsoft.Json;
using RGB.NET.Brushes.Gradients;
using RGB.NET.Core;
using Settings = KeyboardAudioVisualizer.Configuration.Settings;

namespace KeyboardAudioVisualizer
{
    public partial class App : Application
    {
        #region Constants

        private const string PATH_SETTINGS = "Settings.json";

        #endregion

        #region Properties & Fields

        private TaskbarIcon _taskbarIcon;

        #endregion

        #region Methods

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(int.MaxValue));

                _taskbarIcon = (TaskbarIcon)FindResource("TaskbarIcon");
                _taskbarIcon.DoubleClickCommand = ApplicationManager.Instance.OpenConfigurationCommand;

                //Settings settings = SerializationHelper.LoadObjectFromFile<Settings>(PATH_SETTINGS);
                Settings settings = null;
                try { settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(PATH_SETTINGS), new ColorSerializer()); }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    /* File doesn't exist or is corrupt - just create a new one. */
                }

                if (settings == null)
                    settings = ConfigurationMigrator.MigrateOldConfig();

                if (settings == null)
                {
                    settings = new Settings
                    {
                        Version = Settings.CURRENT_VERSION,
                        Background = new LinearGradient(new GradientStop(0.5, new Color(64, 0, 0, 0)))
                    };
                    _taskbarIcon.ShowBalloonTip("Keyboard Audio-Visualizer is starting in the tray!", "Click on the icon to open the configuration.", BalloonIcon.Info);
                }
                else if (settings.Version != Settings.CURRENT_VERSION)
                    ConfigurationUpdates.PerformOn(settings);

                ApplicationManager.Instance.Settings = settings;

                AudioVisualizationFactory.Initialize(settings.CaptureDevice);
                ApplicationManager.Instance.InitializeDevices();
            }
            catch (Exception ex)
            {
                File.WriteAllText("error.log", $"[{DateTime.Now:G}] Exception!\r\n\r\nMessage:\r\n{ex.GetFullMessage()}\r\n\r\nStackTrace:\r\n{ex.StackTrace}\r\n\r\n");
                MessageBox.Show("An error occured while starting the Keyboard Audio-Visualizer.\r\nPlease double check if SDK-support for your devices is enabled.\r\nMore information can be found in the error.log file in the application directory.", "Can't start Keyboard Audio-Visualizer.");

                try { ApplicationManager.Instance.ExitCommand.Execute(null); }
                catch { Environment.Exit(0); }
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            File.WriteAllText(PATH_SETTINGS, JsonConvert.SerializeObject(ApplicationManager.Instance.Settings, new ColorSerializer()));
            ConfigurationMigrator.CleanupOldConfigs();
        }

        #endregion
    }
}
