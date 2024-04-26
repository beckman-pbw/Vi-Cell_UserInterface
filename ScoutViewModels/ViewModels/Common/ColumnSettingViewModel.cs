using ScoutModels;
using ScoutModels.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;

namespace ScoutViewModels.ViewModels.Common
{
    public class ColumnSettingViewModel : BaseViewModel, IEquatable<ColumnSettingViewModel>
    {
        public ColumnSettingViewModel(string label, bool isVisible, DataGridLength colWidth)
        {
            HeaderLabel = label;
            IsVisible = isVisible;
            ColumnWidth = colWidth;
        }

        #region Properties & Fields

        public EventHandler<PropertyChangedEventArgs> SettingChanged;

        public string HeaderLabel
        {
            get { return GetProperty<string>(); }
            set
            {
                SetProperty(value);
                SettingChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HeaderLabel)));
            }
        }

        public bool IsVisible
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                SettingChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVisible)));
            }
        }

        public DataGridLength ColumnWidth
        {
            get { return GetProperty<DataGridLength>(); }
            set
            {
                SetProperty(value);
                SettingChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ColumnWidth)));
            }
        }

        #endregion

        #region Static Methods

        public static Dictionary<string, ColumnSettingViewModel> GetAll()
        {
            var backend = ColumnSettingsModel.GetAll(LoggedInUser.CurrentUserId);
            var list = new Dictionary<string, ColumnSettingViewModel>();

            foreach (var item in backend)
            {
                list.Add(item.Key, new ColumnSettingViewModel(item.Value.Item1, item.Value.Item2, item.Value.Item3));
            }

            return list;
        }

        public static Dictionary<string, ColumnSettingViewModel> Clone(Dictionary<string, ColumnSettingViewModel> dictionary)
        {
            var newList = new Dictionary<string, ColumnSettingViewModel>();

            foreach (var item in dictionary)
            {
                var clonedColSetting = new ColumnSettingViewModel(item.Value.HeaderLabel, item.Value.IsVisible, new DataGridLength(item.Value.ColumnWidth.Value));
                newList.Add(item.Key, clonedColSetting);
            }

            return newList;
        }

        public static bool AreSame(Dictionary<string, ColumnSettingViewModel> a, Dictionary<string, ColumnSettingViewModel> b)
        {
            if (a == null && b == null) return true;
            if (a == null && b != null) return false;
            if (a != null && b == null) return false;

            if (a.Count != b.Count) return false;

            foreach (var item in a)
            {
                if (!b.ContainsKey(item.Key))
                    return false;
                if (!b[item.Key].Equals(item.Value))
                    return false;
            }

            return true;
        }

        #endregion

        #region IEquatable Methods

        public bool Equals(ColumnSettingViewModel other)
        {
            if (other == null) return false;
            return IsVisible.Equals(other.IsVisible) &&
                   (HeaderLabel ?? string.Empty).Equals(other.HeaderLabel ?? string.Empty) &&
                   ColumnWidth.Value.Equals(other.ColumnWidth.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((ColumnSettingViewModel) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (HeaderLabel ?? string.Empty).GetHashCode();
                hashCode = (hashCode * 397) ^ IsVisible.GetHashCode();
                hashCode = (hashCode * 397) ^ ColumnWidth.Value.GetHashCode();
                return hashCode;
            }
        }

        #endregion
    }
}