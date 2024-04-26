using HawkeyeCoreAPI.Facade;
using ScoutDataAccessLayer.DAL;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutModels;
using ScoutModels.Admin;
using ScoutModels.Settings;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.Helper;
using ScoutViewModels.ViewModels.ExpandedSampleWorkflow;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutServices;
using ScoutServices.Interfaces;
using ScoutViewModels.Interfaces;

namespace ScoutViewModels.ViewModels.Home.ExpandedSampleWorkflow
{
    public abstract class SampleTemplateViewModel : BaseViewModel, ICloneable
    {
        protected SampleTemplateViewModel(ObservableCollection<CellTypeQualityControlGroupDomain> qcCellTypes, 
            CellTypeQualityControlGroupDomain qcCellType, uint dilution, ObservableCollection<SamplePostWash> washes, 
            SamplePostWash wash, string tag, bool useSequencing, SequentialNamingSetViewModel sequentialNamingSet,
            AdvancedSampleSettingsViewModel advancedSampleSettings, RunOptionSettingsModel runOptions, UserDomain user, ISampleProcessingService sampleProcessingService,IScoutViewModelFactory viewModelFactory)
        {
            _runOptionSettingsModel = runOptions;
            _sampleProcessingService = sampleProcessingService;
            _viewModelFactory = viewModelFactory; 
            SequentialNamingItems = sequentialNamingSet ?? new SequentialNamingSetViewModel(
                                        _runOptionSettingsModel.RunOptionsSettings.DefaultSampleId);
            AdvancedSampleSettings = advancedSampleSettings ?? new AdvancedSampleSettingsViewModel(
                                         _runOptionSettingsModel);
            UseSequencing = useSequencing;
            User = user;
            QcCellType = qcCellType;
            Dilution = dilution;
            WashType = wash;
            SampleTag = tag;
            WashTypes = washes;
            QcCellTypes = qcCellTypes;
            if(QcCellType == null)
            {
                QcCellType = QcCellTypes.FirstOrNull();
            }
            
            NotifyPropertyChanged(nameof(DisplayedSampleName));
        }

        protected SampleTemplateViewModel(RunOptionSettingsModel runOptions, ISampleProcessingService sampleProcessingService, IScoutViewModelFactory viewModelFactory,CellTypeFacade cellTypeFacade)
        {
            _runOptionSettingsModel = runOptions;
            _sampleProcessingService = sampleProcessingService;
            _viewModelFactory = viewModelFactory;
            SequentialNamingItems = new SequentialNamingSetViewModel(
                                        _runOptionSettingsModel.RunOptionsSettings.DefaultSampleId);
            AdvancedSampleSettings = new AdvancedSampleSettingsViewModel(
                                         _runOptionSettingsModel);
            UseSequencing = false;
            User = LoggedInUser.CurrentUser;
            QcCellType = _runOptionSettingsModel.BpQcGroupList.GetCellTypeQualityControlByIndex(_runOptionSettingsModel.RunOptionsSettings.DefaultBPQC);
            Dilution = uint.Parse(_runOptionSettingsModel.RunOptionsSettings.DefaultDilution);
            WashType = _runOptionSettingsModel.RunOptionsSettings.DefaultWash;
            SampleTag = string.Empty;
            WashTypes = new ObservableCollection<SamplePostWash>(_runOptionSettingsModel.WashList);
            if (User != null)
            {
                QcCellTypes = new ObservableCollection<CellTypeQualityControlGroupDomain>(LoggedInUser.GetCtQcs());
                if (QcCellType == null)
                {
                    QcCellType = QcCellTypes.FirstOrNull();
                }
            }
            
            NotifyPropertyChanged(nameof(DisplayedSampleName));
        }

