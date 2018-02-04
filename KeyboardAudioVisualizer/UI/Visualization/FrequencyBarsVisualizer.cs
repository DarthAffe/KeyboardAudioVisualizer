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
using Rectangle = System.Windows.Shapes.Rectangle;

namespace KeyboardAudioVisualizer.UI.Visualization
{
    [TemplatePart(Name = "PART_BarsPanel", Type = typeof(Panel))]
    public class FrequencyBarsVisualizer : Control
    {
        #region DependencyProperties
        // ReSharper disable InconsistentNaming

        public static readonly DependencyProperty VisualizationProviderProperty = DependencyProperty.Register(
            "VisualizationProvider", typeof(IVisualizationProvider), typeof(FrequencyBarsVisualizer),
            new PropertyMetadata(default(IVisualizationProvider), VisualizationProviderChanged));

        public IVisualizationProvider VisualizationProvider
        {
            get => (IVisualizationProvider)GetValue(VisualizationProviderProperty);
            set => SetValue(VisualizationProviderProperty, value);
        }

        public static readonly DependencyProperty VisualizationIndexProperty = DependencyProperty.Register(
            "VisualizationIndex", typeof(VisualizationIndex?), typeof(FrequencyBarsVisualizer), new PropertyMetadata(null, VisualizationIndexChanged));

        public VisualizationIndex? VisualizationIndex
        {
            get => (VisualizationIndex?)GetValue(VisualizationIndexProperty);
            set => SetValue(VisualizationIndexProperty, value);
        }

        // ReSharper restore InconsistentNaming
        #endregion

        #region Properties & Fields

        private LinearGradient _gradient;
        private Panel _panel;
        private Rectangle[] _bars = new Rectangle[0];

        #endregion

        #region Constructors

        public FrequencyBarsVisualizer()
        {
            RGBSurface.Instance.Updated += args => Dispatcher.BeginInvoke(new Action(Update), DispatcherPriority.Normal);
            SizeChanged += (sender, args) => UpdateSizes();
        }

        #endregion

        #region Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _panel = GetTemplateChild("PART_BarsPanel") as Panel;

            InitializeBars();
        }

        private static void VisualizationProviderChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (!(dependencyObject is FrequencyBarsVisualizer visualizer)) return;

            void ConfigurationOnPropertyChanged(object sender, PropertyChangedEventArgs args) => visualizer.ConfigurationChanged(args.PropertyName);

            if (dependencyPropertyChangedEventArgs.OldValue is IVisualizationProvider oldVisualizationProvider)
                oldVisualizationProvider.Configuration.PropertyChanged -= ConfigurationOnPropertyChanged;

            if (dependencyPropertyChangedEventArgs.NewValue is IVisualizationProvider newVisualizationProvider)
                newVisualizationProvider.Configuration.PropertyChanged += ConfigurationOnPropertyChanged;
        }

        private static void VisualizationIndexChanged(DependencyObject dependencyObject,
                                                      DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (!(dependencyObject is FrequencyBarsVisualizer visualizer)) return;
            visualizer.UpdateGradient();
        }

        private void UpdateGradient()
        {
            void GradientChanged(object sender, EventArgs args) => UpdateColors();
            if (_gradient != null)
                _gradient.GradientChanged -= GradientChanged;

            _gradient = VisualizationIndex.HasValue ? ApplicationManager.Instance.Settings[VisualizationIndex.Value].Gradient : null;
            if (_gradient != null)
                _gradient.GradientChanged += GradientChanged;

            UpdateColors();
        }

        private void ConfigurationChanged(string changedPropertyName)
        {
            if ((changedPropertyName == null) || (changedPropertyName == nameof(FrequencyBarsVisualizationProviderConfiguration.Bars)))
                InitializeBars();
        }

        private void InitializeBars()
        {
            if (_panel == null) return;

            _panel.Children.Clear();
            if (VisualizationProvider == null) return;

            _bars = new Rectangle[((FrequencyBarsVisualizationProviderConfiguration)VisualizationProvider.Configuration).Bars];

            for (int i = 0; i < _bars.Length; i++)
            {
                _bars[i] = new Rectangle { VerticalAlignment = VerticalAlignment.Bottom, Width = 0 };
                _panel.Children.Add(_bars[i]);
            }

            UpdateSizes();
            UpdateColors();
        }

        private void UpdateSizes()
        {
            if (_bars.Length == 0) return;

            double barSpacing = ActualWidth / _bars.Length;
            double barWidth = (barSpacing * 3.0) / 4.0;
            double margin = barSpacing - barWidth;

            for (int i = 0; i < _bars.Length; i++)
            {
                _bars[i].Width = barWidth;
                _bars[i].Margin = new Thickness(margin / 2, 0, margin / 2, 0);
            }
        }

        private void UpdateColors()
        {
            if (_gradient == null) return;

            for (int i = 0; i < _bars.Length; i++)
            {
                RGB.NET.Core.Color color = _gradient.GetColor((double)i / _bars.Length);
                _bars[i].Fill = new SolidColorBrush(Color.FromRgb(color.R, color.G, color.B));
            }
        }

        private void Update()
        {
            IVisualizationProvider visualizationProvider = VisualizationProvider;
            if ((visualizationProvider == null) || (Visibility != Visibility.Visible)) return;

            int count = Math.Min(_bars.Length, visualizationProvider.VisualizationData.Length);
            for (int i = 0; i < count; i++)
            {
                _bars[i].Height = (float)((_bars[i].Height * 0.5) + ((ActualHeight * visualizationProvider.VisualizationData[i]) * (1.0 - 0.5)));
                if (double.IsNaN(_bars[i].Height)) _bars[i].Height = 0;
            }
        }

        #endregion
    }
}
