using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;

namespace ScoutModels.Common
{
    public class BaseDisposableNotifyPropertyChanged : Disposable, INotifyPropertyChanged
    {
        #region Properties & Fields

        private Dictionary<string, object> _properties;

        protected bool Disposed { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Constructor

        public BaseDisposableNotifyPropertyChanged()
        {
            // setup the properties hash table
            _properties = new Dictionary<string, object>();
        }

        ~BaseDisposableNotifyPropertyChanged()
        {
            Dispose(false);
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
                // value doesn't exist; check for it in the saved session
                var propertyInfo = GetType().GetProperty(propertyName);
                var key = GetSessionVariableAttributeKey(propertyInfo);

                if (!string.IsNullOrEmpty(key))
                {
                    var currentUserSession = LoggedInUser.CurrentUser.Session;
                    var sessionVar = currentUserSession.GetVariable(key);
                    if (sessionVar == null)
                    {
                        value = default(T);
                        currentUserSession.SetVariable(key, value);
                    }
                    else
                    {
                        value = (T) sessionVar;
                    }

                    _properties.Add(propertyName, value);
                }
                else
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

            // update the session saved variables (if applicable)
            var type = GetType();
            if (propertyName.HasSessionAttribute(type))
            {
                var key = GetSessionVariableAttributeKey(type.GetProperty(propertyName));
                if (!string.IsNullOrEmpty(key))
                {
                    var currentUserSession = LoggedInUser.CurrentUser.Session;
                    currentUserSession.SetVariable(key, newValue);
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

        protected void CloneBaseProperties(BaseDisposableNotifyPropertyChanged clone)
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

        public string GetSessionVariableAttributeKey(PropertyInfo propertyInfo)
        {
            try
            {
                if (propertyInfo == null) return string.Empty;
                var attributes = propertyInfo.GetCustomAttributes(true)
                                             .Where(a => a.GetType().Equals(typeof(SessionVariableAttribute)));
                foreach (var atr in attributes)
                {
                    if (atr is SessionVariableAttribute sessionAttribute)
                        return sessionAttribute.SessionKey;
                }

                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        #region Disposable

        protected virtual void Dispose(bool disposing)
        {
            Disposed = true;
        }

        #endregion

        #region Notify Changed

        protected void NotifyPropertyChanged(string param)
        {
            DispatcherHelper.ApplicationExecute(() => 
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(param)));
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
    }
}