using KeyboardAudioVisualizer.AudioProcessing.VisualizationProvider;
using RGB.NET.Core;

namespace KeyboardAudioVisualizer.Brushes
{
    public class BeatBrush : AbstractBrush
    {
        #region Properties & Fields

        private readonly IVisualizationProvider _visualizationProvider;
        private readonly Color _color;

        #endregion

        #region Constructors

        public BeatBrush(IVisualizationProvider visualizationProvider, Color color)
        {
            this._visualizationProvider = visualizationProvider;
            this._color = color;
        }

        #endregion

        #region Methods

        protected override Color GetColorAtPoint(Rectangle rectangle, BrushRenderTarget renderTarget)
        {
            Color color = new Color(_color);
            color.APercent *= _visualizationProvider.VisualizationData[0];
            return color;
        }

        #endregion
    }
}
