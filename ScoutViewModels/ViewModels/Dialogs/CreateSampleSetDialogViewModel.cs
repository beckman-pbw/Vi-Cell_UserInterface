using ApiProxies.Generic;
using HawkeyeCoreAPI.Facade;
using ScoutDataAccessLayer.DAL;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Common;
using ScoutModels.ExpandedSampleWorkflow;
using ScoutModels.Interfaces;
using ScoutModels.Settings;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.Helper;
using ScoutUtilities.Interfaces;
using ScoutUtilities.Structs;
using ScoutViewModels.ViewModels.ExpandedSampleWorkflow;
using ScoutViewModels.ViewModels.Home.ExpandedSampleWorkflow;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using ScoutServices.Interfaces;

namespace ScoutViewModels.ViewModels.Dialogs
{
    public class CreateSampleSetDialogViewModel : BaseDialogViewModel
    {
        #region Constructor

        public CreateSampleSetDialogViewModel(IInstrumentStatusService instrumentStatusService, CreateSampleSetEventArgs<SampleSetViewModel> args, Window parentWindow,
            ISolidColorBrushService colorBrushService, RunOptionSettingsModel runOptionSettings,
            IAutomationSettingsService automationSettingsService)
            : base(args, parentWindow)
        {
            ShowDialogTitleBar = true;
            DialogTitle = LanguageResourceHelper.Get("LID_Label_CreateSampleSet");
            CreatedByUser = args.CreatedByUser;
            _runOptionSettings = runOptionSettings;
            _automationConfig = automationSettingsService.GetAutomationConfig();
            _instrumentStatusService = instrumentStatusService;
            SampleSet = args.NewSampleSet;
            SampleSet.CreatedByUser = args.CreatedByUser;
            SampleSet.RunByUser = args.RunByUser;
            SampleSet.PropertyChanged += OnSampleSetPropertyChanged;

            SampleTemplate = new CreateSampleTemplateViewModel(args.NewSampleSet?.SampleTemplate, this);
            SampleSet.SampleSetName = args.SampleSetName;
            if (SampleTemplate != null)
            {
                SampleTemplate.AdvancedSampleSettings.ShowApplySettingsToAll =
                    true; // we should allow this when creating sample sets
                SampleTemplate.OnAdvancedSettingsApplyToAllClicked += OnSampleTemplateAdvancedSettingsApplyToAllClicked;

                SampleTemplate.PropertyChanged += OnSampleTemplatePropertyChanged;

                var samplePosition = _instrumentStatusService.SystemStatusDom.SamplePosition;
                CarouselModel.Instance.SetTopCarouselPosition(samplePosition.Column);
                var hideWells = args.WorkListStatus == WorkListStatus.Paused || args.WorkListStatus == WorkListStatus.Running;
                CarouselVm = new CarouselViewModel(SampleTemplate, SampleSet, LoggedInUser.GetCtQcs(), hideWells,
                    colorBrushService, runOptionSettings);
                CarouselVm.WellWasClicked += CarouselVmWellWasClicked;

                PlateVm = new PlateViewModel(SampleTemplate, SampleSet, LoggedInUser.GetCtQcs(), true,
                    colorBrushService, runOptionSettings);
                PlateVm.WellWasClicked += PlateVmWellWasClicked;

                AutoCupVm = new AutomationCupViewModel(SampleTemplate, SampleSet, LoggedInUser.GetCtQcs(),
                    colorBrushService, runOptionSettings);
                AutoCupVm.WellWasClicked += AutomationCupVmWellWasClicked;

                CarouselVm.SampleTemplate.SequentialNamingItems.ResetSequenceNumbering();
                PlateVm.SampleTemplate.SequentialNamingItems.ResetSequenceNumbering();
                AutoCupVm.SampleTemplate.SequentialNamingItems.ResetSequenceNumbering();

				// Only the A-Cup is supported in CellHealth.
				PlateTypes = new ObservableCollection<SubstrateType>()
				{
					SubstrateType.AutomationCup
				};

                // Handle case where the last used substrate type (most likely ACup) is no longer available.
                SelectedPlateType = PlateTypes.FirstOrDefault();

                AllowSubstrateChanges = args.AllowSubstrateToChange;
            }
        }

