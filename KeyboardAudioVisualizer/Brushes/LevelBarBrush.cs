using KeyboardAudioVisualizer.AudioProcessing.VisualizationProvider;
using RGB.NET.Brushes;
using RGB.NET.Brushes.Gradients;
using RGB.NET.Core;

namespace KeyboardAudioVisualizer.Brushes
{
    public class LevelBarBrush : LinearGradientBrush
    {
        #region Properties & Fields

        private readonly IVisualizationProvider _visualizationProvider;
        public LevelBarDirection Direction { get; set; }
        public int DataIndex { get; set; }

        #endregion

        #region Constructors

        public LevelBarBrush(IVisualizationProvider visualizationProvider, IGradient gradient, LevelBarDirection direction, int dataIndex)
            : base(gradient)
        {
            this._visualizationProvider = visualizationProvider;
            this.Direction = direction;
            this.DataIndex = dataIndex;
        }

        #endregion

        #region Methods

        protected override Color GetColorAtPoint(Rectangle rectangle, BrushRenderTarget renderTarget)
        {
            double offset = CalculateOffset(rectangle, renderTarget);
            return offset < _visualizationProvider.VisualizationData[DataIndex] ? Gradient.GetColor(offset) : Color.Transparent;
        }

        private double CalculateOffset(Rectangle rectangle, BrushRenderTarget renderTarget)
        {
            switch (Direction)
            {
                case LevelBarDirection.Left:
                    return (rectangle.Size.Width - renderTarget.Rectangle.Center.X) / rectangle.Size.Width;

                case LevelBarDirection.Right:
                    return renderTarget.Rectangle.Center.X / rectangle.Size.Width;

                case LevelBarDirection.Top:
                    return (rectangle.Size.Height - renderTarget.Rectangle.Center.Y) / rectangle.Size.Height;

                case LevelBarDirection.Bottom:
                    return renderTarget.Rectangle.Center.Y / rectangle.Size.Height;

                default:
                    return -1;
            }
        }

        #endregion
    }

    #region Data

    public enum LevelBarDirection
    {
        Left, Right, Top, Bottom
    }

    #endregion
}
