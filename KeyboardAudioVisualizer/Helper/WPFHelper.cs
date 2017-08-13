using System.Windows;
using System.Windows.Media;

namespace KeyboardAudioVisualizer.Helper
{
    public static class WPFHelper
    {
        #region Methods

        public static T GetVisualChild<T>(this DependencyObject parent)
            where T : Visual
        {
            T child = default(T);

            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;

                if (child == null)
                    child = GetVisualChild<T>(v);

                if (child != null)
                    break;
            }
            return child;
        }

        #endregion
    }
}
