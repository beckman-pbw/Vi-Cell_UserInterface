using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ScoutUtilities.Common
{
    public class SessionKey
    {
        public const string FilterDialog_IsAllSelected = "FilterDialog_IsAllSelected";
        public const string FilterDialog_FromDate = "FilterDialog_FromDate";
        public const string FilterDialog_ToDate = "FilterDialog_ToDate";
        public const string FilterDialog_SelectedUser = "FilterDialog_SelectedUser";
        public const string FilterDialog_SearchString = "FilterDialog_SearchString";
        public const string FilterDialog_TagSearchString = "FilterDialog_TagSearchString";
        public const string FilterDialog_SelectedCellTypeOrQualityControlGroup = "FilterDialog_SelectedCellTypeOrQualityControlGroup";
        public const string FilterDialog_FilteringItem = "FilterDialog_FilteringItem";

        public const string OpenSampleDialog_SelectedUser = "OpenSampleDialog_SelectedUser";
        public const string OpenSampleDialog_FromDate = "OpenSampleDialog_FromDate";
        public const string OpenSampleDialog_ToDate = "OpenSampleDialog_ToDate";

        public const string CompletedRuns_PrintTitle = "CompletedRuns_PrintTitle";
        public const string CompletedRuns_Comments = "CompletedRuns_Comments";
        public const string CompletedRuns_SelectedUser = "CompletedRuns_SelectedUser";
        public const string CompletedRuns_FromDate = "CompletedRuns_FromDate";
        public const string CompletedRuns_ToDate = "CompletedRuns_ToDate";

        public const string StorageTab_FromDate = "StorageTab_FromDate";
        public const string StorageTab_ToDate = "StorageTab_ToDate";
        public const string StorageTab_SelectedUser = "StorageTab_SelectedUser";
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SessionVariableAttribute : Attribute
    {
        public string SessionKey { get; set; }

        public SessionVariableAttribute(string sessionKey)
        {
            SessionKey = sessionKey;
        }
    }

    public class SavedSessionValue
    {
        public string Key { get; set; }
        public object Value { get; set; }

        public SavedSessionValue(string key, object value = null)
        {
            Key = key;
            Value = value;
        }
    }

    public static class SavedSession
    {
        private static readonly Dictionary<string, SavedSessionValue> SessionVariables;

        static SavedSession()
        {
            SessionVariables = new Dictionary<string, SavedSessionValue>();
        }

        public static void ResetSessionVariables()
        {
            SessionVariables.Clear();
        }

        public static void SetVariable(string sessionKey, object newValue)
        {
            var sessionVar = new SavedSessionValue(sessionKey, newValue);
            if (SessionVariables.TryGetValue(sessionKey, out _))
            {
                SessionVariables[sessionKey] = sessionVar;
            }
            else
            {
                SessionVariables.Add(sessionKey, sessionVar);
            }
        }

        public static object GetVariable(string sessionKey)
        {
            if (SessionVariables.TryGetValue(sessionKey, out var foundSessionVar))
            {
                return foundSessionVar.Value;
            }

            return null;
        }

        public static T GetVariable<T>(string sessionKey)
        {
            if (SessionVariables.TryGetValue(sessionKey, out var foundSessionVar))
            {
                return (T) foundSessionVar.Value;
            }

            return default;
        }

        public static T GetVariable<T>(string sessionKey, T useIfNotExist)
        {
            if (SessionVariables.TryGetValue(sessionKey, out var foundSessionVar))
            {
                return (T) foundSessionVar.Value;
            }

            return useIfNotExist;
        }

        public static bool HasSessionAttribute(this string propertyName, Type typeWithProperty)
        {
            var propInfo = typeWithProperty.GetProperty(propertyName);
            if (propInfo == null) return false;
            return propInfo.HasSessionAttribute();
        }

        public static bool HasSessionAttribute(this PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) return false;
            var sessionAttributes = propertyInfo.GetCustomAttributes(true)
                .Where(a => a.GetType().Equals(typeof(SessionVariableAttribute)));
            
            return sessionAttributes.Any();
        }
    }
}