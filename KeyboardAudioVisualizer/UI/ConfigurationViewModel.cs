using System;
using System.Diagnostics;
using System.Reflection;
using CSCore.CoreAudioAPI;
using KeyboardAudioVisualizer.AudioProcessing.VisualizationProvider;
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
            get => 1.0 / ApplicationManager.Instance.UpdateTrigger.UpdateFrequency;
            set
            {
                double val = MathHelper.Clamp(value, 1, 60);
                ApplicationManager.Instance.Settings.UpdateRate = val;
                ApplicationManager.Instance.UpdateTrigger.UpdateFrequency = 1.0 / val;
                OnPropertyChanged();
            }
        }

        public bool EnableAudioPrescale
        {
            get => ApplicationManager.Instance.Settings.EnableAudioPrescale;
            set
            {
                ApplicationManager.Instance.Settings.EnableAudioPrescale = value;
                OnPropertyChanged();
            }
        }

        //BLARG 01.14.2020: This makes the combobox actually display the selected device
        public MMDevice SelectedCaptureDevice
        {
            get => ApplicationManager.Instance.Settings.CaptureDevice;
            set
            {
                ApplicationManager.Instance.Settings.CaptureDevice = value;
            }
        }

        public VisualizationType SelectedPrimaryVisualization
        {
            get => ApplicationManager.Instance.Settings[VisualizationIndex.Primary].SelectedVisualization;
            set
            {
                ApplicationManager.Instance.Settings[VisualizationIndex.Primary].SelectedVisualization = value;
                ApplicationManager.Instance.ApplyVisualization(VisualizationIndex.Primary, value);
            }
        }

        public VisualizationType SelectedSecondaryVisualization
        {
            get => ApplicationManager.Instance.Settings[VisualizationIndex.Secondary].SelectedVisualization;
            set
            {
                ApplicationManager.Instance.Settings[VisualizationIndex.Secondary].SelectedVisualization = value;
                ApplicationManager.Instance.ApplyVisualization(VisualizationIndex.Secondary, value);
            }
        }

        public VisualizationType SelectedTertiaryVisualization
        {
            get => ApplicationManager.Instance.Settings[VisualizationIndex.Tertiary].SelectedVisualization;
            set
            {
                ApplicationManager.Instance.Settings[VisualizationIndex.Tertiary].SelectedVisualization = value;
                ApplicationManager.Instance.ApplyVisualization(VisualizationIndex.Tertiary, value);
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
