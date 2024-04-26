using ScoutUtilities;
using ScoutUtilities.Interfaces;
using System;

namespace ScoutViewModels.ViewModels.Tabs.SettingsPanel
{
    public class BaseSettingsPanel : BaseViewModel, IComparable<BaseSettingsPanel>, IListItem
    {
        #region Properties & Fields

        public string ListItemLabel
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Commands

        private RelayCommand _saveCommand;
        public RelayCommand SaveCommand => _saveCommand ?? (_saveCommand = new RelayCommand(PerformSave, CanPerformSave));

        protected virtual bool CanPerformSave()
        {
            return true;
        }

        protected virtual void PerformSave()
        {

        }

        #endregion

        #region Virtual Methods
        
        public virtual void UpdateListItemLabel()
        {
            // Used to update the ListItemLabel. Example: when the language changes, we need to update the Settings list items with the new language.
        }

        public virtual void SetDefaultSettings()
        {

        }

        #endregion

        #region IComparable

        public int CompareTo(BaseSettingsPanel other)
        {
            if (ReferenceEquals(this, other))
                return 0;
            if (ReferenceEquals(null, other))
                return 1;
            return string.Compare(ListItemLabel, other.ListItemLabel, StringComparison.Ordinal);
        }

        #endregion
    }
}