using ScoutDomains.Common;
using ScoutModels.Settings;
using ScoutUtilities.Common;
using System;
using System.IO;

namespace ScoutViewModels.ViewModels.ExpandedSampleWorkflow
{
    public class AdvancedSampleSettingsViewModel : BaseViewModel, ICloneable
    {
        public AdvancedSampleSettingsViewModel(RunOptionSettingsModel runOptionSettings, bool showApplySettingsToAll = false, 
            bool applySettingsToAll = false)
        {
            _runOptionSettingsModel = runOptionSettings;
            var runOptions = _runOptionSettingsModel.RunOptionsSettings;
            SampleName = runOptions.DefaultSampleId;
            NthImage = uint.TryParse(runOptions.NumberOfImages, out var n) ? n : (uint) 1;
            ExportSamples = runOptions.IsExportSampleResultActive;
            ExportSamplesAsPdf = runOptions.IsAutoExportPDFSelected;
            ExportSampleDirectory = runOptions.ExportSampleResultPath;
            AppendSampleExport = runOptions.IsAppendSampleResultExportActive;
            AppendExportDirectory = runOptions.AppendSampleResultPath;
            AppendExportFileName = runOptions.DefaultFileName;

            ShowApplySettingsToAll = showApplySettingsToAll;
            ApplySettingsToAll = showApplySettingsToAll;
            ValidateDirectories();
        }

        public AdvancedSampleSettingsViewModel(SampleDomain sampleDomain, RunOptionSettingsModel runOptionSettings)
        {
            _runOptionSettingsModel = runOptionSettings;

            SampleName = sampleDomain.SampleName;

            NthImage = (uint) sampleDomain.NthImage;
            ExportSamples = sampleDomain.IsExportEachSampleActive;
            ExportSamplesAsPdf = sampleDomain.IsExportPDFSelected;
            ExportSampleDirectory = sampleDomain.ExportPathForEachSample;
            AppendSampleExport = sampleDomain.IsAppendResultExport;
            AppendExportDirectory = sampleDomain.AppendResultExport;
            AppendExportFileName =  sampleDomain.ExportFileName;
            
            ShowApplySettingsToAll = false;
            ApplySettingsToAll = false;            
            ValidateDirectories();
        }

        public AdvancedSampleSettingsViewModel(AdvancedSampleSettingsViewModel copy)
        {
            SampleName = copy.SampleName;
            NthImage = copy.NthImage;
            ExportSamples = copy.ExportSamples;
            ExportSamplesAsPdf = copy.ExportSamplesAsPdf;
            ExportSampleDirectory = copy.ExportSampleDirectory;
            AppendSampleExport = copy.AppendSampleExport;
            AppendExportDirectory = copy.AppendExportDirectory;
            AppendExportFileName = copy.AppendExportFileName;

            ShowApplySettingsToAll = copy.ShowApplySettingsToAll;
            ApplySettingsToAll = copy.ApplySettingsToAll;

            _runOptionSettingsModel = copy._runOptionSettingsModel;
            ValidateDirectories();
        }

        private void ValidateDirectories()
        {
            if (!FileSystem.IsFolderValidForExport(ExportSampleDirectory))
                ExportSampleDirectory = FileSystem.GetDefaultExportPath(CurrentRunOptions.Username);

            if (!FileSystem.IsFolderValidForExport(AppendExportDirectory))
                AppendExportDirectory = FileSystem.GetDefaultExportPath(CurrentRunOptions.Username);
        }

        #region Properties & Fields

        public RunOptionSettingsModel CurrentRunOptions
        {
            get { return _runOptionSettingsModel; }
            private set { _runOptionSettingsModel = value; }
        }
        private RunOptionSettingsModel _runOptionSettingsModel;

        public string SampleName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public uint NthImage
        {
            get { return GetProperty<uint>(); }
            set { SetProperty(value); }
        }

        public bool ExportSamples
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool ExportSamplesAsPdf
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string ExportSampleDirectory
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool AppendSampleExport
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string AppendExportDirectory
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string AppendExportFileName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool ShowApplySettingsToAll
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool ApplySettingsToAll
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Methods

        public AdvancedSampleSettingsViewModel CreateCopy()
        {
            return (AdvancedSampleSettingsViewModel) Clone();
        }

        public object Clone()
        {
            return new AdvancedSampleSettingsViewModel(this);
        }
        
        #endregion
    }
}