        protected override void DisposeManaged()
        {
            SampleSet.PropertyChanged -= OnSampleSetPropertyChanged;
            SampleTemplate.OnAdvancedSettingsApplyToAllClicked -= OnSampleTemplateAdvancedSettingsApplyToAllClicked;
            SampleTemplate.PropertyChanged -= OnSampleTemplatePropertyChanged;
            CarouselVm.WellWasClicked -= CarouselVmWellWasClicked;
            PlateVm.WellWasClicked -= PlateVmWellWasClicked;
            AutoCupVm.WellWasClicked -= AutomationCupVmWellWasClicked;
            CarouselVm?.Dispose();
            PlateVm?.Dispose();
            AutoCupVm?.Dispose();
            base.DisposeManaged();
        } 

        #endregion

        #region Event Handlers

        private void OnSampleTemplatePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (SelectedPlateType == SubstrateType.Carousel) CarouselVm.SampleTemplate = SampleTemplate;
            if (SelectedPlateType == SubstrateType.Plate96) PlateVm.SampleTemplate = SampleTemplate;
            if (SelectedPlateType == SubstrateType.AutomationCup) AutoCupVm.SampleTemplate = SampleTemplate;
        }

        private void OnSampleTemplateAdvancedSettingsApplyToAllClicked(object sender,
            AdvanceSampleSettingsDialogEventArgs<AdvancedSampleSettingsViewModel> args)
        {
            if (args.AdvancedSampleSettingsViewModel.ApplySettingsToAll)
            {
                foreach (var sample in GetCheckedSamples())
                {
                    sample.AdvancedSampleSettings = args.AdvancedSampleSettingsViewModel;
                }
            }
        }

