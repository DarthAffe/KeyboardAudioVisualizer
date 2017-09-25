using System.ComponentModel;
using RGB.NET.Core;

namespace KeyboardAudioVisualizer.Configuration
{
    public interface IConfiguration : INotifyPropertyChanged
    {
        IBrush Brush { get; set; }
    }
}