        //only used for unit tests
        protected SampleTemplateViewModel(ObservableCollection<CellTypeQualityControlGroupDomain> qcCellTypes,
            CellTypeQualityControlGroupDomain qcCellType, uint dilution, ObservableCollection<SamplePostWash> washes,
            SamplePostWash wash, string tag, bool useSequencing, SequentialNamingSetViewModel sequentialNamingSet,
            AdvancedSampleSettingsViewModel advancedSampleSettings, RunOptionSettingsModel runOptions, UserDomain user, ISampleProcessingService sampleProcessingService, IScoutViewModelFactory viewModelFactory, CellTypeFacade cellTypeFacade)
        {
            _runOptionSettingsModel = runOptions;
            _sampleProcessingService = sampleProcessingService;
            _viewModelFactory = viewModelFactory;
            _cellTypeFacade = cellTypeFacade;
            SequentialNamingItems = sequentialNamingSet ?? new SequentialNamingSetViewModel(
                                        _runOptionSettingsModel.RunOptionsSettings.DefaultSampleId);
            AdvancedSampleSettings = advancedSampleSettings ?? new AdvancedSampleSettingsViewModel(
                                         _runOptionSettingsModel);
            UseSequencing = useSequencing;
            User = user;
            QcCellType = qcCellType;
            Dilution = dilution;
            WashType = wash;
            SampleTag = tag;
            WashTypes = washes;
            QcCellTypes = qcCellTypes;
            if (QcCellType == null)
            {
                QcCellType = QcCellTypes.FirstOrNull();
            }

            NotifyPropertyChanged(nameof(DisplayedSampleName));
        }
        protected SampleTemplateViewModel(SampleTemplateViewModel sampleTemplateViewModel)
        {
            _runOptionSettingsModel = sampleTemplateViewModel._runOptionSettingsModel;
            _viewModelFactory = sampleTemplateViewModel._viewModelFactory;
            _sampleProcessingService = sampleTemplateViewModel._sampleProcessingService;
            SequentialNamingItems = sampleTemplateViewModel.SequentialNamingItems;
            AdvancedSampleSettings = sampleTemplateViewModel.AdvancedSampleSettings;
            UseSequencing = sampleTemplateViewModel.UseSequencing;
            User = sampleTemplateViewModel.User;
            QcCellType = sampleTemplateViewModel.QcCellType;
            Dilution = sampleTemplateViewModel.Dilution;
            WashType = sampleTemplateViewModel.WashType;
            SampleTag = sampleTemplateViewModel.SampleTag;
            WashTypes = sampleTemplateViewModel.WashTypes;
            QcCellTypes = sampleTemplateViewModel.QcCellTypes;
            QcCellType = sampleTemplateViewModel.QcCellType;

            NotifyPropertyChanged(nameof(DisplayedSampleName));
        }

        public SampleTemplateViewModel()
        {

        }

        protected override void DisposeUnmanaged()
        {
            SequentialNamingItems.Dispose();
            base.DisposeUnmanaged();
        }

        #region Properties & Fields

        protected RunOptionSettingsModel _runOptionSettingsModel;
        protected ISampleProcessingService _sampleProcessingService;
        protected IScoutViewModelFactory _viewModelFactory;
        private readonly CellTypeFacade _cellTypeFacade;

        public UserDomain User { get; set; }

        public event EventHandler<AdvanceSampleSettingsDialogEventArgs<AdvancedSampleSettingsViewModel>> OnAdvancedSettingsApplyToAllClicked;

        public SequentialNamingSetViewModel SequentialNamingItems { get; set; }
        
        public AdvancedSampleSettingsViewModel AdvancedSampleSettings { get; set; }

        public virtual string DisplayedSampleName // this is what is shown on the GUI -- may contain sequencing representation
        {
            get
            {
                return SequentialNamingItems.GetSampleNameForDisplay(UseSequencing);
            }
            set
            {
                SequentialNamingItems.SetBaseString(value);
                AdvancedSampleSettings.SampleName = SequentialNamingItems.GetSampleNameForDisplay(UseSequencing);
                NotifyPropertyChanged(nameof(DisplayedSampleName));
            }
        }

        public bool UseSequencing
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public uint Dilution
        {
            get { return GetProperty<uint>(); }
            set
            {
                if ((value >= ApplicationConstants.MinimumDilutionFactor) &&
                    (value <= ApplicationConstants.MaximumDilutionFactor))
                {
                    SetProperty(value);
                }
            }
        }

        public string SampleTag
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<CellTypeQualityControlGroupDomain> QcCellTypes
        {
            get { return GetProperty<ObservableCollection<CellTypeQualityControlGroupDomain>>(); }
            set { SetProperty(value); }
        }

        public bool IsQualityControl
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public CellTypeQualityControlGroupDomain QcCellType
        {
            get { return GetProperty<CellTypeQualityControlGroupDomain>(); }
            set
            {
                SetProperty(value);
                if (value != null)
                {
                    IsQualityControl = value.SelectedCtBpQcType == CtBpQcType.QualityControl;
                }
            }
        }

        public ObservableCollection<SamplePostWash> WashTypes
        {
            get { return GetProperty<ObservableCollection<SamplePostWash>>(); }
            set { SetProperty(value); }
        }

