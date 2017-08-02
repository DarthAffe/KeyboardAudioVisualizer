using System.Diagnostics;
using KeyboardAudioVisualizer.Helper;

namespace KeyboardAudioVisualizer.UI
{
    public class ConfigurationViewModel
    {
        #region Properties & Fields

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
