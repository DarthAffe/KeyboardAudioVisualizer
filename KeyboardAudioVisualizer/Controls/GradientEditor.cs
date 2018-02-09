using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using RGB.NET.Brushes.Gradients;
using RGB.NET.Core;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using Size = System.Windows.Size;
using Rectangle = System.Windows.Shapes.Rectangle;
using GradientStop = RGB.NET.Brushes.Gradients.GradientStop;

namespace KeyboardAudioVisualizer.Controls
{
    [TemplatePart(Name = "PART_Gradient", Type = typeof(Canvas))]
    [TemplatePart(Name = "PART_Stops", Type = typeof(Canvas))]
    public class GradientEditor : Control
    {
        #region Properties & Fields

        private Canvas _gradientContainer;
        private Canvas _stopContainer;
        private readonly List<Rectangle> _previewRectangles = new List<Rectangle>();
        private readonly Dictionary<GradientStop, ContentControl> _stops = new Dictionary<GradientStop, ContentControl>();
        private ContentControl _draggingStop;
        private AdornerLayer _adornerLayer;
        private ColorPickerAdorner _adorner;
        private Window _window;

        #endregion

        #region DepdencyProperties

        public static readonly DependencyProperty GradientProperty = DependencyProperty.Register(
            "Gradient", typeof(LinearGradient), typeof(GradientEditor), new FrameworkPropertyMetadata(null,
                                                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                                                                                                      OnGradientChanged));

        public LinearGradient Gradient
        {
            get => (LinearGradient)GetValue(GradientProperty);
            set => SetValue(GradientProperty, value);
        }

        public static readonly DependencyProperty GradientStopStyleProperty = DependencyProperty.Register(
            "GradientStopStyle", typeof(Style), typeof(GradientEditor), new PropertyMetadata(default(Style)));

        public Style GradientStopStyle
        {
            get => (Style)GetValue(GradientStopStyleProperty);
            set => SetValue(GradientStopStyleProperty, value);
        }

        public static readonly DependencyProperty SelectedStopProperty = DependencyProperty.Register(
            "SelectedStop", typeof(GradientStop), typeof(GradientEditor), new PropertyMetadata(default(GradientStop), SelectedStopChanged));

        public GradientStop SelectedStop
        {
            get => (GradientStop)GetValue(SelectedStopProperty);
            set => SetValue(SelectedStopProperty, value);
        }

