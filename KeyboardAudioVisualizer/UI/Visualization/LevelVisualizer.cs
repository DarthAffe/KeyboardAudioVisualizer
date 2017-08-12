using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using KeyboardAudioVisualizer.AudioProcessing.VisualizationProvider;
using RGB.NET.Core;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;

namespace KeyboardAudioVisualizer.UI.Visualization
{
    public class LevelVisualizer : Control
    {
        #region DependencyProperties
        // ReSharper disable InconsistentNaming

        public static readonly DependencyProperty VisualizationProviderProperty = DependencyProperty.Register(
            "VisualizationProvider", typeof(IVisualizationProvider), typeof(LevelVisualizer), new PropertyMetadata(default(IVisualizationProvider)));

        public IVisualizationProvider VisualizationProvider
        {
            get => (IVisualizationProvider)GetValue(VisualizationProviderProperty);
            set => SetValue(VisualizationProviderProperty, value);
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

            //TODO DarthAffe 12.08.2017: Create brush from config
            BrushLeft = new LinearGradientBrush(Color.FromRgb(255, 0, 0), Color.FromRgb(0, 0, 255), new Point(0, 0.5), new Point(1, 0.5));
            BrushRight = new LinearGradientBrush(Color.FromRgb(0, 0, 255), Color.FromRgb(255, 0, 0), new Point(0, 0.5), new Point(1, 0.5));
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

        #endregion
    }
}
