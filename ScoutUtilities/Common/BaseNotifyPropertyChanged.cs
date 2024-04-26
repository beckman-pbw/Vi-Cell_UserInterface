using log4net;
using ScoutUtilities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ScoutUtilities.Common
{
    public class BaseNotifyPropertyChanged : INotifyPropertyChanged
    {
        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Dictionary<string, object> _properties;
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly object _propertiesLock = new object();

        public BaseNotifyPropertyChanged()
        {
            lock (_propertiesLock)
            {
                _properties = new Dictionary<string, object>();
            }
        }

        protected void CloneBaseProperties(BaseNotifyPropertyChanged clone)
        {
            if (_properties == null || !_properties.Any()) return;
            clone._properties = new Dictionary<string, object>();

            foreach (var p in _properties)
            {
                if (p.Value == null)
                {
                    clone._properties.Add(p.Key, null);
                    continue;
                }

                var pType = p.Value.GetType();

                if (pType.IsPrimitive || pType.IsValueType)
                {
                    clone._properties.Add(p.Key, p.Value);
                    continue;
                }

                if (pType.GetInterfaces().Contains(typeof(ICloneable)))
                {
                    var val = ((ICloneable)p.Value).Clone();
                    clone._properties.Add(p.Key, val);
                    continue;
                }
            }
        }

        #region Notify Changed

        protected void NotifyPropertyChanged(string param)
        {
            // Since this potentially can switch thread, its more efficient to test
            // if there are listeners, before causing the switch
            DispatcherHelper.ApplicationExecute(() =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(param)));
        }

        public void RaiseCanExecuteChanged(RelayCommand command)
        {
            command.RaiseCanExecuteChanged();
        }

        public void NotifyAllPropertiesChanged(bool includeRelayCommands = true)
        {
            DispatcherHelper.ApplicationExecute(() =>
            {
                foreach (var propInfo in GetType().GetProperties())
                {
                    if (string.IsNullOrEmpty(propInfo?.Name)) continue;

                    if (includeRelayCommands && propInfo.PropertyType == typeof(RelayCommand))
                    {
                        var cmd = (RelayCommand)propInfo.GetValue(this, null);
                        cmd.RaiseCanExecuteChanged();
                    }
                    else
                    {
                        NotifyPropertyChanged(propInfo.Name);
                    }
                }
            });
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
            lock (_propertiesLock)
            {
                if (!_properties.TryGetValue(propertyName, out value))
                {
                    value = default(T);
                    _properties.Add(propertyName, value);
                }
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
            lock (_propertiesLock)
            {
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