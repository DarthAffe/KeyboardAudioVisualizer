using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using KeyboardAudioVisualizer.AudioProcessing.VisualizationProvider;
using KeyboardAudioVisualizer.Helper;
using RGB.NET.Brushes.Gradients;
using RGB.NET.Core;

namespace KeyboardAudioVisualizer.UI.Visualization
{
    public class BeatVisualizer : Control
    {
        #region Properties & Fields

        private LinearGradient _gradient;

        #endregion

        #region DependencyProperties
        // ReSharper disable InconsistentNaming

        public static readonly DependencyProperty VisualizationProviderProperty = DependencyProperty.Register(
            "VisualizationProvider", typeof(IVisualizationProvider), typeof(BeatVisualizer), new PropertyMetadata(default(IVisualizationProvider)));

        public IVisualizationProvider VisualizationProvider
        {
            get => (IVisualizationProvider)GetValue(VisualizationProviderProperty);
            set => SetValue(VisualizationProviderProperty, value);
        }

        public static readonly DependencyProperty VisualizationIndexProperty = DependencyProperty.Register(
            "VisualizationIndex", typeof(VisualizationIndex?), typeof(BeatVisualizer), new PropertyMetadata(null, VisualizationIndexChanged));

        public VisualizationIndex? VisualizationIndex
        {
            get => (VisualizationIndex?)GetValue(VisualizationIndexProperty);
            set => SetValue(VisualizationIndexProperty, value);
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

        private static void VisualizationIndexChanged(DependencyObject dependencyObject,
                                                      DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (!(dependencyObject is BeatVisualizer visualizer)) return;
            visualizer.UpdateGradient();
        }

        private void UpdateGradient()
        {
            void GradientChanged(object sender, EventArgs args) => UpdateColor();
            if (_gradient != null)
                _gradient.GradientChanged -= GradientChanged;

            _gradient = VisualizationIndex.HasValue ? ApplicationManager.Instance.Settings[VisualizationIndex.Value].Gradient : null;
            if (_gradient != null)
                _gradient.GradientChanged += GradientChanged;

            UpdateColor();
        }

        private void UpdateColor()
        {
            if (_gradient == null) return;

            GradientStopCollection gradientStops = new GradientStopCollection();
            foreach (RGB.NET.Brushes.Gradients.GradientStop stop in _gradient.GradientStops)
                gradientStops.Add(new System.Windows.Media.GradientStop(System.Windows.Media.Color.FromArgb(stop.Color.GetA(), stop.Color.GetR(), stop.Color.GetG(), stop.Color.GetB()), stop.Offset));

            Brush = new LinearGradientBrush(gradientStops, new System.Windows.Point(0, 0.5), new System.Windows.Point(1, 0.5));
        }

        #endregion
    }
}