        public static readonly DependencyProperty ColorSelectorTemplateProperty = DependencyProperty.Register(
            "ColorSelectorTemplate", typeof(DataTemplate), typeof(GradientEditor), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate ColorSelectorTemplate
        {
            get => (DataTemplate)GetValue(ColorSelectorTemplateProperty);
            set => SetValue(ColorSelectorTemplateProperty, value);
        }

        public static readonly DependencyProperty CanAddOrDeleteStopsProperty = DependencyProperty.Register(
            "CanAddOrDeleteStops", typeof(bool), typeof(GradientEditor), new PropertyMetadata(true));

        public bool CanAddOrDeleteStops
        {
            get => (bool)GetValue(CanAddOrDeleteStopsProperty);
            set => SetValue(CanAddOrDeleteStopsProperty, value);
        }

        #endregion

        #region AttachedProperties

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.RegisterAttached(
            "IsSelected", typeof(bool), typeof(GradientEditor), new PropertyMetadata(default(bool)));

        public static void SetIsSelected(DependencyObject element, bool value) => element.SetValue(IsSelectedProperty, value);
        public static bool GetIsSelected(DependencyObject element) => (bool)element.GetValue(IsSelectedProperty);

        #endregion

        #region Constructors

        public GradientEditor()
        {
            if (Gradient == null)
                Gradient = new LinearGradient();
        }

        #endregion

        #region Methods

        public override void OnApplyTemplate()
        {
            if ((_gradientContainer = GetTemplateChild("PART_Gradient") as Canvas) != null)
            {
                _gradientContainer.SizeChanged += (sender, args) => UpdateGradientPreview();
                _gradientContainer.MouseDown += GradientContainerOnMouseDown;
            }

            if ((_stopContainer = GetTemplateChild("PART_Stops") as Canvas) != null)
                _stopContainer.SizeChanged += (sender, args) => UpdateGradientStops();

            _adornerLayer = AdornerLayer.GetAdornerLayer(this);
            _window = Window.GetWindow(this);
            if (_window != null)
            {
                _window.PreviewMouseDown += WindowMouseDown;
                _window.PreviewKeyDown += (sender, args) =>
                {
                    if (args.Key == Key.Escape)
                        SelectedStop = null;
                };
            }

            UpdateGradientPreview();
            UpdateGradientStops();
        }

        private void UpdateGradientPreview()
        {
            if ((_gradientContainer == null) || (Gradient == null)) return;

            List<GradientStop> gradientStops = Gradient.GradientStops.OrderBy(x => x.Offset).ToList();
            if (gradientStops.Count == 0)
                UpdatePreviewRectangleCount(gradientStops.Count);
            else if (gradientStops.Count == 1)
            {
                UpdatePreviewRectangleCount(gradientStops.Count);
                GradientStop firstStop = gradientStops[0];
                UpdatePreviewRectangle(_previewRectangles[0], _gradientContainer.ActualWidth, _gradientContainer.ActualHeight, 0, 1, firstStop.Color, firstStop.Color);
            }
            else
            {
                UpdatePreviewRectangleCount(gradientStops.Count + 1);

                GradientStop firstStop = gradientStops[0];
                UpdatePreviewRectangle(_previewRectangles[0], _gradientContainer.ActualWidth, _gradientContainer.ActualHeight, 0, firstStop.Offset, firstStop.Color, firstStop.Color);
                for (int i = 0; i < (gradientStops.Count - 1); i++)
                {
                    GradientStop stop = gradientStops[i];
                    GradientStop nextStop = gradientStops[i + 1];
                    Rectangle rect = _previewRectangles[i + 1];
                    UpdatePreviewRectangle(rect, _gradientContainer.ActualWidth, _gradientContainer.ActualHeight, stop.Offset, nextStop.Offset, stop.Color, nextStop.Color);
                }
                GradientStop lastStop = gradientStops[gradientStops.Count - 1];
                UpdatePreviewRectangle(_previewRectangles[_previewRectangles.Count - 1], _gradientContainer.ActualWidth, _gradientContainer.ActualHeight, lastStop.Offset, 1, lastStop.Color, lastStop.Color);
            }
        }

        private void UpdatePreviewRectangle(Rectangle rect, double referenceWidth, double referenceHeight, double from, double to,
                                            RGB.NET.Core.Color startColor, RGB.NET.Core.Color endColor)
        {
            rect.Fill = new LinearGradientBrush(Color.FromArgb(startColor.A, startColor.R, startColor.G, startColor.B),
                                                Color.FromArgb(endColor.A, endColor.R, endColor.G, endColor.B),
                                                new Point(0, 0.5), new Point(1, 0.5));

            //DarthAffe 09.02.2018: Forced rounding to prevent render issues on resize
            Canvas.SetLeft(rect, Math.Floor(referenceWidth * from.Clamp(0, 1)));
            rect.Width = Math.Ceiling(referenceWidth * (to.Clamp(0, 1) - from.Clamp(0, 1)));

            Canvas.SetTop(rect, 0);
            rect.Height = referenceHeight;
        }

        private void UpdatePreviewRectangleCount(int gradientCount)
        {
            int countDiff = gradientCount - _previewRectangles.Count;
            if (countDiff > 0)
                for (int i = 0; i < countDiff; i++)
                {
                    Rectangle rect = new Rectangle { VerticalAlignment = VerticalAlignment.Stretch };
                    _previewRectangles.Add(rect);
                    _gradientContainer.Children.Add(rect);
                }

            if (countDiff < 0)
                for (int i = 0; i < Math.Abs(countDiff); i++)
                {
                    int index = _previewRectangles.Count - i - 1;
                    Rectangle rect = _previewRectangles[index];
                    _previewRectangles.RemoveAt(index);
                    _gradientContainer.Children.Remove(rect);
                }
        }

        private void UpdateGradientStops()
        {
            if (Gradient == null) return;

            List<GradientStop> gradientStops = Gradient.GradientStops.OrderBy(x => x.Offset).ToList();
            UpdateGradientStopsCount(gradientStops);
            foreach (GradientStop stop in gradientStops)
                UpdateGradientStop(_stops[stop], _stopContainer.ActualWidth, _stopContainer.ActualHeight, stop);
        }

        private void UpdateGradientStop(ContentControl control, double referenceWidth, double referenceHeight, GradientStop stop)
        {
            control.Background = new SolidColorBrush(Color.FromArgb(stop.Color.A, stop.Color.R, stop.Color.G, stop.Color.B));

            Canvas.SetLeft(control, (referenceWidth * stop.Offset.Clamp(0, 1)) - (control.Width / 2.0));

            Canvas.SetTop(control, 0);
            control.Height = referenceHeight;
        }

        private void UpdateGradientStopsCount(List<GradientStop> gradientStops)
        {
            foreach (GradientStop stop in gradientStops)
            {
                if (!_stops.ContainsKey(stop))
                {
                    ContentControl control = new ContentControl
                    {
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Style = GradientStopStyle,
                        Content = stop
                    };
                    control.MouseDown += GradientStopOnMouseDown;
                    _stops.Add(stop, control);
                    _stopContainer.Children.Add(control);
                }
            }

            List<GradientStop> stopsToRemove = new List<GradientStop>();
            foreach (KeyValuePair<GradientStop, ContentControl> stopPair in _stops)
                if (!gradientStops.Contains(stopPair.Key))
                {
                    ContentControl control = stopPair.Value;
                    control.MouseDown -= GradientStopOnMouseDown;
                    stopsToRemove.Add(stopPair.Key);
                    _stopContainer.Children.Remove(control);
                }

            foreach (GradientStop stop in stopsToRemove)
                _stops.Remove(stop);
        }

        private void AttachGradient(AbstractGradient gradient) => gradient.GradientChanged += GradientChanged;
        private void DetachGradient(AbstractGradient gradient) => gradient.GradientChanged -= GradientChanged;

        private void GradientChanged(object o, EventArgs eventArgs)
        {
            UpdateGradientPreview();
            UpdateGradientStops();
        }

        private static void OnGradientChanged(DependencyObject dependencyObject,
                                              DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (!(dependencyObject is GradientEditor ge)) return;

            if (dependencyPropertyChangedEventArgs.OldValue is AbstractGradient oldGradient)
                ge.DetachGradient(oldGradient);

            if (dependencyPropertyChangedEventArgs.NewValue is AbstractGradient newGradient)
                ge.AttachGradient(newGradient);
        }

        private void GradientContainerOnMouseDown(object o, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if ((mouseButtonEventArgs.ChangedButton != MouseButton.Left) || (Gradient == null) || !CanAddOrDeleteStops) return;

            double offset = mouseButtonEventArgs.GetPosition(_gradientContainer).X / _gradientContainer.ActualWidth;
            RGB.NET.Core.Color color = Gradient.GetColor(offset);
            GradientStop newStop = new GradientStop(offset, color);
            Gradient.GradientStops.Add(newStop);
            SelectedStop = newStop;
        }

        private void GradientStopOnMouseDown(object o, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (!((o as ContentControl)?.Content is GradientStop stop) || (Gradient == null)) return;

            if (mouseButtonEventArgs.ChangedButton == MouseButton.Right)
            {
                if (CanAddOrDeleteStops)
                    Gradient.GradientStops.Remove(stop);
            }
            else if (mouseButtonEventArgs.ChangedButton == MouseButton.Left)
            {
                SelectedStop = stop;
                _draggingStop = (ContentControl)o;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_draggingStop?.Content is GradientStop stop)
            {
                double location = e.GetPosition(_gradientContainer).X;
                stop.Offset = (location / _gradientContainer.ActualWidth).Clamp(0, 1);
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            _draggingStop = null;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            _draggingStop = null;
        }

        private static void SelectedStopChanged(DependencyObject dependencyObject,
                                                DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (!(dependencyObject is GradientEditor gradientEditor)) return;

            if (gradientEditor._adorner != null)
                gradientEditor._adornerLayer.Remove(gradientEditor._adorner);

            if (dependencyPropertyChangedEventArgs.OldValue is GradientStop oldStop)
            {
                if (gradientEditor._stops.TryGetValue(oldStop, out ContentControl oldcontrol))
                    SetIsSelected(oldcontrol, false);
            }

            if (dependencyPropertyChangedEventArgs.NewValue is GradientStop stop)
            {
                ContentControl stopContainer = gradientEditor._stops[stop];
                SetIsSelected(stopContainer, true);

                if (gradientEditor._adornerLayer != null)
                {
                    ContentControl contentControl = new ContentControl
                    {
                        ContentTemplate = gradientEditor.ColorSelectorTemplate,
                        Content = stop
                    };

                    ColorPickerAdorner adorner = new ColorPickerAdorner(stopContainer, contentControl);
                    gradientEditor._adorner = adorner;
                    gradientEditor._adornerLayer.Add(adorner);
                }
            }
        }

        private void WindowMouseDown(object o, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if ((_adorner != null) && (VisualTreeHelper.HitTest(_adorner, mouseButtonEventArgs.GetPosition(_adorner)) == null))
                SelectedStop = null;
        }

        #endregion
    }

    public class ColorPickerAdorner : Adorner
    {
        #region Properties & Fields

        private readonly VisualCollection _visualChildren;
        private readonly FrameworkElement _colorSelector;
        protected override int VisualChildrenCount => 1;
        protected override Visual GetVisualChild(int index) => _colorSelector;

        #endregion

        #region Constructors

        public ColorPickerAdorner(UIElement adornedElement, FrameworkElement colorSelector)
            : base(adornedElement)
        {
            this._colorSelector = colorSelector;

            _visualChildren = new VisualCollection(this) { colorSelector };
        }

        #endregion

        #region Methods

        protected override Size ArrangeOverride(Size finalSize)
        {
            Window referenceWindow = Window.GetWindow(AdornedElement);
            Point referenceLocation = AdornedElement.TranslatePoint(new Point(0, 0), referenceWindow);

            double referenceWidth = ((FrameworkElement)AdornedElement).ActualWidth / 2.0;
            double referenceHeight = ((FrameworkElement)AdornedElement).Height;
            double referenceX = referenceLocation.X + referenceWidth;
            double halfWidth = finalSize.Width / 2.0;
            double maxOffset = referenceWindow.Width - halfWidth;

            double offset = (referenceX < halfWidth ? referenceX
                                                    : (((referenceX + (referenceWidth * 2)) > maxOffset)
                                                        ? halfWidth - ((maxOffset - referenceX) - (referenceWidth * 2))
                                                        : halfWidth));

            _colorSelector.Arrange(new Rect(new Point(referenceWidth - offset, referenceHeight), finalSize));
            return _colorSelector.RenderSize;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            _colorSelector.Measure(constraint);
            return _colorSelector.DesiredSize;
        }

        #endregion
    }
}
