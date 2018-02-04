using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using RGB.NET.Core;
using Color = RGB.NET.Core.Color;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;
using WpfColor = System.Windows.Media.Color;

namespace KeyboardAudioVisualizer.Controls
{
    [TemplatePart(Name = "PART_Selector", Type = typeof(Panel))]
    [TemplatePart(Name = "PART_SliderAlpha", Type = typeof(Slider))]
    [TemplatePart(Name = "PART_SliderRed", Type = typeof(Slider))]
    [TemplatePart(Name = "PART_SliderGreen", Type = typeof(Slider))]
    [TemplatePart(Name = "PART_SliderBlue", Type = typeof(Slider))]
    [TemplatePart(Name = "PART_SliderHue", Type = typeof(Slider))]
    [TemplatePart(Name = "PART_SliderSaturation", Type = typeof(Slider))]
    [TemplatePart(Name = "PART_SliderValue", Type = typeof(Slider))]
    [TemplatePart(Name = "PART_Preview", Type = typeof(Rectangle))]
    public class ColorSelector : Control
    {
        #region Properties & Fields

        private bool _ignorePropertyChanged;
        private bool _dragSelector;

        private byte _a;
        private byte _r;
        private byte _g;
        private byte _b;
        private double _hue;
        private double _saturation;
        private double _value;

        private Panel _selector;
        private Rectangle _selectorColor;
        private Grid _selectorGrip;
        private Slider _sliderAlpha;
        private Slider _sliderRed;
        private Slider _sliderGreen;
        private Slider _sliderBlue;
        private Slider _sliderHue;
        private Slider _sliderSaturation;
        private Slider _sliderValue;
        private Rectangle _preview;

        private SolidColorBrush _previewBrush;
        private SolidColorBrush _selectorBrush;
        private LinearGradientBrush _alphaBrush;
        private LinearGradientBrush _redBrush;
        private LinearGradientBrush _greenBrush;
        private LinearGradientBrush _blueBrush;
        private LinearGradientBrush _hueBrush;
        private LinearGradientBrush _saturationBrush;
        private LinearGradientBrush _valueBrush;

        #endregion

        #region DependencyProperties

        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register(
            "SelectedColor", typeof(Color), typeof(ColorSelector), new FrameworkPropertyMetadata(new Color(255, 0, 0),
                                                                                                 FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                                                                                                 SelectedColorChanged));

        public Color SelectedColor
        {
            get => (Color)GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }

        #endregion

        #region Methods

        public override void OnApplyTemplate()
        {
            if ((_selector = GetTemplateChild("PART_Selector") as Panel) != null)
            {
                _selectorBrush = new SolidColorBrush();
                _selectorColor = new Rectangle
                {
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    SnapsToDevicePixels = true,
                    StrokeThickness = 0,
                    Fill = _selectorBrush
                };
                _selector.Children.Add(_selectorColor);

                Rectangle selectorWhite = new Rectangle
                {
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    SnapsToDevicePixels = true,
                    StrokeThickness = 0,
                    Fill = new LinearGradientBrush(WpfColor.FromRgb(255, 255, 255), WpfColor.FromArgb(0, 255, 255, 255), new Point(0, 0.5), new Point(1, 0.5))
                };
                _selector.Children.Add(selectorWhite);

                Rectangle selectorBlack = new Rectangle
                {
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    SnapsToDevicePixels = true,
                    StrokeThickness = 0,
                    Fill = new LinearGradientBrush(WpfColor.FromRgb(0, 0, 0), WpfColor.FromArgb(0, 0, 0, 0), new Point(0.5, 1), new Point(0.5, 0))
                };
                _selector.Children.Add(selectorBlack);

                _selectorGrip = new Grid
                {
                    VerticalAlignment = VerticalAlignment.Bottom,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    SnapsToDevicePixels = true
                };
                _selectorGrip.Children.Add(new Ellipse
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    SnapsToDevicePixels = true,
                    Stroke = new SolidColorBrush(WpfColor.FromRgb(0, 0, 0)),
                    StrokeThickness = 2,
                    Fill = null,
                    Width = 12,
                    Height = 12
                });
                _selectorGrip.Children.Add(new Ellipse
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    SnapsToDevicePixels = true,
                    Stroke = new SolidColorBrush(WpfColor.FromRgb(255, 255, 255)),
                    StrokeThickness = 1,
                    Fill = null,
                    Width = 10,
                    Height = 10
                });
                _selector.Children.Add(_selectorGrip);