        public bool IsEnabled
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public virtual bool IsFastModeEnabled
        {
            get
            {
                if (IsAdminOrServiceUser) return true;
                if (LoggedInUser.IsConsoleUserLoggedIn())
                {
                    return LoggedInUser.CurrentUser.IsFastModeEnabled;
                }
                return false;
            }
            set { SetProperty(value); }
        }

        public SamplePostWash WashType
        {
            get { return IsFastModeEnabled ? GetProperty<SamplePostWash>() : SamplePostWash.NormalWash; }
            set { SetProperty(value); }
        }

        #endregion

        #region Public Methods

        public abstract object Clone();
        public void CopyValues(SampleTemplateViewModel target)
        {
            target.QcCellTypes = new ObservableCollection<CellTypeQualityControlGroupDomain>(QcCellTypes);
                target.QcCellType = QcCellType;
                target.Dilution = Dilution;
                target.WashTypes = new ObservableCollection<SamplePostWash>(WashTypes);
                target.WashType = WashType;
                target.SampleTag = SampleTag;
                target.UseSequencing = UseSequencing;
                target.SequentialNamingItems = (SequentialNamingSetViewModel) SequentialNamingItems.Clone();
                target.AdvancedSampleSettings = (AdvancedSampleSettingsViewModel) AdvancedSampleSettings.Clone();
                target._runOptionSettingsModel = _runOptionSettingsModel;
                target.User = User;
                target._sampleProcessingService = _sampleProcessingService;
                target._viewModelFactory = _viewModelFactory;
        }

      

        public SampleSetTemplateDomain GetSampleSetTemplateDomain()
        {
            var sampleId = string.Empty;
            var textFirst = true;
            ushort numDigits = 0;
            ushort startDigit = 0;
            if (SequentialNamingItems != null)
            {
                sampleId = SequentialNamingItems.GetBaseString();
                textFirst = SequentialNamingItems.TextFirst;
                var numberSeq = SequentialNamingItems.GetIntegerItem();
                if (numberSeq != null)
                {
                    numDigits = numberSeq.NumberOfDigits ?? 0;
                    startDigit = numberSeq.StartingDigit ?? 0;
                }
            }

            var template = new SampleSetTemplateDomain(QcCellType.CellTypeIndex, QcCellType.Name,
                Dilution, SampleTag, WashType, sampleId, AdvancedSampleSettings.NthImage, UseSequencing, textFirst,
                sampleId, numDigits, startDigit);
            return template;
        }

        #endregion

        #region Commands

        private RelayCommand _templateSettingsCommand;
        public RelayCommand TemplateSettingsCommand => _templateSettingsCommand ?? (_templateSettingsCommand = new RelayCommand(OpenTemplateSettings));

        private void OpenTemplateSettings()
        {
            var args = new SequentialNamingEventArgs(UseSequencing, SequentialNamingItems.GetSequentialNamingItems());
            if (DialogEventBus.SequentialNamingDialog(this, args) == true)
            {
                UseSequencing = args.UseSequencing;
                SequentialNamingItems = new SequentialNamingSetViewModel(args.SeqNamingItems);
                AdvancedSampleSettings.SampleName = SequentialNamingItems.GetSampleNameForDisplay(UseSequencing);
                NotifyPropertyChanged(nameof(DisplayedSampleName));
            }
        }

        private RelayCommand _advancedSettingsCommand;
        public RelayCommand AdvancedSettingsCommand => _advancedSettingsCommand ?? (_advancedSettingsCommand = new RelayCommand(OpenAdvancedSettings));

        private void OpenAdvancedSettings()
        {
            var advSettings = AdvancedSampleSettings.CreateCopy();
            var args = new AdvanceSampleSettingsDialogEventArgs<AdvancedSampleSettingsViewModel>(advSettings);
            if (DialogEventBus<AdvancedSampleSettingsViewModel>.AdvanceSampleSettingsDialog(this, args) == true)
            {
                AdvancedSampleSettings = args.AdvancedSampleSettingsViewModel;
                if (AdvancedSampleSettings.ApplySettingsToAll)
                {
                    OnAdvancedSettingsApplyToAllClicked?.Invoke(this, args);
                }
            }
        }

        private RelayCommand _eswInfoCommand;
        public RelayCommand EswInfoCommand => _eswInfoCommand ?? (_eswInfoCommand = new RelayCommand(OpenInfoDialog));

        private void OpenInfoDialog()
        {
            DialogEventBus.InfoDialog(this, new InfoDialogEventArgs());
        }

        #endregion
    }
}