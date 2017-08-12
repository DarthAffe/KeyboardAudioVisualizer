using System.Windows;
using System.Windows.Controls;
using KeyboardAudioVisualizer.AudioProcessing.Equalizer;

namespace KeyboardAudioVisualizer.UI.Visualization
{
    //[TemplatePart(Name = "PART_Grips", Type = typeof(Canvas))]
    public class EqualizerVisualizer : Control
    {
        #region DependencyProperties
        // ReSharper disable InconsistentNaming

        public static readonly DependencyProperty EqualizerProperty = DependencyProperty.Register(
            "Equalizer", typeof(IEqualizer), typeof(EqualizerVisualizer), new PropertyMetadata(default(IEqualizer)));

        public IEqualizer Equalizer
        {
            get => (IEqualizer)GetValue(EqualizerProperty);
            set => SetValue(EqualizerProperty, value);
        }

        public static readonly DependencyProperty ReferenceLevelProperty = DependencyProperty.Register(
            "ReferenceLevel", typeof(double), typeof(EqualizerVisualizer), new PropertyMetadata(default(double)));

        public double ReferenceLevel
        {
            get => (double)GetValue(ReferenceLevelProperty);
            set => SetValue(ReferenceLevelProperty, value);
        }

        // ReSharper restore InconsistentNaming
        #endregion

        #region Properties & Fields

        //private Canvas _canvas;

        #endregion

        #region Constructors

        //public EqualizerVisualizer()
        //{
        //    SizeChanged += (sender, args) => Update();
        //    Update();
        //}

        #endregion

        #region Methods

        //public override void OnApplyTemplate()
        //{
        //    base.OnApplyTemplate();

        //    _canvas = GetTemplateChild("PART_Grips") as Canvas;
        //}

        //private static void EqualizerChanged(DependencyObject dependencyObject,
        //    DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        //{
        //    EqualizerVisualizer visualizer = dependencyObject as EqualizerVisualizer;
        //    if (visualizer == null) return;

        //    void BandsChanged(object sender, NotifyCollectionChangedEventArgs args) => visualizer.Update();

        //    if (dependencyPropertyChangedEventArgs.OldValue is IEqualizer oldEqualizer)
        //        oldEqualizer.Bands.CollectionChanged -= BandsChanged;

        //    if (dependencyPropertyChangedEventArgs.NewValue is IEqualizer newEqualizer)
        //        newEqualizer.Bands.CollectionChanged += BandsChanged;
        //}

        //private void Update()
        //{
        //    if (_canvas == null) return;

        //    void OnBandChanged(object sender, PropertyChangedEventArgs args) => Update();

        //    foreach (object child in _canvas.Children)
        //    {
        //        EqualizerBand band = (child as ContentControl)?.Content as EqualizerBand;
        //        if (band == null) continue;
        //        band.PropertyChanged -= OnBandChanged;
        //    }

        //    _canvas.Children.Clear();

        //    foreach (EqualizerBand band in Equalizer.Bands)
        //    {
        //        ContentControl ctrl = new ContentControl();

        //        ctrl.Content = band;

        //        _canvas.Children.Add(ctrl);
        //    }
        //}

        #endregion
    }
}
