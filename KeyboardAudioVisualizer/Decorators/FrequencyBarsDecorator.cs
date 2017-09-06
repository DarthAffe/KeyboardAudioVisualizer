using System;
using KeyboardAudioVisualizer.AudioProcessing.VisualizationProvider;
using RGB.NET.Core;
using Color = RGB.NET.Core.Color;
using Rectangle = RGB.NET.Core.Rectangle;

namespace KeyboardAudioVisualizer.Decorators
{
    public class FrequencyBarsDecorator : AbstractDecorator, IBrushDecorator
    {
        #region Properties & Fields

        private readonly IVisualizationProvider _visualizationProvider;

        #endregion

        #region Constructors

        public FrequencyBarsDecorator(IVisualizationProvider visualizationProvider)
        {
            this._visualizationProvider = visualizationProvider;
        }

        #endregion

        #region Methods

        public void ManipulateColor(Rectangle rectangle, BrushRenderTarget renderTarget, ref Color color)
        {
            int barSampleIndex = (int)Math.Floor(_visualizationProvider.VisualizationData.Length * (renderTarget.Point.X / (rectangle.Location.X + rectangle.Size.Width)));
            double curBarHeight = 1.0 - Math.Max(0f, _visualizationProvider.VisualizationData[barSampleIndex]);
            double verticalPos = (renderTarget.Point.Y / rectangle.Size.Height);

            if (curBarHeight > verticalPos)
                color.A = 0;
        }

        #endregion
    }
}
