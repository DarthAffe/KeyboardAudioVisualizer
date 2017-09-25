using System;
using RGB.NET.Core;

namespace KeyboardAudioVisualizer.Attributes
{
    public class VisualizerForAttribute : Attribute
    {
        #region Properties & Fields

        public RGBDeviceType VisualizerFor { get; set; }

        #endregion

        #region Constructors

        public VisualizerForAttribute(RGBDeviceType visualizerFor)
        {
            this.VisualizerFor = visualizerFor;
        }

        #endregion
    }
}
