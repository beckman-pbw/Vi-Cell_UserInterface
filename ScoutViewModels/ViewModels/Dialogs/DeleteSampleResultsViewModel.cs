using ScoutLanguageResources;
using ScoutModels.Common;
using ScoutServices.Interfaces;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScoutViewModels.ViewModels.Dialogs
{
    public class DeleteSampleResultsViewModel : BaseDialogViewModel
    {
        #region Constructor

        public DeleteSampleResultsViewModel(ISampleResultsManager sampleResultsManager, 
            DeleteSampleResultsEventArgs args, System.Windows.Window parentWindow) 
            : base(args, parentWindow)
        {
            DialogTitle = LanguageResourceHelper.Get("LID_Label_DeleteSampleResults");
            ShowDialogTitleBar = true;

            _itemsToDelete = args.ItemsToDelete;
            SelectedSamplesCount = _itemsToDelete.Count;
            _sampleResultsManager = sampleResultsManager;
            _deleteSamplesSubscriber = _sampleResultsManager.SubscribeDeleteSampleResultsProgress().Subscribe(OnDeleteSamplesModelDeletionProgress);
        }

        protected override void DisposeUnmanaged()
        {
            _deleteSamplesSubscriber?.Dispose();
            base.DisposeUnmanaged();
        }

        #endregion

        #region Properties & Fields

        private ISampleResultsManager _sampleResultsManager;
        private List<uuidDLL> _itemsToDelete;
        private IDisposable _deleteSamplesSubscriber;

        public bool RetainResultsAndFirstImage
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsDeleteSampleProcessActive
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);                
                AcceptCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
            }
        }

        public int SelectedSamplesCount
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public string DeletingMessage
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string DeletionPercentage
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Event Handlers

        private void OnDeleteProgress(HawkeyeError status, uuidDLL sampleId, int percent)
        {
            DispatcherHelper.ApplicationExecute(() => 
            { 
                OnDeleteSamplesModelDeletionProgress(new SampleProgressEventArgs(percent, (percent >= 100), status, "")); 
            });
        }

        private void OnDeleteSamplesModelDeletionProgress(SampleProgressEventArgs e)
        {
			if (e == null || e.Error == null)
			{
				return;
			}

			if (e.Error != HawkeyeError.eSuccess)
            {
                ApiHawkeyeMsgHelper.ErrorCommon(e.Error.Value);
                Close(true);
            }
            else
            {
                if (e.OperationIsComplete)
                {
                    DispatcherHelper.ApplicationExecute(() =>
                    {
                        IsDeleteSampleProcessActive = false;
                        DeletionPercentage = $"100%";
                        DeletingMessage = string.Empty;
                    });
                    MessageBus.Default.Publish(RetainResultsAndFirstImage
                        ? LanguageResourceHelper.Get("LID_MSGBOX_DeleteImages")
                        : LanguageResourceHelper.Get("LID_MSGBOX_DeleteSample"));

                    Close(true);
                }
                else
                {
                    DispatcherHelper.ApplicationExecute(() =>
                    {
                        IsDeleteSampleProcessActive = true;
                        DeletionPercentage = $"{e.PercentComplete}%";
                        DeletingMessage = LanguageResourceHelper.Get("LID_Label_DeletionInprogress");
                    });
                }
            }
        }

        #endregion

        #region Commands

        public override bool CanCancel()
        {
            return true;
        }

        public override bool CanDecline()
        {
            return true;
        }

        public override bool CanAccept()
        {
            return SelectedSamplesCount > 0;
        }

        protected override void OnAccept()
        {
            IsDeleteSampleProcessActive = true;
            var username = ScoutModels.LoggedInUser.CurrentUserId;
            _sampleResultsManager.DeleteSampleResults(username, "", _itemsToDelete, RetainResultsAndFirstImage);
        }

        public override bool Close(bool? result)
        {
            if (!IsDeleteSampleProcessActive)
            {
                return base.Close(result);
            }

            DeletingMessage = LanguageResourceHelper.Get("LID_Status_Cancelled");
            DeletionPercentage = string.Empty;

            ExportManager.AoExportMgr.PublishCancelDelete();
            result = true;
            return base.Close(result);
        }
        #endregion
    }
}