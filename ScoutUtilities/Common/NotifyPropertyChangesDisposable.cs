using log4net;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ScoutUtilities.Common
{

    public class NotifyPropertyChangesDisposable : Disposable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string param)
        {
            DispatcherHelper.ApplicationExecute(() =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(param)));
        }

        protected bool SetFieldAndNotify<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            NotifyPropertyChanged(propertyName);
            return true;
        }
    }
}