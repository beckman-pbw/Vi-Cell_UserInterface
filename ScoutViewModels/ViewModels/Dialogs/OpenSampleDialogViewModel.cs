using ApiProxies.Generic;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Common;
using ScoutModels.Review;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.Helper;
using ScoutUtilities.UIConfiguration;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using ScoutModels.Admin;
using ScoutServices.Interfaces;
using System.Collections.Generic;
using ScoutUtilities.Structs;

namespace ScoutViewModels.ViewModels.Dialogs
{
    public class OpenSampleDialogViewModel : BaseDialogViewModel
    {
        public OpenSampleDialogViewModel(ISampleResultsManager sampleResultsManager, OpenSampleEventArgs args, System.Windows.Window parentWindow) : base(args, parentWindow)
        {
            DialogTitle = LanguageResourceHelper.Get("LID_POPUPHeader_OpenSample");
            DialogLocation = DialogLocation.CenterApp;
            SampleRecordList = new ObservableCollection<SampleRecordDomain>();
            CreateUserList();
            SetDefaultDates();
            GetSampleList();
            _deleteSampleSubscriber = sampleResultsManager.SubscribeSamplesDeleted().Subscribe(DeleteSamplesFromCallback);
        }

        protected override void DisposeUnmanaged()
        {
            _deleteSampleSubscriber?.Dispose();
            base.DisposeUnmanaged();
        }

        #region Properties & Fields

        private readonly IDisposable _deleteSampleSubscriber;
        private List<uuidDLL> _samplesToDelete;

        public ObservableCollection<SampleRecordDomain> SampleRecordList
        {
            get { return GetProperty<ObservableCollection<SampleRecordDomain>>(); }
            set
            {
                SetProperty(value);
                ExportSummarySampleCommand.RaiseCanExecuteChanged();
            }
        }

