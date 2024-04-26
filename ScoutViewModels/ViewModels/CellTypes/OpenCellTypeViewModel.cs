using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutDomains.Analysis;
using ScoutDomains.RunResult;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Common;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using ScoutViewModels.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using ScoutDomains;
using ScoutModels.Interfaces;
using ScoutModels.Review;
using ScoutServices.Interfaces;

namespace ScoutViewModels.ViewModels.CellTypes
{
    public class OpenCellTypeViewModel : BaseCellTypeViewModel
    {
        #region Constructor

        public OpenCellTypeViewModel(UserDomain currentUser, IInstrumentStatusService instrumentStatusService, ICellTypeManager cellTypeManager)
            : base(currentUser, instrumentStatusService, cellTypeManager)
        {
            _recordHelper = new ResultRecordHelper();
            ReviewViewModel = new ReviewViewModel();
            ReviewViewModel.ReviewModel.SampleAnalysisOccurred += HandleSampleAnalysisOccurred;
        }

        protected override void DisposeUnmanaged()
        {
            if (ReviewViewModel != null && ReviewViewModel.ReviewModel != null)
                ReviewViewModel.ReviewModel.SampleAnalysisOccurred -= HandleSampleAnalysisOccurred;
            ReviewViewModel?.Dispose();
            _recordHelper?.Dispose();
            base.DisposeUnmanaged();
        }

        #endregion

        #region Properties & Fields

        private bool _enableSampleAnalysisListener;
        private ResultRecordHelper _recordHelper;
        private CellTypeDomain _tempCellType;

        public CellTypeDomain SelectedCellFromList { get; set; }
        public CellTypeDomain SelectedCellType { get; set; }
        public Func<bool, Tuple<CellTypeDomain, CellTypeDomain>> GetSelectedCell { get; set; }

        public ReviewViewModel ReviewViewModel
        {
            get { return GetProperty<ReviewViewModel>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Reanalyze Command

        private RelayCommand _reanalyzeCommand;

        public RelayCommand ReanalyzeCommand => _reanalyzeCommand ?? (_reanalyzeCommand = new RelayCommand(OnReanalyzeSampleUpdate, CanPerformReanalysis));

        private bool CanPerformReanalysis()
        {
            return ReviewViewModel.SelectedSampleRecord != null &&
                   CurrentUser.RoleID != UserPermissionLevel.eService &&
                   _cellTypeManager.InstrumentStateAllowsEdit;
        }

        private void OnReanalyzeSampleUpdate(object param)
        {
            var value = GetSelectedCell(true);
            SelectedCellFromList = value.Item1;
            SelectedCellType = value.Item2;
            OnReanalyzeSample();
        }

        #endregion

        #region Methods

        protected override void OnSystemStatusChanged()
        {
            DispatcherHelper.ApplicationExecute(() =>
            {
                ReanalyzeCommand.RaiseCanExecuteChanged();
            });
        }

        public void OnReanalyzeSample()
        {
            try
            {
                SetValueForCell();
                _enableSampleAnalysisListener = true;
                
                if (!_cellTypeManager.SaveCellTypeValidation(SelectedCellFromList, true))
                    return;

                OnSelectedCell(SelectedCellFromList);
                ReviewViewModel.SetLoadingIndicator(true);
                var fromImage = ReviewModel.IsImageStatusDifferent(ReviewViewModel.UnsavedReanalyzedSampleRecord, _tempCellType);
                var reanalyzeStatus = ReviewViewModel.ReviewModel.ReanalyzeSample(ReviewViewModel.SelectedSampleFromList.UUID, 
                    _tempCellType.CellTypeIndex, _tempCellType.AnalysisDomain.AnalysisIndex, fromImage);

                if (reanalyzeStatus.Equals(HawkeyeError.eSuccess))
                {
                    PostToMessageHub(LanguageResourceHelper.Get("LID_StatusBar_SampleReanalyzed"));
                }
                else
                {
                    ReviewViewModel.SetLoadingIndicator(false);
                    _enableSampleAnalysisListener = false;
                    ApiHawkeyeMsgHelper.ErrorCommon(reanalyzeStatus);
                }
            }
            catch (Exception ex)
            {
                ReviewViewModel.SetLoadingIndicator(false);
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERRORONREANALYSESAMPLE"));
            }
        }

        private void SetValueForCell()
        {
            if (SelectedCellType.TempCellName.Contains(ApplicationConstants.SelectedSampleCellIndication))
            {
                var cellName = SelectedCellType.TempCellName.Remove(SelectedCellType.TempCellName.Length - 1);
                SelectedCellFromList.TempCellName = cellName;
                SelectedCellFromList.CellTypeName = cellName;
            }
            else
            {
                SelectedCellFromList.TempCellName = SelectedCellType.TempCellName;
                SelectedCellFromList.CellTypeName = SelectedCellType.CellTypeName;
            }

            SelectedCellFromList.IsUserDefineCellType = SelectedCellType.IsUserDefineCellType;
        }

        private void OnSelectedCell(CellTypeDomain selectedCellType)
        {
            CellTypeModel.svc_SetTemporaryCellType(selectedCellType);
            CellTypeModel.svc_SetTemporaryAnalysisDefinition(selectedCellType.AnalysisDomain);
            GetTempCellType();
        }

        private void GetTempCellType()
        {
            var tempCellTypes = CellTypeModel.svc_GetTemporaryCellType();
            if (tempCellTypes != null && tempCellTypes.Any())
            {
                _tempCellType = tempCellTypes.FirstOrDefault();
            }
            else
            {
                Log.Debug($"No temporary cell types returned from the backend.");
            }
        }

        private void HandleSampleAnalysisOccurred(object sender, ApiEventArgs<HawkeyeError, uuidDLL, ResultRecordDomain> e)
        {
            if (!_enableSampleAnalysisListener)
            {
                return;
            }

            _enableSampleAnalysisListener = false;

            Log.Debug($"ReanalyzeSample Callback::hawkeyeStatus: '{e.Arg1}'");

            try
            {
                if (e.Arg1.Equals(HawkeyeError.eSuccess))
                {
                    // Update the sample with Arg3 (new result record)
                    ReviewViewModel.UpdateSample(e.Arg3);

                    ReviewViewModel.SelectedSampleFromList.SelectedResultSummary.CellTypeDomain.CellTypeName =
                        ApplicationConstants.TempCellTypeCharacter + ReviewViewModel.SelectedSampleFromList.SelectedResultSummary.CellTypeDomain.CellTypeName;
                    
                    ReviewViewModel.ShowParameterList = new List<KeyValuePair<string, string>>(
                        _recordHelper.SetListParameter(ReviewViewModel.SelectedSampleFromList, LoggedInUser.CurrentUserId));
                }
                else
                {
                    ApiHawkeyeMsgHelper.ErrorCommon(e.Arg1);
                }

                ReviewViewModel.SetLoadingIndicator(false);
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERRORONSAMPLEANALYSIS"));
            }
        }

        #endregion 
    }
}