using System;

namespace KeyboardAudioVisualizer.Attributes
{
    public class DisplayNameAttribute : Attribute
    {
        #region Properties & Fields

        public string DisplayName { get; set; }

        #endregion

        #region Constructors

        public DisplayNameAttribute(string displayName)
        {
            this.DisplayName = displayName;
        }

        #endregion
    }
}
