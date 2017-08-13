using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using KeyboardAudioVisualizer.AudioProcessing.Equalizer;
using KeyboardAudioVisualizer.Helper;

namespace KeyboardAudioVisualizer.UI.Visualization
{
    [TemplatePart(Name = "PART_Grips", Type = typeof(ItemsControl))]
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

        // ReSharper restore InconsistentNaming
        #endregion

        #region Properties & Fields

        private ItemsControl _grips;
        private EqualizerBand _draggingBand;

        #endregion

        #region Constructors


        #endregion

        #region Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _grips = GetTemplateChild("PART_Grips") as ItemsControl;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            EqualizerBand band = GetClickedBand();
            if (band != null)
                _draggingBand = band;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            _draggingBand = null;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            _draggingBand = null;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_draggingBand == null) return;

            UpdateBand(_draggingBand, e.GetPosition(_grips));
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);

            if (_grips == null) return;

            EqualizerBand band = GetClickedBand();
            if (band == null)
            {
                EqualizerBand newBand = Equalizer.AddBand(0, 0);
                UpdateBand(newBand, e.GetPosition(_grips));
            }
            else
                Equalizer.RemoveBandBand(band);
        }

        private void UpdateBand(EqualizerBand band, Point position)
        {
            double halfHeight = _grips.ActualHeight / 2.0;

            band.Offset = (float)(position.X / _grips.ActualWidth);
            band.Value = (float)(-(position.Y - halfHeight) / halfHeight);
        }

        private EqualizerBand GetClickedBand()
        {
            ItemsPresenter itemsPresenter = _grips.GetVisualChild<ItemsPresenter>();
            if (itemsPresenter == null) return null;

            Panel panel = VisualTreeHelper.GetChild(itemsPresenter, 0) as Panel;
            if (panel == null) return null;

            foreach (UIElement element in panel.Children)
                if (element.IsMouseOver)
                {
                    EqualizerBand band = ((element as ContentPresenter)?.Content as EqualizerBand);
                    if (band != null) return band;
                }

            return null;
        }

        #endregion
    }
}
