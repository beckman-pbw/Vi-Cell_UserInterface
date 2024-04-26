using log4net;
using ScoutUtilities;
using ScoutUtilities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using ScoutUtilities.Common;

namespace ScoutUI.Common
{
    public class BaseUserControl : UserControl, INotifyPropertyChanged
    {
        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Dictionary<string, object> _properties;

        public BaseUserControl()
        {
            _properties = new Dictionary<string, object>();
        }

        #region Notify Changed

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string param)
        {
            DispatcherHelper.ApplicationExecute(() =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(param)));
        }

        public void RaiseCanExecuteChanged(RelayCommand command)
        {
            command.RaiseCanExecuteChanged();
        }

        #endregion

        #region Getter/Setter

        public T GetProperty<T>([CallerMemberName] string propertyName = "")
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            object value;
            if (!_properties.TryGetValue(propertyName, out value))
            {
                value = default(T);
                _properties.Add(propertyName, value);
            }

            return (T)value;
        }

        public void SetProperty<T>(T newValue, NotifyType notifyType = NotifyType.Auto, [CallerMemberName] string propertyName = "")
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            bool valueChanged;
            object oldValue;
            if (!_properties.TryGetValue(propertyName, out oldValue))
            {
                oldValue = default(T);
                valueChanged = true; // we're changing from NULL to something

                // property doesn't exist yet, so add it with the new value
                _properties.Add(propertyName, newValue);
            }
            else
            {
                // see if the value changed
                valueChanged = !EqualityComparer<T>.Default.Equals((T)oldValue, newValue);

                // update the property's field
                _properties[propertyName] = newValue;
            }

            switch (notifyType)
            {
                case NotifyType.Auto:
                    if (valueChanged) NotifyPropertyChanged(propertyName);
                    break;
                case NotifyType.Force:
                    NotifyPropertyChanged(propertyName);
                    break;
                case NotifyType.None:
                    break;
            }
        }

        #endregion
    }
}