        private void OnSampleSetPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (SelectedPlateType == SubstrateType.Carousel) CarouselVm.SampleSet = SampleSet;
            if (SelectedPlateType == SubstrateType.Plate96) PlateVm.SampleSet = SampleSet;
            if (SelectedPlateType == SubstrateType.AutomationCup) AutoCupVm.SampleSet = SampleSet;
        }

        private void CarouselVmWellWasClicked(object sender, EventArgs e)
        {
            // if the carousel is suddenly void of any samples, let's reset the sequencing numbers
            if (CarouselVm.SampleTemplate.UseSequencing && CarouselVm.SampleWellButtons.All(w => w.Sample == null))
            {
                CarouselVm.SampleTemplate.SequentialNamingItems.ResetSequenceNumbering();
            }

            UpdateButtons();
        }

        private void PlateVmWellWasClicked(object sender, EventArgs e)
        {
            // if the plate is suddenly void of any samples, let's reset the sequencing numbers
            if (PlateVm.SampleTemplate.UseSequencing && PlateVm.SampleWellButtons.All(w => w.Sample == null))
            {
                PlateVm.SampleTemplate.SequentialNamingItems.ResetSequenceNumbering();
            }

            UpdateButtons();
        }

        private void AutomationCupVmWellWasClicked(object sender, EventArgs e)
        {
            // if the Automation Cup is suddenly void of any samples, let's reset the sequencing numbers
            if (AutoCupVm.SampleTemplate.UseSequencing && AutoCupVm.SampleWellButtons.All(w => w.Sample == null))
            {
                AutoCupVm.SampleTemplate.SequentialNamingItems.ResetSequenceNumbering();
            }

            UpdateButtons();
        }

        #endregion

        #region Private Methods

        private void UpdateButtons()
        {
            AcceptCommand.RaiseCanExecuteChanged();
            FileLoadSampleSetCommand.RaiseCanExecuteChanged();
            FileSaveSampleSetCommand.RaiseCanExecuteChanged();
        }

        private BaseCarrierViewModel GetCarrierViewModel()
        {
            if (SelectedPlateType == SubstrateType.AutomationCup) return AutoCupVm;
            if (SelectedPlateType == SubstrateType.Plate96) return PlateVm;
            return CarouselVm;
        }

        private List<SampleViewModel> GetCheckedSamples()
        {
            return GetCarrierViewModel().GetSampleWellButtons().Where(
                w => w.IsChecked && w.Sample != null).Select(w => w.Sample).ToList();
        }

        private void SaveSampleSetCarrierOptions()
        {
            SampleSet.SubstrateType = SelectedPlateType;
            SampleSet.PlatePrecession = SelectedPlateType == SubstrateType.Carousel
                ? Precession.RowMajor
                : PlateVm.UsingRowWiseSort ? Precession.RowMajor : Precession.ColumnMajor;
        }

        #endregion

        #region Properties & Fields

        public CarouselViewModel CarouselVm { get; set; }
        public PlateViewModel PlateVm { get; set; }
        public AutomationCupViewModel AutoCupVm { get; set; }
        public SampleTemplateViewModel SampleTemplate { get; set; }

        private RunOptionSettingsModel _runOptionSettings;
        private AutomationConfig _automationConfig;
        private readonly IInstrumentStatusService _instrumentStatusService;

        public string CreatedByUser
        {
            get { return GetProperty<string>(); }
            private set { SetProperty(value); }
        }

        public SampleSetViewModel SampleSet
        {
            get { return GetProperty<SampleSetViewModel>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<SubstrateType> PlateTypes
        {
            get { return GetProperty<ObservableCollection<SubstrateType>>(); }
            private set { SetProperty(value); }
        }

        public SubstrateType SelectedPlateType
        {
            get { return GetProperty<SubstrateType>(); }
            set
            {
                if (value == SubstrateType.AutomationCup && !Misc.ByteToBool(_automationConfig.AutomationIsEnabled))
                    value = SubstrateType.Carousel;

                if (value == SubstrateType.AutomationCup)
                {
                    SampleTemplate.WashType = SamplePostWash.NormalWash;
                    var well = AutoCupVm.GetSampleWellButtons().FirstOrDefault();
                    AutoCupVm.CreateSampleVmForWell(well);
                }

                SampleSet.SubstrateType = value;
                SetProperty(value);

                CarouselVm?.NotifyAllPropertiesChanged();
                PlateVm?.NotifyAllPropertiesChanged();
                AutoCupVm?.NotifyAllPropertiesChanged();
                NotifyPropertyChanged(nameof(ShowCarousel));
                NotifyPropertyChanged(nameof(Show96WellPlate));
                NotifyPropertyChanged(nameof(ShowAutomationCup));
                SampleTemplate.NotifyAllPropertiesChanged();
                UpdateButtons();
            }
        }

        public bool ShowCarousel => SelectedPlateType == SubstrateType.Carousel;
        public bool Show96WellPlate => SelectedPlateType == SubstrateType.Plate96;
        public bool ShowAutomationCup => SelectedPlateType == SubstrateType.AutomationCup &&
                                         Misc.ByteToBool(_automationConfig.AutomationIsEnabled);

        public bool OptionPanelIsOpen
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool AllowSubstrateChanges
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Commands

        #region Cancel Command

        protected override void OnCancel()
        {
            if (!GetCheckedSamples().Any() || DialogEventBus.DialogBoxYesNo(this,
                    LanguageResourceHelper.Get("LID_Dialog_DiscardSampleSetChanges")) == true)
            {
                base.OnCancel();
            }
        }

        #endregion

        #region Accept Command

        public override bool CanAccept()
        {
            return GetCarrierViewModel().GetSampleWellButtons().Any(s => s.WellState == SampleWellState.UsedInCurrentSet);
        }

        private bool ValidateSampleAndSampleSetNames(string setName, List<SampleViewModel> samples)
        {
            if (string.IsNullOrWhiteSpace(setName))
            {
                var sampleSetName = "\"" + LanguageResourceHelper.Get("LID_Label_SampleSetName") + "\"";
                var msg = string.Format(LanguageResourceHelper.Get("LID_ERRMSGBOX_BlankField_With_Double_Quotes"), sampleSetName);

                DialogEventBus.DialogBoxOk(this, msg);
                return false;
            }
            foreach (var sample in samples)
            {
                if (string.IsNullOrWhiteSpace(sample.SampleName))
                {
                    DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_ERRMSGBOX_SampleIDBlank"));
                    return false;
                }
            }

            return true;
        }

        protected override void OnAccept()
        {
            ushort indexCounter = 0;
            var newSamples = GetCheckedSamples();
            if(!ValidateSampleAndSampleSetNames(SampleSet.SampleSetName, newSamples))
            {
                return;
            }
            SampleSet.SampleSetName = SampleSet.SampleSetName.Trim();
            foreach (var sample in newSamples)
            {
                sample.SetSampleIndex(indexCounter++);
                sample.ChangePlatePresession(PlateVm.UsingRowWiseSort ? Precession.RowMajor : Precession.ColumnMajor);
                if (SelectedPlateType == SubstrateType.Carousel)
                {
                    CarouselModel.Instance.Add(new SampleModel(sample.SamplePosition, sample.SampleName.Trim(), sample.Username, sample.SampleSetName.Trim()));
                }
            }
            SampleSet.Samples = new ObservableCollection<SampleViewModel>(newSamples);
            SampleSet.SampleSetStatus = SampleSetStatus.Pending;
            SaveSampleSetCarrierOptions();

            base.OnAccept();
        }

        #endregion

        #region File Load Sample Set Command

        private RelayCommand _fileLoadSampleSetCommand;
        public RelayCommand FileLoadSampleSetCommand => _fileLoadSampleSetCommand ?? (_fileLoadSampleSetCommand = new RelayCommand(PerformImportSampleSet, CanPerformImportSampleSet));

        private bool CanPerformImportSampleSet()
        {
            return true;
        }

        private void PerformImportSampleSet()
        {
            try
            {
                OptionPanelIsOpen = false; // close the mini window

                if (!ImportExportModel.PromptForImportLocation(out string filePath)) return;

                string consoleUsername = LoggedInUser.CurrentUserId;

                var sampleDomains = ImportExportModel.ImportSampleSet(consoleUsername, "", filePath);
                var useCarousel = sampleDomains.Any(s => s.SamplePosition.IsCarousel());
                var rowWiseSort = sampleDomains.All(s => s.RowWisePosition.Equals(ApplicationConstants.Row));
                
                var carrierVm = GetCarrierViewModel();
                var usedSampleWells = carrierVm.OccupiedCarouselPositions();
                var usedInImport = sampleDomains.Select(s => s.SamplePosition).ToList();
                var hasIntersect = usedSampleWells.Intersect(usedInImport).Any();

                if (hasIntersect)
                {
                    var question = LanguageResourceHelper.Get("LID_Dialog_ImportOccupiedWellsMoveThem");
                    if (DialogEventBus.DialogBoxYesNo(this, question) != true) return;

                    var availableWells = useCarousel
                        ? CarouselVm.AvailableCarouselPositions()
                        : PlateVm.AvailableCarouselPositions();

                    var notEnoughWells = availableWells.Count < sampleDomains.Count;

                    for (var i = 0; i < sampleDomains.Count; i++)
                    {
                        var currentSample = sampleDomains[i];

                        if (availableWells.Count > i)
                        {
                            var nextAvailableWell = availableWells[i];

                            // update the position for the imported SampleDomain
                            currentSample.SamplePosition = nextAvailableWell;
                            currentSample.SampleRowPosition = currentSample.SamplePosition.ToString();
                            currentSample.Position = currentSample.SamplePosition.ToString();

                            // update the Carrier VM with the new SampleViewModel
                            var sampleVm = new SampleViewModel(currentSample, _runOptionSettings);
                            var well = useCarousel
                                ? CarouselVm.GetSampleWellButtons().First(sw => sw.SamplePosition.Equals(nextAvailableWell))
                                : PlateVm.GetSampleWellButtons().First(sw => sw.SamplePosition.Equals(nextAvailableWell));
                            well.WellState = SampleWellState.UsedInCurrentSet;
                            well.Sample = sampleVm;
                        }
                    }

                    if (notEnoughWells)
                    {
                        // we don't have enough slots -- tell the user after adding what we can.
                        DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_Dialog_ImportNotEnoughWells"));
                    }
                }
                else
                {
                    // we have enough space: add the samples in their correct slots
                    foreach (var sample in sampleDomains)
                    {
                        var sampleVm = new SampleViewModel(sample, _runOptionSettings);
                        var well = useCarousel
                            ? CarouselVm.GetSampleWellButtons().First(sw => sw.SamplePosition.Equals(sampleVm.SamplePosition))
                            : PlateVm.GetSampleWellButtons().First(sw => sw.SamplePosition.Equals(sampleVm.SamplePosition));
                        well.WellState = SampleWellState.UsedInCurrentSet;
                        well.Sample = sampleVm;
                    }
                }

                // We need to update the row/col sort based on what is in the saved SampleDomains
                carrierVm.SampleSet.PlatePrecession = rowWiseSort ? Precession.RowMajor : Precession.ColumnMajor;
                SampleSet.PlatePrecession = rowWiseSort ? Precession.RowMajor : Precession.ColumnMajor;
                PlateVm.UsingRowWiseSort = rowWiseSort;

                PostToMessageHub(LanguageResourceHelper.Get("LID_MSGBOX_LoadedSampleSetSuccess"));

                if (useCarousel) CarouselVm.NotifyAllPropertiesChanged();
                else PlateVm.NotifyAllPropertiesChanged();

                SampleSet.NotifyAllPropertiesChanged();
                NotifyAllPropertiesChanged();
            }
            catch (Exception ex)
            {
                var msg = LanguageResourceHelper.Get("LID_MSGBOX_LoadedSampleSetFailed");
                Log.Error(msg, ex);
                PostToMessageHub(msg, MessageType.System);
                DialogEventBus.DialogBoxOk(this, msg);
            }
        }

        #endregion

        #region File Save Sample Set Command

        private RelayCommand _fileSaveSampleSetCommand;
        public RelayCommand FileSaveSampleSetCommand => _fileSaveSampleSetCommand ?? (_fileSaveSampleSetCommand = new RelayCommand(PerformExportSampleSet, CanPerformExportSampleSet));

        private bool CanPerformExportSampleSet()
        {
            return GetCheckedSamples().Any();
        }

        private void PerformExportSampleSet()
        {
            try
            {
                OptionPanelIsOpen = false; // close the mini window

                // we need to save a couple things before performing the export:
                SaveSampleSetCarrierOptions();

                var usingRowSort = SampleSet.PlatePrecession == Precession.RowMajor;
                var sampleRecords = GetCheckedSamples().Select(vm => vm.GenerateSampleDomain(usingRowSort)).ToList();
                if (!sampleRecords.Any())
                {
                    DialogEventBus.DialogBoxOk(null, LanguageResourceHelper.Get("LID_MSGBOX_SampleExport"));
                    return;
                }

                if (!ImportExportModel.PromptForExportLocation(out var fileName, out var filePath, SampleSet.SubstrateType))
                {
                    return;
                }
                if (!FileSystem.IsFolderValidForExport(filePath))
                {
                    var invalidPath = LanguageResourceHelper.Get("LID_MSGBOX_QueueManagement_PathError");
                    var msg = $"{invalidPath}";
                    if (FileSystem.IsPathValid(filePath))
                    {
                        string drive = Path.GetPathRoot(filePath);
                        if (drive.ToUpper().StartsWith("C:"))
                            msg += "\n" + LanguageResourceHelper.Get("LID_MSGBOX_ExportPathError");

                    }
                    DialogEventBus.DialogBoxOk(this, msg);
                    return;
                }

                var result = ImportExportModel.ExportSampleSet(sampleRecords, SampleSet.SubstrateType, usingRowSort,
                    fileName, filePath, ApplicationConstants.ExportEncryptKey);

                if (result)
                {
                    PostToMessageHub(LanguageResourceHelper.Get("LID_MessageHub_SaveSampleSetSuccess"));
                }
                else
                {
                    PostToMessageHub(LanguageResourceHelper.Get("LID_MessageHub_SaveSampleSetFailed"), MessageType.System);
                }
            }
            catch (UnauthorizedAccessException unauthorizedAccessException)
            {
                ExceptionHelper.HandleExceptions(unauthorizedAccessException, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_File_Unauthorized"));
            }
            catch (IOException ioException)
            {
                ExceptionHelper.HandleExceptions(ioException, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_File_ERROR"));
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_MessageHub_SaveSampleSetFailed"));
            }
        }

        #endregion

        #region Advanced Settings for Sample Command

        private RelayCommand<SampleViewModel> _sampleAdvancedSettingsCommand;
        public RelayCommand<SampleViewModel> SampleAdvancedSettingsCommand => _sampleAdvancedSettingsCommand ?? (_sampleAdvancedSettingsCommand = new RelayCommand<SampleViewModel>(OpenSampleAdvancedSettings, CanOpenSampleAdvancedSettings));

        protected bool CanOpenSampleAdvancedSettings(SampleViewModel sampleVm)
        {
            return true;
        }

        protected void OpenSampleAdvancedSettings(SampleViewModel sampleVm)
        {
            if (sampleVm == null) return;

            var advSettingsCopy = sampleVm.AdvancedSampleSettings.CreateCopy();
            advSettingsCopy.ShowApplySettingsToAll = false;
            advSettingsCopy.ApplySettingsToAll = false;
            advSettingsCopy.SampleName = sampleVm.SampleName;

            var args = new AdvanceSampleSettingsDialogEventArgs<AdvancedSampleSettingsViewModel>(advSettingsCopy);
            if (DialogEventBus<AdvancedSampleSettingsViewModel>.AdvanceSampleSettingsDialog(this, args) == true)
            {
                sampleVm.AdvancedSampleSettings = args.AdvancedSampleSettingsViewModel;
            }
        }

        #endregion

        #endregion
    }
}