using System;
using KeyboardAudioVisualizer.AudioProcessing.VisualizationPRovider;
using RGB.NET.Brushes;
using RGB.NET.Brushes.Gradients;
using RGB.NET.Core;
using Color = RGB.NET.Core.Color;
using Rectangle = RGB.NET.Core.Rectangle;

namespace KeyboardAudioVisualizer.Brushes
{
    public class FrequencyBarsBrush : LinearGradientBrush
    {
        #region Properties & Fields

        private readonly IVisualizationProvider _visualizationProvider;

        #endregion

        #region Constructors

        public FrequencyBarsBrush(IVisualizationProvider visualizationProvider, IGradient gradient)
            : base(gradient)
        {
            this._visualizationProvider = visualizationProvider;
        }

        #endregion

        #region Methods

        protected override Color GetColorAtPoint(Rectangle rectangle, BrushRenderTarget renderTarget)
        {
            int barSampleIndex = (int)Math.Floor(_visualizationProvider.VisualizationData.Length * (renderTarget.Point.X / (rectangle.Location.X + rectangle.Size.Width)));
            double curBarHeight = 1.0 - Math.Max(0f, _visualizationProvider.VisualizationData[barSampleIndex]);
            double verticalPos = (renderTarget.Point.Y / rectangle.Size.Height);

            return curBarHeight <= verticalPos ? base.GetColorAtPoint(rectangle, renderTarget) : Color.Transparent;
        }

        #endregion
    }
}
