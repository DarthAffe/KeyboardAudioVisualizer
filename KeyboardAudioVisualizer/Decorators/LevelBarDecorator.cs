using KeyboardAudioVisualizer.AudioProcessing.VisualizationProvider;
using RGB.NET.Core;

namespace KeyboardAudioVisualizer.Decorators
{
    public class LevelBarDecorator : AbstractDecorator, IBrushDecorator
    {
        #region Properties & Fields

        private readonly IVisualizationProvider _visualizationProvider;
        public LevelBarDirection Direction { get; set; }
        public int DataIndex { get; set; }

        #endregion

        #region Constructors

        public LevelBarDecorator(IVisualizationProvider visualizationProvider, LevelBarDirection direction, int dataIndex)
        {
            this._visualizationProvider = visualizationProvider;
            this.Direction = direction;
            this.DataIndex = dataIndex;
        }

        #endregion

        #region Methods

        public void ManipulateColor(Rectangle rectangle, BrushRenderTarget renderTarget, ref Color color)
        {
            double offset = CalculateOffset(rectangle, renderTarget);

            if (offset >= _visualizationProvider.VisualizationData[DataIndex])
                color.A = 0;
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
