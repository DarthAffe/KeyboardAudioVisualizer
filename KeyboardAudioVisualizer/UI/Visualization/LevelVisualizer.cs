using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using KeyboardAudioVisualizer.AudioProcessing.VisualizationProvider;
using KeyboardAudioVisualizer.Helper;
using RGB.NET.Brushes.Gradients;
using RGB.NET.Core;
using Color = System.Windows.Media.Color;
using GradientStop = RGB.NET.Brushes.Gradients.GradientStop;
using Point = System.Windows.Point;

namespace KeyboardAudioVisualizer.UI.Visualization
{
    public class LevelVisualizer : Control
    {
        #region Properties & Fields

        private LinearGradient _gradient;

        #endregion

        #region DependencyProperties
        // ReSharper disable InconsistentNaming

        public static readonly DependencyProperty VisualizationProviderProperty = DependencyProperty.Register(
            "VisualizationProvider", typeof(IVisualizationProvider), typeof(LevelVisualizer), new PropertyMetadata(default(IVisualizationProvider)));

        public IVisualizationProvider VisualizationProvider
        {
            get => (IVisualizationProvider)GetValue(VisualizationProviderProperty);
            set => SetValue(VisualizationProviderProperty, value);
        }

        public static readonly DependencyProperty VisualizationIndexProperty = DependencyProperty.Register(
            "VisualizationIndex", typeof(VisualizationIndex?), typeof(LevelVisualizer), new PropertyMetadata(null, VisualizationIndexChanged));

        public VisualizationIndex? VisualizationIndex
        {
            get => (VisualizationIndex?)GetValue(VisualizationIndexProperty);
            set => SetValue(VisualizationIndexProperty, value);
        }

        public static readonly DependencyProperty BrushLeftProperty = DependencyProperty.Register(
            "BrushLeft", typeof(Brush), typeof(LevelVisualizer), new PropertyMetadata(default(Brush)));

        public Brush BrushLeft
        {
            get => (Brush)GetValue(BrushLeftProperty);
            set => SetValue(BrushLeftProperty, value);
        }

        public static readonly DependencyProperty BrushRightProperty = DependencyProperty.Register(
            "BrushRight", typeof(Brush), typeof(LevelVisualizer), new PropertyMetadata(default(Brush)));

        public Brush BrushRight
        {
            get => (Brush)GetValue(BrushRightProperty);
            set => SetValue(BrushRightProperty, value);
        }

        public static readonly DependencyProperty SizeLeftProperty = DependencyProperty.Register(
            "SizeLeft", typeof(int), typeof(LevelVisualizer), new PropertyMetadata(default(int)));

        public int SizeLeft
        {
            get => (int)GetValue(SizeLeftProperty);
            set => SetValue(SizeLeftProperty, value);
        }

        public static readonly DependencyProperty SizeRightProperty = DependencyProperty.Register(
            "SizeRight", typeof(int), typeof(LevelVisualizer), new PropertyMetadata(default(int)));

        public int SizeRight
        {
            get => (int)GetValue(SizeRightProperty);
            set => SetValue(SizeRightProperty, value);
        }

        // ReSharper restore InconsistentNaming
        #endregion

        #region Constructors

        public LevelVisualizer()
        {
            RGBSurface.Instance.Updated += args => Dispatcher.BeginInvoke(new Action(Update), DispatcherPriority.Normal);
        }

        #endregion

        #region Methods

        private void Update()
        {
            IVisualizationProvider visualizationProvider = VisualizationProvider;
            if ((visualizationProvider == null) || (Visibility != Visibility.Visible)) return;

            int horizontalSizeLeft = (int)(visualizationProvider.VisualizationData[0] * (ActualWidth / 2));
            if (Math.Abs(SizeLeft - horizontalSizeLeft) > 1)
                SizeLeft = horizontalSizeLeft;

            int horizontalSizeRight = (int)(visualizationProvider.VisualizationData[1] * (ActualWidth / 2));
            if (Math.Abs(SizeRight - horizontalSizeRight) > 1)
                SizeRight = horizontalSizeRight;
        }

        private void SetBrushes()
        {
            if (_gradient == null) return;

            GradientStopCollection gradientStops = new GradientStopCollection();
            foreach (GradientStop stop in _gradient.GradientStops)
                gradientStops.Add(new System.Windows.Media.GradientStop(Color.FromArgb(stop.Color.GetA(), stop.Color.GetR(), stop.Color.GetG(), stop.Color.GetB()), stop.Offset));

            BrushLeft = new LinearGradientBrush(gradientStops, new Point(1, 0.5), new Point(0, 0.5));
            BrushRight = new LinearGradientBrush(gradientStops, new Point(0, 0.5), new Point(1, 0.5));
        }

        private static void VisualizationIndexChanged(DependencyObject dependencyObject,
                                                      DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (!(dependencyObject is LevelVisualizer visualizer)) return;
            visualizer.UpdateGradient();
        }

        private void UpdateGradient()
        {
            void GradientChanged(object sender, EventArgs args) => SetBrushes();
            if (_gradient != null)
                _gradient.GradientChanged -= GradientChanged;

            _gradient = VisualizationIndex.HasValue ? ApplicationManager.Instance.Settings[VisualizationIndex.Value].Gradient : null;
            if (_gradient != null)
                _gradient.GradientChanged += GradientChanged;

            SetBrushes();
        }

        #endregion
    }
}
