using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KeyboardAudioVisualizer.Configuration
{
    public class AbstractConfiguration : INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Methods

        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion
    }
}