        public SampleRecordDomain SelectedSampleRecord
        {
            get { return GetProperty<SampleRecordDomain>(); }
            set
            {
                SetProperty(value);
                AcceptCommand.RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<UserDomain> UserList
        {
            get { return GetProperty<ObservableCollection<UserDomain>>(); }
            set { SetProperty(value); }
        }

        public string UserId => SelectedUser == null ? string.Empty : SelectedUser.UserID.Equals(LanguageResourceHelper.Get("LID_Label_All")) ? string.Empty : SelectedUser.UserID;

        [SessionVariable(SessionKey.OpenSampleDialog_SelectedUser)]
        public UserDomain SelectedUser
        {
            get { return GetProperty<UserDomain>(); }
            set
            {
                SetProperty(value);
                NotifyPropertyChanged(nameof(UserId));
            }
        }

        [SessionVariable(SessionKey.OpenSampleDialog_FromDate)]
        public DateTime FromDate
        {
            get { return GetProperty<DateTime>(); }
            set { SetProperty(value); }
        }

        [SessionVariable(SessionKey.OpenSampleDialog_ToDate)]
        public DateTime ToDate
        {
            get { return GetProperty<DateTime>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Commands

        #region Accept, Decline, Cancel Commands

        public override bool CanAccept()
        {
            return SelectedSampleRecord != null;
        }

        #endregion

        #region Export Command

        private RelayCommand _exportSummarySampleCommand;
        public RelayCommand ExportSummarySampleCommand => _exportSummarySampleCommand ?? (_exportSummarySampleCommand = new RelayCommand(PerformExportSummary, CanPerformExportSummary));

        private bool CanPerformExportSummary()
        {
            return SampleRecordList != null && SampleRecordList.Any();
        }

        private void PerformExportSummary()
        {
            try
            {
                if (SampleRecordList == null || !SampleRecordList.Any()) return;

                var defaultFileName = ApplicationConstants.SummaryExportFileNameAppendant + Misc.ConvertToFileNameFormat(DateTime.Now);
                var sampleList = SampleRecordList.ToList();
                var records = ResultRecordHelper.ExportCompleteRunResult(sampleList);
                string filename = ExportModel.PromptAndExportSamplesToCsv(ref sampleList, defaultFileName);
                if (!String.IsNullOrEmpty(filename))
                {
                    var outdir = System.IO.Path.GetDirectoryName(filename);
                    var csvFilename = System.IO.Path.GetFileNameWithoutExtension(filename);

                    ExportManager.EvExportCsvReq.PublishSummaryOnly(LoggedInUser.CurrentUserId, "", sampleList, outdir, csvFilename, false);
                    return;
                }
                return;

            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_EXPORT_DATA"));
            }
            ExportModel.ExportFailedMessage();
        }

        #endregion

        #region Filter Sample List Command

        private RelayCommand _filterSampleListCommand;
        public RelayCommand FilterSampleListCommand => _filterSampleListCommand ?? (_filterSampleListCommand = new RelayCommand(FilterSampleList));

        private void FilterSampleList()
        {
            GetSampleList();
        }

        #endregion

        #endregion

        #region Methods

        private void CreateUserList()
        {

            UserList = new ObservableCollection<UserDomain>(UserModel.GetUserList().ToObservableCollection());
            UserList.Add(new UserDomain { UserID = LanguageResourceHelper.Get("LID_Label_All") });

            UserDomain user;
            switch (LoggedInUser.CurrentUserId)
            {
                case ApplicationConstants.ServiceUser:
                    UserList.Add(new UserDomain { UserID = ApplicationConstants.ServiceUser });
                    user = UserList.FirstOrDefault(a => a.UserID.Equals(ApplicationConstants.ServiceUser));
                    break;
                case ApplicationConstants.SilentAdmin:
                    UserList.Add(new UserDomain { UserID = ApplicationConstants.SilentAdmin });
                    user = UserList.FirstOrDefault(a => a.UserID.Equals(ApplicationConstants.SilentAdmin));
                    break;
                case ApplicationConstants.AutomationClient:
                    UserList.Add(new UserDomain { UserID = ApplicationConstants.AutomationClient });
                    user = UserList.FirstOrDefault(a => a.UserID.Equals(ApplicationConstants.AutomationClient));
                    break;
                //// Leave for future reference or use
                //case ApplicationConstants.ServiceAdmin:
                //    UserList.Add(new UserDomain { UserID = ApplicationConstants.ServiceAdmin });
                //    user = UserList.FirstOrDefault(a => a.UserID.Equals(ApplicationConstants.ServiceAdmin));
                //    break;
                default:
                    user = UserList.FirstOrDefault(a => a.UserID.Equals(LoggedInUser.CurrentUserId));
                    break;
            }

            if (user == null) user = UserList.FirstOrDefault();
            var currentUserSession = LoggedInUser.CurrentUser.Session;
            user = currentUserSession.GetVariable(SessionKey.OpenSampleDialog_SelectedUser, user);
            SelectedUser = UserList.FirstOrDefault(u => u.UserID.Equals(user.UserID));
            //special case where SelectedUser = All then the user changes languages
            if (SelectedUser == null)
            {
                SelectedUser = UserList.FirstOrDefault(u => u.UserID.Equals(LoggedInUser.CurrentUserId));
            }
        }

        private void SetDefaultDates()
        {
            if (LoggedInUser.IsConsoleUserLoggedIn())
            {
                var currentUserSession = LoggedInUser.CurrentUser.Session;
                FromDate = (DateTime)currentUserSession.GetVariable(SessionKey.OpenSampleDialog_FromDate);
                ToDate = (DateTime)currentUserSession.GetVariable(SessionKey.OpenSampleDialog_ToDate);
            }
            else
            {
                FromDate = DateTimeConversionHelper.DateTimeToStartOfDay(
                        DateTime.Today.AddDays(ApplicationConstants.DefaultFilterFromDaysToSubtract));
                ToDate = DateTimeConversionHelper.DateTimeToEndOfDay(DateTime.Today);
            }
        }

        private void GetSampleList()
        {
            if (LoggedInUser.IsConsoleUserLoggedIn())
            {
                LoggedInUser.CurrentUser.Session.SetVariable(SessionKey.OpenSampleDialog_FromDate, DateTimeConversionHelper.DateTimeToStartOfDay(FromDate));
                LoggedInUser.CurrentUser.Session.SetVariable(SessionKey.OpenSampleDialog_ToDate, DateTimeConversionHelper.DateTimeToEndOfDay(ToDate));
            }

            var startDate = DateTimeConversionHelper.DateTimeToUnixSecondRounded(FromDate);
            var endDate = DateTimeConversionHelper.DateTimeToEndOfDayUnixSecondRounded(ToDate);
            var sampleRecordList = ReviewModel.GetFlattenedResultRecordList_wrappedInSampleRecords(startDate, endDate, UserId);

            if (sampleRecordList == null || !sampleRecordList.Any())
            {
                SampleRecordList = new ObservableCollection<SampleRecordDomain>();
                return;
            }

            var maximumSearchCount = UISettings.MaximumSearchCountForOpenSample;
            if (sampleRecordList.Count <= maximumSearchCount)
            {
                SampleRecordList = sampleRecordList.ToObservableCollection();
            }
            else if (sampleRecordList.Count > maximumSearchCount)
            {
                var message = string.Format(LanguageResourceHelper.Get("LID_MSGBOX_RefineSearch"), Misc.ConvertToString(maximumSearchCount));
                DialogEventBus.DialogBoxOk(this, message);
            }

            SelectedSampleRecord = null;
        }

        private void DeleteSamplesFromCallback(SamplesDeletedEventArgs args)
        {
            try
            {
                if (args.PercentComplete <= 0) //start list of samples to delete from home view
                {
                    _samplesToDelete = new List<uuidDLL>();
                    return;
                }
                else
                {
                    _samplesToDelete.Add(args.SampleUuid);
                    if (args.PercentComplete < 100)
                        return;
                    foreach (var uuid in _samplesToDelete) //delete operation complete, remove samples from home view
                    {
                        for (int i = SampleRecordList.Count - 1; i >= 0; i--)
                        {
                            var record = SampleRecordList[i];
                            if (record.UUID.Equals(uuid))
                            {
                                DispatcherHelper.ApplicationExecute(() =>
                                {
                                    SampleRecordList.Remove(record);
                                });
                            }
                        }
                    }
                    _samplesToDelete.Clear();
                }
            }
            catch (Exception e)
            {
                Log.Debug("OpenSampleDialogViewModel :: DeleteSamplesFromCallback :: " + e.Message);
            }
        }

        #endregion
    }
}