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
    public class BeatVisualizer : Control
    {
        #region DependencyProperties
        // ReSharper disable InconsistentNaming

        public static readonly DependencyProperty VisualizationProviderProperty = DependencyProperty.Register(
            "VisualizationProvider", typeof(IVisualizationProvider), typeof(BeatVisualizer), new PropertyMetadata(default(IVisualizationProvider)));

        public IVisualizationProvider VisualizationProvider
        {
            get => (IVisualizationProvider)GetValue(VisualizationProviderProperty);
            set => SetValue(VisualizationProviderProperty, value);
        }

        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(
            "Brush", typeof(Brush), typeof(BeatVisualizer), new PropertyMetadata(default(Brush)));

        public Brush Brush
        {
            get => (Brush)GetValue(BrushProperty);
            set => SetValue(BrushProperty, value);
        }

        public static readonly DependencyProperty BeatValueProperty = DependencyProperty.Register(
            "BeatValue", typeof(float), typeof(BeatVisualizer), new PropertyMetadata(default(float)));

        public float BeatValue
        {
            get => (float)GetValue(BeatValueProperty);
            set => SetValue(BeatValueProperty, value);
        }

        // ReSharper restore InconsistentNaming
        #endregion

        #region Constructors

        public BeatVisualizer()
        {
            RGBSurface.Instance.Updated += args => Dispatcher.BeginInvoke(new Action(Update), DispatcherPriority.Normal);

            //TODO DarthAffe 12.08.2017: Create brush from config
            Brush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        }

        #endregion

        #region Methods

        private void Update()
        {
            IVisualizationProvider visualizationProvider = VisualizationProvider;
            if ((visualizationProvider == null) || (Visibility != Visibility.Visible)) return;

            if (visualizationProvider.VisualizationData[0] > 0.9)
                BeatValue = 1f;
            else if (BeatValue > 0.01f)
            {
                float newValue = BeatValue * 0.625f;
                if (newValue > 0.01f)
                    BeatValue = newValue;
                else BeatValue = 0;
            }
        }

        #endregion
    }
}
