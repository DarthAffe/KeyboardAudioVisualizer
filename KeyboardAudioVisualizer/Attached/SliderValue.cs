using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace KeyboardAudioVisualizer.Attached
{
    public static class SliderValue
    {
        #region Properties & Fields
        // ReSharper disable InconsistentNaming

        public static readonly DependencyProperty UnitProperty = DependencyProperty.RegisterAttached(
            "Unit", typeof(string), typeof(SliderValue), new PropertyMetadata(default(string)));

        public static void SetUnit(DependencyObject element, string value) => element.SetValue(UnitProperty, value);
        public static string GetUnit(DependencyObject element) => (string)element.GetValue(UnitProperty);

        public static readonly DependencyProperty IsShownProperty = DependencyProperty.RegisterAttached(
            "IsShown", typeof(bool), typeof(SliderValue), new PropertyMetadata(default(bool), IsShownChanged));

        public static void SetIsShown(DependencyObject element, bool value) => element.SetValue(IsShownProperty, value);
        public static bool GetIsShown(DependencyObject element) => (bool)element.GetValue(IsShownProperty);

        public static readonly DependencyProperty BorderBrushProperty = DependencyProperty.RegisterAttached(
            "BorderBrush", typeof(Brush), typeof(SliderValue), new PropertyMetadata(default(Brush)));

        public static void SetBorderBrush(DependencyObject element, Brush value) => element.SetValue(BorderBrushProperty, value);
        public static Brush GetBorderBrush(DependencyObject element) => (Brush)element.GetValue(BorderBrushProperty);

        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.RegisterAttached(
            "Background", typeof(Brush), typeof(SliderValue), new PropertyMetadata(default(Brush)));

        public static void SetBackground(DependencyObject element, Brush value) => element.SetValue(BackgroundProperty, value);
        public static Brush GetBackground(DependencyObject element) => (Brush)element.GetValue(BackgroundProperty);

        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.RegisterAttached(
            "Foreground", typeof(Brush), typeof(SliderValue), new PropertyMetadata(default(Brush)));

        public static void SetForeground(DependencyObject element, Brush value) => element.SetValue(ForegroundProperty, value);
        public static Brush GetForeground(DependencyObject element) => (Brush)element.GetValue(ForegroundProperty);

        public static readonly DependencyProperty FontProperty = DependencyProperty.RegisterAttached(
            "Font", typeof(FontFamily), typeof(SliderValue), new PropertyMetadata(default(FontFamily)));

        public static void SetFont(DependencyObject element, FontFamily value) => element.SetValue(FontProperty, value);
        public static FontFamily GetFont(DependencyObject element) => (FontFamily)element.GetValue(FontProperty);

        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.RegisterAttached(
            "FontSize", typeof(double), typeof(SliderValue), new PropertyMetadata(default(double)));

        public static void SetFontSize(DependencyObject element, double value) => element.SetValue(FontSizeProperty, value);
        public static double GetFontSize(DependencyObject element) => (double)element.GetValue(FontSizeProperty);

        // ReSharper enable InconsistentNaming
        #endregion

        #region Methods

        private static void IsShownChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (!(dependencyObject is Slider slider)) return;

            if (dependencyPropertyChangedEventArgs.NewValue as bool? == true)
            {
                slider.MouseEnter += SliderOnMouseEnter;
                slider.MouseLeave += SliderOnMouseLeave;
            }
            else
            {
                slider.MouseEnter -= SliderOnMouseEnter;
                slider.MouseLeave -= SliderOnMouseLeave;
                RemoveAdorner(slider);
            }
        }

        private static void SliderOnMouseEnter(object sender, MouseEventArgs mouseEventArgs)
        {
            if (!(sender is Slider slider)) return;
            AdornerLayer.GetAdornerLayer(slider)?.Add(new SliderValueAdorner(slider, GetUnit(slider))
            {
                BorderBrush = GetBorderBrush(slider),
                Background = GetBackground(slider),
                Foreground = GetForeground(slider),
                Font = GetFont(slider),
                FontSize = GetFontSize(slider)
            });
        }

        private static void SliderOnMouseLeave(object sender, MouseEventArgs mouseEventArgs)
        {
            if (!(sender is Slider slider)) return;
            RemoveAdorner(slider);
        }

        private static void RemoveAdorner(Slider slider)
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(slider);
            Adorner adorner = adornerLayer?.GetAdorners(slider)?.FirstOrDefault(x => x is SliderValueAdorner);
            if (adorner != null)
            {
                adornerLayer.Remove(adorner);
                (adorner as SliderValueAdorner)?.Cleanup();
            }
        }

        #endregion
    }
}
