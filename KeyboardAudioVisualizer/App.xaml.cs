using System;
using System.IO;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using KeyboardAudioVisualizer.AudioProcessing;
using KeyboardAudioVisualizer.Helper;

namespace KeyboardAudioVisualizer
{
    public partial class App : Application
    {
        #region Constants

        private const string PATH_SETTINGS = "Settings.xml";

        #endregion

        #region Properties & Fields

        private TaskbarIcon _taskbarIcon;

        #endregion

        #region Constructors

        #endregion

        #region Methods

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                _taskbarIcon = (TaskbarIcon)FindResource("TaskbarIcon");
                _taskbarIcon.DoubleClickCommand = ApplicationManager.Instance.OpenConfigurationCommand;

                Settings settings = SerializationHelper.LoadObjectFromFile<Settings>(PATH_SETTINGS);
                if (settings == null)
                {
                    settings = new Settings();
                    _taskbarIcon.ShowBalloonTip("Keyboard Audio-Visualizer is starting in the tray!", "Click on the icon to open the configuration.", BalloonIcon.Info);
                }
                ApplicationManager.Instance.Settings = settings;

                AudioProcessor.Initialize();
                ApplicationManager.Instance.InitializeDevices();
            }
            catch (Exception ex)
            {
                File.WriteAllText("error.log", $"[{DateTime.Now:G}] Exception!\r\n\r\nMessage:\r\n{ex.GetFullMessage()}\r\n\r\nStackTrace:\r\n{ex.StackTrace}\r\n\r\n");
                MessageBox.Show("An error occured while starting the Keyboard Audio-Visualizer.\r\nPlease double check if SDK-support for your devices is enabled.\r\nMore information can be found in the error.log file in the application directory.", "Can't start Keyboard Audio-Visualizer.");
                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            SerializationHelper.SaveObjectToFile(ApplicationManager.Instance.Settings, PATH_SETTINGS);
        }

        #endregion
    }
}
