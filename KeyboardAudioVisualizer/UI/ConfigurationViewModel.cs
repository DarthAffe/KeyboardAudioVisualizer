using System;
using System.Diagnostics;
using System.Reflection;
using KeyboardAudioVisualizer.Helper;
using RGB.NET.Core;

namespace KeyboardAudioVisualizer.UI
{
    public class ConfigurationViewModel : AbstractBindable
    {
        #region Properties & Fields

        public Version Version => Assembly.GetEntryAssembly().GetName().Version;

        public double UpdateRate
        {
            get => 1.0 / RGBSurface.Instance.UpdateFrequency;
            set
            {
                double val = MathHelper.Clamp(value, 1, 40);
                ApplicationManager.Instance.Settings.UpdateRate = val;
                RGBSurface.Instance.UpdateFrequency = 1.0 / val;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        private ActionCommand _openHomepageCommand;
        public ActionCommand OpenHomepageCommand => _openHomepageCommand ?? (_openHomepageCommand = new ActionCommand(OpenHomepage));

        #endregion

        #region Constructors

        #endregion

        #region Methods

        private void OpenHomepage() => Process.Start("https://github.com/DarthAffe/KeyboardAudioVisualizer");

        #endregion
    }
}
