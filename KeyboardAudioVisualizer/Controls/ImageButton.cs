using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace KeyboardAudioVisualizer.Controls
{
    public class ImageButton : Button
    {
        #region Properties & Fields
        // ReSharper disable InconsistentNaming

        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(
            "Image", typeof(ImageSource), typeof(ImageButton), new PropertyMetadata(default(ImageSource)));

        public ImageSource Image
        {
            get => (ImageSource)GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }

        public static readonly DependencyProperty HoverImageProperty = DependencyProperty.Register(
            "HoverImage", typeof(ImageSource), typeof(ImageButton), new PropertyMetadata(default(ImageSource)));

        public ImageSource HoverImage
        {
            get => (ImageSource)GetValue(HoverImageProperty);
            set => SetValue(HoverImageProperty, value);
        }

        public static readonly DependencyProperty PressedImageProperty = DependencyProperty.Register(
            "PressedImage", typeof(ImageSource), typeof(ImageButton), new PropertyMetadata(default(ImageSource)));

        public ImageSource PressedImage
        {
            get => (ImageSource)GetValue(PressedImageProperty);
            set => SetValue(PressedImageProperty, value);
        }

        // ReSharper restore InconsistentNaming
        #endregion

        #region Constructors

        static ImageButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageButton), new FrameworkPropertyMetadata(typeof(ImageButton)));
        }

        #endregion
    }
}
