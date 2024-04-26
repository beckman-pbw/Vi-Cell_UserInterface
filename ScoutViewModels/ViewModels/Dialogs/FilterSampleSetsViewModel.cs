using HawkeyeCoreAPI.Facade;
using ScoutDataAccessLayer.DAL;
using ScoutDomains;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Admin;
using ScoutModels.Settings;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Helper;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using ScoutDomains.User;

namespace ScoutViewModels.ViewModels.Dialogs
{
    public class FilterSampleSetsViewModel : BaseDialogViewModel
    {
        public FilterSampleSetsViewModel(FilterSampleSetsEventArgs args, Window parentWindow) : base(args, parentWindow)
        {
            _filterArgs = args;
            DialogTitle = LanguageResourceHelper.Get("LID_Header_FilterSampleSets");
            SizeToContent = true;
            
            var userNames = UserModel.GetUserList().Select(u => u.UserID).ToList();
            userNames.Insert(0, LanguageResourceHelper.Get("LID_Label_All"));
            if (LoggedInUser.CurrentUser.RoleID == UserPermissionLevel.eService)
                userNames.Add(ApplicationConstants.ServiceUser);
            Users = userNames.Any() ? userNames.ToObservableCollection() : new ObservableCollection<string>();
            
            CellTypesAndQualityControls = LoggedInUser.GetCtQcs().ToObservableCollection();
            FilterByItems = Enum.GetValues(typeof(eFilterItem)).Cast<eFilterItem>().ToObservableCollection();

            // Start filtering from/to date relative to today
            FromDate = DateTime.Now.AddDays(ApplicationConstants.DefaultFilterFromDaysToSubtract);
            ToDate = DateTime.Now;

            // Get fields from the session
            var currentUserSession = LoggedInUser.CurrentUser.Session;
            var user = currentUserSession.GetVariable(SessionKey.FilterDialog_SelectedUser, LoggedInUser.CurrentUserId);
            SelectedUser = string.IsNullOrEmpty(user) ? userNames.FirstOrDefault() : userNames.FirstOrDefault(u => u.Equals(user));
            //special case where SelectedUser = All then the user changes languages
            if (string.IsNullOrEmpty(SelectedUser))
            {
                SelectedUser = userNames.FirstOrDefault();
            }
            IsAllSelected = currentUserSession.GetVariable(SessionKey.FilterDialog_IsAllSelected, true);
            
            var selectedCellQual = currentUserSession.GetVariable(SessionKey.FilterDialog_SelectedCellTypeOrQualityControlGroup,
                CellTypesAndQualityControls.GetCellTypeQualityControlByIndex(
                    new RunOptionSettingsModel(XMLDataAccess.Instance, LoggedInUser.CurrentUserId)
                        .RunOptionsSettings.DefaultBPQC));
            SelectedCellTypeOrQualityControlGroup = !IsAllSelected && selectedCellQual != null
                ? CellTypesAndQualityControls.GetCellTypeQualityControlByName(selectedCellQual.Name)
                : null;

            FilteringItem = currentUserSession.GetVariable(SessionKey.FilterDialog_FilteringItem, FilterByItems.FirstOrDefault());
            _savedSession = new UserSession(LoggedInUser.CurrentUser.Session);
        }

        public override void OnClosed(bool? result)
        {
            if (result == true) // user wants to filter with the new settings -- update args for the callback
            {
                _filterArgs.User = SelectedUser.Equals(LanguageResourceHelper.Get("LID_Label_All"))
                    ? string.Empty
                    : SelectedUser;
                var currentUserSession = LoggedInUser.CurrentUser.Session;
                currentUserSession.SetVariable(SessionKey.FilterDialog_SelectedUser, _filterArgs.User);
                currentUserSession.SetVariable(SessionKey.FilterDialog_FromDate, FromDate);
                currentUserSession.SetVariable(SessionKey.FilterDialog_ToDate, ToDate);
                _filterArgs.FromDate = FromDate;
                _filterArgs.ToDate = ToDate;
                _filterArgs.SearchString = SearchString;
                _filterArgs.TagSearchString = TagSearchString;
                _filterArgs.IsAllCellTypesSelected = IsAllSelected;
                if (FilteringItem == eFilterItem.eSample && SelectedCellTypeOrQualityControlGroup != null && !IsAllSelected)
                {
                    _filterArgs.CellTypeOrQualityControlName = Misc.GetBaseQualityControlName(
                        SelectedCellTypeOrQualityControlGroup.Name);
                }
                else
                {
                    _filterArgs.CellTypeOrQualityControlName = string.Empty;
                }
                _filterArgs.FilteringItem = FilteringItem;
            }
            else
            {
                LoggedInUser.CurrentUser.Session = new UserSession(_savedSession);
                var currentUserSession = LoggedInUser.CurrentUser.Session;
                var user = currentUserSession.GetVariable(SessionKey.FilterDialog_SelectedUser);
                user = user.Equals(LanguageResourceHelper.Get("LID_Label_All"))
                    ? string.Empty
                    : user;
                currentUserSession.SetVariable(SessionKey.FilterDialog_SelectedUser, user);
            }

            base.OnClosed(result);
        }

        private bool IsValidFormData()
        {
            return FromDate <= ToDate && FromDate <= DateTime.Now;
        }

        public override bool CanAccept()
        {
            return base.CanAccept() && IsValidFormData();
        }

        #region Properties & Fields

        private readonly FilterSampleSetsEventArgs _filterArgs;
        private UserSession _savedSession;

        [SessionVariable(SessionKey.FilterDialog_IsAllSelected)]
        public bool IsAllSelected
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                if (value)
                    SelectedCellTypeOrQualityControlGroup = null;
            }
        }

        public ObservableCollection<string> Users
        {
            get { return GetProperty<ObservableCollection<string>>(); }
            set { SetProperty(value); }
        }

        [SessionVariable(SessionKey.FilterDialog_SelectedUser)]
        public string SelectedUser
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public DateTime FromDate
        {
            get { return GetProperty<DateTime>(); }
            set
            {
                var newDt = DateTimeConversionHelper.DateTimeToStartOfDay(value);
                SetProperty(newDt);
                AcceptCommand.RaiseCanExecuteChanged();
            }
        }

        public DateTime ToDate
        {
            get { return GetProperty<DateTime>(); }
            set
            {
                var newDt = DateTimeConversionHelper.DateTimeToEndOfDay(value);
                SetProperty(newDt);
                AcceptCommand.RaiseCanExecuteChanged();
            }
        }

        [SessionVariable(SessionKey.FilterDialog_SearchString)]
        public string SearchString
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        [SessionVariable(SessionKey.FilterDialog_TagSearchString)]
        public string TagSearchString
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<CellTypeQualityControlGroupDomain> CellTypesAndQualityControls
        {
            get { return GetProperty<ObservableCollection<CellTypeQualityControlGroupDomain>>(); }
            set { SetProperty(value); }
        }

        [SessionVariable(SessionKey.FilterDialog_SelectedCellTypeOrQualityControlGroup)]
        public CellTypeQualityControlGroupDomain SelectedCellTypeOrQualityControlGroup
        {
            get { return GetProperty<CellTypeQualityControlGroupDomain>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<eFilterItem> FilterByItems
        {
            get { return GetProperty<ObservableCollection<eFilterItem>>(); }
            set { SetProperty(value); }
        }

        [SessionVariable(SessionKey.FilterDialog_FilteringItem)]
        public eFilterItem FilteringItem
        {
            get { return GetProperty<eFilterItem>(); }
            set { SetProperty(value); }
        }

        #endregion
    }
}