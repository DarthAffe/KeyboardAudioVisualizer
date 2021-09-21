using System;
using CSCore.CoreAudioAPI;
using KeyboardAudioVisualizer.AudioProcessing;
using KeyboardAudioVisualizer.Controls;

namespace KeyboardAudioVisualizer.UI
{
    public partial class ConfigurationWindow : BlurredDecorationWindow
    {
        public ConfigurationWindow() => InitializeComponent();

        //DarthAffe 07.02.2018: This prevents the applicaiton from not shutting down and crashing afterwards if 'close' is selected in the taskbar-context-menu
        private void ConfigurationWindow_OnClosed(object sender, EventArgs e)
        {
            ApplicationManager.Instance.ExitCommand.Execute(null);
        }
        
        //BLARG 01.14.2020: ComboBoxes are magic
        private void DeviceBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //For some reason the SelectionChanged event happens when you first open the configuration window, so in order to avoid unnecessary device changes we wait for the window to be visible
            if (deviceBox.IsVisible)
            {
                MMDevice captureDevice = ApplicationManager.Instance.Settings.DeviceCollection.ItemAt(deviceBox.SelectedIndex);

                AudioVisualizationFactory.Instance.ChangeAudioDevice(captureDevice);

                KeyboardAudioVisualizer.ApplicationManager.Instance.Settings.CaptureDevice = captureDevice;
            }
        }
    }
}
