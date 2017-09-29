using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Point = System.Windows.Point;

namespace KeyboardAudioVisualizer.Attached
{
    public class SliderValueAdorner : System.Windows.Documents.Adorner
    {
        #region Properties & Fields

        private readonly string _unit;
        private readonly Slider _slider;
        private readonly Thumb _thumb;
        private readonly RepeatButton _decreaseRepeatButton;

        public Brush BorderBrush { get; set; } = Brushes.Black;
        public Brush Background { get; set; } = Brushes.Black;
        public Brush Foreground { get; set; } = Brushes.White;
        public FontFamily Font { get; set; } = new FontFamily("Verdana");
        public double FontSize { get; set; } = 14;

        #endregion

        #region Constructors

        public SliderValueAdorner(UIElement adornedElement, string unit)
            : base(adornedElement)
        {
            this._unit = unit;

            _slider = (Slider)adornedElement;
            Track track = (Track)_slider.Template.FindName("PART_Track", _slider);

            _thumb = track.Thumb;
            _decreaseRepeatButton = track.DecreaseRepeatButton;
            _decreaseRepeatButton.SizeChanged += OnButtonSizeChanged;
        }

        #endregion

        #region Methods

        public void Cleanup()
        {
            _decreaseRepeatButton.SizeChanged -= OnButtonSizeChanged;
        }

        private void OnButtonSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs) => InvalidateVisual();

        protected override void OnRender(DrawingContext drawingContext)
        {
            double offset = _decreaseRepeatButton.ActualWidth + (_thumb.ActualWidth / 2.0);

            FormattedText text = new FormattedText(GetText(), CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface(Font, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), FontSize, Foreground);
            Geometry border = CreateBorder(offset, text.Width, text.Height);

            drawingContext.DrawGeometry(Background, new Pen(BorderBrush, 1), border);
            drawingContext.DrawText(text, new Point(offset - (text.Width / 2.0), -26));
        }

        private string GetText()
        {
            string valueText = _slider.Value.ToString();
            if (!string.IsNullOrWhiteSpace(_unit))
                valueText += " " + _unit;

            return valueText;
        }

        private Geometry CreateBorder(double offset, double width, double height)
        {
            double halfWidth = width / 2.0;

            PathGeometry borderGeometry = new PathGeometry();
            PathFigure border = new PathFigure
            {
                StartPoint = new Point(offset, 0),
                IsClosed = true,
                IsFilled = true
            };

            border.Segments.Add(new LineSegment(new Point(offset + 4, -6), true));
            border.Segments.Add(new LineSegment(new Point(offset + 4 + halfWidth, -6), true));
            border.Segments.Add(new LineSegment(new Point(offset + 4 + halfWidth, -10 - height), true));
            border.Segments.Add(new LineSegment(new Point(offset - 4 - halfWidth, -10 - height), true));
            border.Segments.Add(new LineSegment(new Point(offset - 4 - halfWidth, -6), true));
            border.Segments.Add(new LineSegment(new Point(offset - 4, -6), true));

            borderGeometry.Figures.Add(border);

            return borderGeometry;
        }

        #endregion
    }
}