                _selector.SizeChanged += (sender, args) => UpdateSelector();
                _selector.MouseLeftButtonDown += (sender, args) =>
                                                 {
                                                     _dragSelector = true;
                                                     UpdateSelectorValue(args.GetPosition(_selector));
                                                 };
                _selector.MouseEnter += (sender, args) =>
                                        {
                                            if (args.LeftButton == MouseButtonState.Pressed)
                                            {
                                                _dragSelector = true;
                                                UpdateSelectorValue(args.GetPosition(_selector));
                                            }
                                        };
                _selector.MouseLeftButtonUp += (sender, args) => _dragSelector = false;
                _selector.MouseLeave += (sender, args) => _dragSelector = false;
                _selector.MouseMove += (sender, args) => UpdateSelectorValue(args.GetPosition(_selector));
                _selector.ClipToBounds = true;
            }

            if ((_sliderAlpha = GetTemplateChild("PART_SliderAlpha") as Slider) != null)
            {
                _alphaBrush = new LinearGradientBrush(new GradientStopCollection(new[] { new GradientStop(new WpfColor(), 0),
                                                                                         new GradientStop(new WpfColor(), 1) }));
                _sliderAlpha.Background = _alphaBrush;
                _sliderAlpha.ValueChanged += AChanged;
            }

            if ((_sliderRed = GetTemplateChild("PART_SliderRed") as Slider) != null)
            {
                _redBrush = new LinearGradientBrush(new GradientStopCollection(new[] { new GradientStop(new WpfColor(), 0),
                                                                                       new GradientStop(new WpfColor(), 1) }));
                _sliderRed.Background = _redBrush;
                _sliderRed.ValueChanged += RChanged;
            }

            if ((_sliderGreen = GetTemplateChild("PART_SliderGreen") as Slider) != null)
            {
                _greenBrush = new LinearGradientBrush(new GradientStopCollection(new[] { new GradientStop(new WpfColor(), 0),
                                                                                         new GradientStop(new WpfColor(), 1) }));
                _sliderGreen.Background = _greenBrush;
                _sliderGreen.ValueChanged += GChanged;
            }

            if ((_sliderBlue = GetTemplateChild("PART_SliderBlue") as Slider) != null)
            {
                _blueBrush = new LinearGradientBrush(new GradientStopCollection(new[] { new GradientStop(new WpfColor(), 0),
                                                                                        new GradientStop(new WpfColor(), 1) }));
                _sliderBlue.Background = _blueBrush;
                _sliderBlue.ValueChanged += BChanged;
            }

            if ((_sliderHue = GetTemplateChild("PART_SliderHue") as Slider) != null)
            {
                _hueBrush = new LinearGradientBrush(new GradientStopCollection(new[] { new GradientStop(new WpfColor(), 0),
                                                                                       new GradientStop(new WpfColor(), 1.0 / 6.0),
                                                                                       new GradientStop(new WpfColor(), 2.0 / 6.0),
                                                                                       new GradientStop(new WpfColor(), 3.0 / 6.0),
                                                                                       new GradientStop(new WpfColor(), 4.0 / 6.0),
                                                                                       new GradientStop(new WpfColor(), 5.0 / 6.0),
                                                                                       new GradientStop(new WpfColor(), 1) }));
                _sliderHue.Background = _hueBrush;
                _sliderHue.ValueChanged += HueChanged;
            }

            if ((_sliderSaturation = GetTemplateChild("PART_SliderSaturation") as Slider) != null)
            {
                _saturationBrush = new LinearGradientBrush(new GradientStopCollection(new[] { new GradientStop(new WpfColor(), 0),
                                                                                              new GradientStop(new WpfColor(), 1) }));
                _sliderSaturation.Background = _saturationBrush;
                _sliderSaturation.ValueChanged += SaturationChanged;
            }

            if ((_sliderValue = GetTemplateChild("PART_SliderValue") as Slider) != null)
            {
                _valueBrush = new LinearGradientBrush(new GradientStopCollection(new[] { new GradientStop(new WpfColor(), 0),
                                                                                         new GradientStop(new WpfColor(), 1) }));
                _sliderValue.Background = _valueBrush;
                _sliderValue.ValueChanged += ValueChanged;
            }

            if ((_preview = GetTemplateChild("PART_Preview") as Rectangle) != null)
            {
                _previewBrush = new SolidColorBrush();
                _preview.Fill = _previewBrush;
            }

