using System;
using KeyboardAudioVisualizer.AudioProcessing.VisualizationProvider;
using RGB.NET.Core;
using Color = RGB.NET.Core.Color;
using Rectangle = RGB.NET.Core.Rectangle;

namespace KeyboardAudioVisualizer.Decorators
{
    public class FrequencyBarsDecorator : AbstractUpdateAwareDecorator, IBrushDecorator
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

        protected override void Update(double deltaTime) => _visualizationProvider.Update();

        public Color ManipulateColor(Rectangle rectangle, BrushRenderTarget renderTarget, Color color)
        {
            int barSampleIndex = Math.Min(_visualizationProvider.VisualizationData.Length, (int)Math.Floor(_visualizationProvider.VisualizationData.Length * (renderTarget.Point.X / (rectangle.Location.X + rectangle.Size.Width))));
            double curBarHeight = 1.0 - Math.Max(0f, _visualizationProvider.VisualizationData[barSampleIndex]);
            double verticalPos = (renderTarget.Point.Y / rectangle.Size.Height);

            return curBarHeight > verticalPos ? color.SetA(0) : color;
        }

        #endregion
    }
}
