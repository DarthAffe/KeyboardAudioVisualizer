using System;
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
    }
}