            SetColor(SelectedColor);
        }

        private static void SelectedColorChanged(DependencyObject dependencyObject,
                                                 DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (!(dependencyObject is ColorSelector cs) || !(dependencyPropertyChangedEventArgs.NewValue is Color color)) return;
            cs.SetColor(color);
        }

        private void SetColor(Color color)
        {
            if (_ignorePropertyChanged) return;

            SetA(color);
            SetRGB(color);
            SetHSV(color);

            UpdateSelector();
            UpdateUIColors();
        }

        private void AChanged(object sender, RoutedPropertyChangedEventArgs<double> routedPropertyChangedEventArgs)
        {
            if (_ignorePropertyChanged) return;

            _a = (byte)routedPropertyChangedEventArgs.NewValue.Clamp(0, byte.MaxValue);
            Color color = new Color(_a, _r, _g, _b);
            UpdateSelectedColor(color);
            UpdateUIColors();
            UpdateSelector();
        }

        private void RChanged(object sender, RoutedPropertyChangedEventArgs<double> routedPropertyChangedEventArgs)
        {
            if (_ignorePropertyChanged) return;

            _r = (byte)routedPropertyChangedEventArgs.NewValue.Clamp(0, byte.MaxValue);
            RGBChanged();
        }

        private void GChanged(object sender, RoutedPropertyChangedEventArgs<double> routedPropertyChangedEventArgs)
        {
            if (_ignorePropertyChanged) return;

            _g = (byte)routedPropertyChangedEventArgs.NewValue.Clamp(0, byte.MaxValue);
            RGBChanged();
        }

        private void BChanged(object sender, RoutedPropertyChangedEventArgs<double> routedPropertyChangedEventArgs)
        {
            if (_ignorePropertyChanged) return;

            _b = (byte)routedPropertyChangedEventArgs.NewValue.Clamp(0, byte.MaxValue);
            RGBChanged();
        }

        private void RGBChanged()
        {
            Color color = new Color(_a, _r, _g, _b);
            UpdateSelectedColor(color);
            SetHSV(color);
            UpdateUIColors();
            UpdateSelector();
        }

        private void HueChanged(object sender, RoutedPropertyChangedEventArgs<double> routedPropertyChangedEventArgs)
        {
            if (_ignorePropertyChanged) return;

            _hue = routedPropertyChangedEventArgs.NewValue.Clamp(0, 360);
            HSVChanged();
        }

        private void SaturationChanged(object sender, RoutedPropertyChangedEventArgs<double> routedPropertyChangedEventArgs)
        {
            if (_ignorePropertyChanged) return;

            _saturation = routedPropertyChangedEventArgs.NewValue.Clamp(0, 1);
            HSVChanged();
        }

        private void ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> routedPropertyChangedEventArgs)
        {
            if (_ignorePropertyChanged) return;

            _value = routedPropertyChangedEventArgs.NewValue.Clamp(0, 1);
            HSVChanged();
        }

        private void HSVChanged()
        {
            Color color = Color.FromHSV(_a, _hue, _saturation, _value);
            UpdateSelectedColor(color);
            SetRGB(color);
            UpdateUIColors();
            UpdateSelector();
        }

        private void SetA(Color color)
        {
            _ignorePropertyChanged = true;

            _a = color.A;
            if (_sliderAlpha != null)
                _sliderAlpha.Value = _a;

            _ignorePropertyChanged = false;
        }

        private void SetRGB(Color color)
        {
            _ignorePropertyChanged = true;

            _r = color.R;
            if (_sliderRed != null)
                _sliderRed.Value = _r;

            _g = color.G;
            if (_sliderGreen != null)
                _sliderGreen.Value = _g;

            _b = color.B;
            if (_sliderBlue != null)
                _sliderBlue.Value = _b;

            _ignorePropertyChanged = false;
        }

        private void SetHSV(Color color)
        {
            _ignorePropertyChanged = true;

            _hue = color.Hue;
            if (_sliderHue != null)
                _sliderHue.Value = _hue;

            _saturation = color.Saturation;
            if (_sliderSaturation != null)
                _sliderSaturation.Value = _saturation;

            _value = color.Value;
            if (_sliderValue != null)
                _sliderValue.Value = _value;

            _ignorePropertyChanged = false;
        }

        private void UpdateSelectedColor(Color color)
        {
            _ignorePropertyChanged = true;

            SelectedColor = color;

            _ignorePropertyChanged = false;
        }

        private void UpdateSelector()
        {
            if (_selector == null) return;

            double selectorX = (_selector.ActualWidth * _saturation) - (_selectorGrip.ActualWidth / 2);
            double selectorY = (_selector.ActualHeight * _value) - (_selectorGrip.ActualHeight / 2);
            if (!double.IsNaN(selectorX) && !double.IsNaN(selectorY))
                _selectorGrip.Margin = new Thickness(selectorX, 0, 0, selectorY);
        }

        private void UpdateSelectorValue(Point mouseLocation)
        {
            if (!_dragSelector) return;

            double saturation = mouseLocation.X / _selector.ActualWidth;
            double value = 1 - (mouseLocation.Y / _selector.ActualHeight);
            if (!double.IsNaN(saturation) && !double.IsNaN(value))
            {
                _saturation = saturation;
                _value = value;
                HSVChanged();
            }
        }

        private void UpdateUIColors()
        {
            Color hueColor = Color.FromHSV(_hue, 1, 1);

            if (_previewBrush != null)
                _previewBrush.Color = WpfColor.FromArgb(_a, _r, _g, _b);

            if (_selectorBrush != null)
                _selectorBrush.Color = WpfColor.FromRgb(hueColor.R, hueColor.G, hueColor.B);

            if (_alphaBrush != null)
            {
                _alphaBrush.GradientStops[0].Color = WpfColor.FromArgb(0, _r, _g, _b);
                _alphaBrush.GradientStops[1].Color = WpfColor.FromArgb(255, _r, _g, _b);
            }

            if (_redBrush != null)
            {
                _redBrush.GradientStops[0].Color = WpfColor.FromArgb(_a, 0, _g, _b);
                _redBrush.GradientStops[1].Color = WpfColor.FromArgb(_a, 255, _g, _b);
            }

            if (_greenBrush != null)
            {
                _greenBrush.GradientStops[0].Color = WpfColor.FromArgb(_a, _r, 0, _b);
                _greenBrush.GradientStops[1].Color = WpfColor.FromArgb(_a, _r, 255, _b);
            }

            if (_blueBrush != null)
            {
                _blueBrush.GradientStops[0].Color = WpfColor.FromArgb(_a, _r, _g, 0);
                _blueBrush.GradientStops[1].Color = WpfColor.FromArgb(_a, _r, _g, 255);
            }

            if (_hueBrush != null)
            {
                Color referenceColor1 = Color.FromHSV(0, _saturation, _value);
                Color referenceColor2 = Color.FromHSV(60, _saturation, _value);
                Color referenceColor3 = Color.FromHSV(120, _saturation, _value);
                Color referenceColor4 = Color.FromHSV(180, _saturation, _value);
                Color referenceColor5 = Color.FromHSV(240, _saturation, _value);
                Color referenceColor6 = Color.FromHSV(300, _saturation, _value);

                _hueBrush.GradientStops[0].Color = WpfColor.FromArgb(_a, referenceColor1.R, referenceColor1.G, referenceColor1.B);
                _hueBrush.GradientStops[1].Color = WpfColor.FromArgb(_a, referenceColor2.R, referenceColor2.G, referenceColor2.B);
                _hueBrush.GradientStops[2].Color = WpfColor.FromArgb(_a, referenceColor3.R, referenceColor3.G, referenceColor3.B);
                _hueBrush.GradientStops[3].Color = WpfColor.FromArgb(_a, referenceColor4.R, referenceColor4.G, referenceColor4.B);
                _hueBrush.GradientStops[4].Color = WpfColor.FromArgb(_a, referenceColor5.R, referenceColor5.G, referenceColor5.B);
                _hueBrush.GradientStops[5].Color = WpfColor.FromArgb(_a, referenceColor6.R, referenceColor6.G, referenceColor6.B);
                _hueBrush.GradientStops[6].Color = WpfColor.FromArgb(_a, referenceColor1.R, referenceColor1.G, referenceColor1.B);
            }

            if (_saturationBrush != null)
            {
                Color referenceColor = Color.FromHSV(_hue, 1, _value);

                _saturationBrush.GradientStops[0].Color = WpfColor.FromArgb(_a, 255, 255, 255);
                _saturationBrush.GradientStops[1].Color = WpfColor.FromArgb(_a, referenceColor.R, referenceColor.G, referenceColor.B);
            }

            if (_valueBrush != null)
            {
                Color referenceColor = Color.FromHSV(_hue, _saturation, 1);

                _valueBrush.GradientStops[0].Color = WpfColor.FromArgb(_a, 0, 0, 0);
                _valueBrush.GradientStops[1].Color = WpfColor.FromArgb(_a, referenceColor.R, referenceColor.G, referenceColor.B);
            }
        }

        #endregion
    }
}
