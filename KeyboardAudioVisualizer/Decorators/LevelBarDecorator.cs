using KeyboardAudioVisualizer.AudioProcessing.VisualizationProvider;
using RGB.NET.Core;

namespace KeyboardAudioVisualizer.Decorators
{
    public class LevelBarDecorator : AbstractUpdateAwareDecorator, IBrushDecorator
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

        protected override void Update(double deltaTime) => _visualizationProvider.Update();

        public Color ManipulateColor(Rectangle rectangle, BrushRenderTarget renderTarget, Color color)
        {
            double offset = CalculateOffset(rectangle, renderTarget);

            if (Direction == LevelBarDirection.Horizontal)
            {
                if (offset < 0)
                {
                    offset = (-offset * 2);
                    if (offset >= _visualizationProvider.VisualizationData[0])
                        return color.SetA(0);
                }
                else
                {
                    offset *= 2;
                    if (offset >= _visualizationProvider.VisualizationData[1])
                        return color.SetA(0);
                }
            }
            else
            {
                if (offset >= _visualizationProvider.VisualizationData[DataIndex])
                    return color.SetA(0);
            }

            return color;
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

                case LevelBarDirection.Horizontal:
                    return (renderTarget.Rectangle.Center.X / rectangle.Size.Width) - 0.5;

                default:
                    return -1;
            }
        }

        #endregion
    }

    #region Data

    public enum LevelBarDirection
    {
        Left, Right, Top, Bottom,

        //HACK DarthAffe 12.09.2017: Just a bad workaround ...
        Horizontal
    }

    #endregion
}
