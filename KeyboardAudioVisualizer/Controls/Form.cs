using System;
using System.Windows;
using System.Windows.Controls;

namespace KeyboardAudioVisualizer.Controls
{
    public class Form : Panel
    {
        #region DependencyProperties
        // ReSharper disable InconsistentNaming

        public static readonly DependencyProperty RowHeightProperty = DependencyProperty.Register("RowHeight", typeof(double), typeof(Form),
            new FrameworkPropertyMetadata(24.0, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double RowHeight
        {
            get => (double)GetValue(RowHeightProperty);
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(RowHeight), "Row height can't be negative");
                SetValue(RowHeightProperty, value);
            }
        }

        public static readonly DependencyProperty LabelWidthProperty = DependencyProperty.Register("LabelWidth", typeof(double), typeof(Form),
            new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double LabelWidth
        {
            get => (double)GetValue(LabelWidthProperty);
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(RowHeight), "Label width can't be negative");
                SetValue(LabelWidthProperty, value);
            }
        }

        public static readonly DependencyProperty ElementSpacingProperty = DependencyProperty.Register("ElementSpacing", typeof(double), typeof(Form),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double ElementSpacing
        {
            get => (double)GetValue(ElementSpacingProperty);
            set => SetValue(ElementSpacingProperty, value);
        }

        public static readonly DependencyProperty RowSpacingProperty = DependencyProperty.Register("RowSpacing", typeof(double), typeof(Form),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double RowSpacing
        {
            get => (double)GetValue(RowSpacingProperty);
            set => SetValue(RowSpacingProperty, value);
        }

        // ReSharper restore InconsistentNaming
        #endregion

        #region AttachedProperties
        // ReSharper disable InconsistentNaming

        public static readonly DependencyProperty IsLabelProperty = DependencyProperty.RegisterAttached("IsLabel", typeof(bool), typeof(Form),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static void SetIsLabel(UIElement element, bool value) => element.SetValue(IsLabelProperty, value);
        public static bool GetIsLabel(UIElement element) => (bool)element.GetValue(IsLabelProperty);

        public static readonly DependencyProperty LineBreaksProperty = DependencyProperty.RegisterAttached("LineBreaks", typeof(int), typeof(Form),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static void SetLineBreaks(UIElement element, int value) => element.SetValue(LineBreaksProperty, value);
        public static int GetLineBreaks(UIElement element) => (int)element.GetValue(LineBreaksProperty);

        public static readonly DependencyProperty RowSpanProperty = DependencyProperty.RegisterAttached("RowSpan", typeof(int), typeof(Form),
            new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static void SetRowSpan(DependencyObject element, int value) => element.SetValue(RowSpanProperty, value);
        public static int GetRowSpan(DependencyObject element) => (int)element.GetValue(RowSpanProperty);

        public static readonly DependencyProperty FillProperty = DependencyProperty.RegisterAttached("Fill", typeof(bool), typeof(Form),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static void SetFill(DependencyObject element, bool value) => element.SetValue(FillProperty, value);
        public static bool GetFill(DependencyObject element) => (bool)element.GetValue(FillProperty);

        // ReSharper restore InconsistentNaming
        #endregion

        #region Methods

        protected override Size MeasureOverride(Size availableSize)
        {
            if (InternalChildren.Count == 0) return new Size(0, 0);

            FormLayout layout = new FormLayout(RowHeight, LabelWidth, ElementSpacing, RowSpacing);

            foreach (UIElement child in InternalChildren)
            {
                child.Measure(availableSize);
                layout.AddElement(child, 0);
            }

            return new Size(layout.Width, layout.Height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (InternalChildren.Count == 0) return new Size(0, 0);

            FormLayout layout = new FormLayout(RowHeight, LabelWidth, ElementSpacing, RowSpacing);

            foreach (UIElement child in InternalChildren)
                child.Arrange(layout.AddElement(child, finalSize.Width));

            return new Size(finalSize.Width, layout.Height);
        }

        #endregion

        #region Data

        private class FormLayout
        {
            #region Properties & Fields

            private readonly double _rowHeight;
            private readonly double _labelWidth;
            private readonly double _elementSpacing;
            private readonly double _rowSpacing;

            private double _currentRowWidth;

            private int _newRows = 0;
            private int _rows = -1;
            private double _currentMaxWidth;
            public double Width => Math.Max((Math.Max(_currentMaxWidth, _currentRowWidth) - _elementSpacing), 0);
            public double Height => ((_rows + 1) * _rowHeight) + (_rows * _rowSpacing);

            #endregion

            #region Constructors

            public FormLayout(double rowHeight, double labelWidth, double elementSpacing, double rowSpacing)
            {
                this._rowHeight = rowHeight;
                this._labelWidth = labelWidth;
                this._elementSpacing = elementSpacing;
                this._rowSpacing = rowSpacing;
            }

            #endregion

            #region Methods

            public Rect AddElement(UIElement element, double targetWidth)
            {
                bool isLabel = GetIsLabel(element);
                int lineBreaks = GetLineBreaks(element);
                int rowSpan = GetRowSpan(element);

                double elementWidth = isLabel ? _labelWidth : element.DesiredSize.Width;
                double height = _rowHeight;

                if (_newRows > 0)
                {
                    AddLineBreaks(_newRows);
                    _newRows = 0;
                }

                if (lineBreaks > 0) AddLineBreaks(lineBreaks);
                else if (isLabel) AddLineBreaks(1);
                else if (_rows < 0) _rows = 0;

                if (!isLabel && (_currentRowWidth < _labelWidth))
                    _currentRowWidth = _labelWidth + _elementSpacing;

                if (rowSpan > 1)
                {
                    height = (rowSpan * _rowHeight) + ((rowSpan - 1) * _rowSpacing);
                    _newRows = Math.Max(_newRows, rowSpan - 1);
                }

                if (element is FrameworkElement fe)
                    fe.MaxHeight = height;

                double width = elementWidth;
                if ((targetWidth >= 1) && GetFill(element))
                    width = targetWidth - _currentRowWidth;

                Rect rect = new Rect(new Point(_currentRowWidth, (_rows * _rowHeight) + (_rows * _rowSpacing)), new Size(width, height));

                _currentRowWidth += width + _elementSpacing;

                return rect;
            }

            private void AddLineBreaks(int count)
            {
                if (count <= 0) return;

                _currentMaxWidth = Math.Max(_currentMaxWidth, _currentRowWidth);

                _currentRowWidth = 0;
                _rows += count;
            }

            #endregion
        }

        #endregion
    }
}
