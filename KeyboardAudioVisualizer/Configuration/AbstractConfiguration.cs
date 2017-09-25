using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using RGB.NET.Core;

namespace KeyboardAudioVisualizer.Configuration
{
    public class AbstractConfiguration : AbstractBindable, IConfiguration, INotifyPropertyChanged
    {
        #region Properties & Fields

        private IBrush _brush;
        public IBrush Brush
        {
            get => _brush;
            set => SetProperty(ref _brush, value);
        }

        #endregion

        #region Methods

        protected override bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if ((typeof(T) == typeof(double)) || (typeof(T) == typeof(float)))
            {
                if (Math.Abs((double)(object)storage - (double)(object)value) < 0.000001) return false;
            }
            else
            {
                if (Equals(storage, value)) return false;
            }

            storage = value;
            // ReSharper disable once ExplicitCallerInfoArgument
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }
}
