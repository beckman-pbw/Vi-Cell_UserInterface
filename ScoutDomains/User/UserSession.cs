using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ScoutUtilities.Common;

namespace ScoutDomains.User
{
    public class UserSession
    {
        private readonly Dictionary<string, SavedSessionValue> _sessionVariables;

        public UserSession()
        {
            _sessionVariables = new Dictionary<string, SavedSessionValue>();
        }

        public UserSession(UserSession oldSession)
        {
            _sessionVariables = new Dictionary<string, SavedSessionValue>(oldSession._sessionVariables);
        }

        public void ResetSessionVariables()
        {
            _sessionVariables.Clear();
        }

        public void SetVariable(string sessionKey, object newValue)
        {
            var sessionVar = new SavedSessionValue(sessionKey, newValue);
            if (_sessionVariables.TryGetValue(sessionKey, out _))
            {
                _sessionVariables[sessionKey] = sessionVar;
            }
            else
            {
                _sessionVariables.Add(sessionKey, sessionVar);
            }
        }

        public object GetVariable(string sessionKey)
        {
            if (_sessionVariables.TryGetValue(sessionKey, out var foundSessionVar))
            {
                return foundSessionVar.Value;
            }

            return null;
        }

        public T GetVariable<T>(string sessionKey)
        {
            if (_sessionVariables.TryGetValue(sessionKey, out var foundSessionVar))
            {
                return (T)foundSessionVar.Value;
            }

            return default;
        }

        public T GetVariable<T>(string sessionKey, T useIfNotExist)
        {
            if (_sessionVariables.TryGetValue(sessionKey, out var foundSessionVar))
            {
                return (T)foundSessionVar.Value;
            }

            return useIfNotExist;
        }

        public bool HasSessionAttribute(string propertyName, Type typeWithProperty)
        {
            var propInfo = typeWithProperty.GetProperty(propertyName);
            if (propInfo == null) return false;
            return propInfo.HasSessionAttribute();
        }

        public bool HasSessionAttribute(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) return false;
            var sessionAttributes = propertyInfo.GetCustomAttributes(true)
                .Where(a => a.GetType() == typeof(SessionVariableAttribute));

            return sessionAttributes.Any();
        }
    }
}