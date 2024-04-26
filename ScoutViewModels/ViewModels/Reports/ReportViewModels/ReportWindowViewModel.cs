using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.Reporting.WinForms;
using ScoutLanguageResources;
using ScoutUtilities;
using ScoutUtilities.Enums;
using ScoutViewModels.ViewModels.Dialogs;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using ScoutDomains;
using ScoutModels.Reports;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Events;
using ScoutViewModels.ViewModels.Reports;

namespace ScoutViewModels.ViewModels
{
    public class ReportWindowViewModel : BaseDialogViewModel
    {

        public ReportWindowViewModel() : base(new BaseDialogEventArgs(dialogLocation: DialogLocation.FullScreen), null)
        {
        }

        #region Properties & Fields

        public string ReportWindowTitle
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string ReportTitle
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public ContentControl ReportContent
        {
            get { return GetProperty<ContentControl>(); }
            set { SetProperty(value); }
        }

        public ReportViewer ReportViewer
        {
            get { return GetProperty<ReportViewer>(); }
            set { SetProperty(value); }
        }

        public ReportType ReportType
        {
	        get { return GetProperty<ReportType>(); }
	        set { SetProperty(value); }
        }

        #endregion

        #region Commands

        private RelayCommand _exportCommand;
        public RelayCommand ExportCommand => _exportCommand ?? (_exportCommand = new RelayCommand(PerformExport));

        #endregion

        #region Virtual Methods

        protected virtual void PerformExport()
        {
            var filename = ReportTitle;
            if (ReportWindowModel.OpenPdfSaveFileDialog(ref filename))
            {
	            string deviceInfo = ApplicationConstants.ReportDeviceInfoForPortrait;
	            if (ReportType == ReportType.CellType ||
	                ReportType == ReportType.CompletedRunSummary)
	            {
		            deviceInfo = ApplicationConstants.ReportDeviceInfoForLandScape;
	            }
                if(!FileSystem.IsFolderValidForExport(filename))
                {
                    var invalidPath = LanguageResourceHelper.Get("LID_MSGBOX_QueueManagement_PathError");
                    var msg = $"{invalidPath}";
                    if (FileSystem.IsPathValid(filename))
                    {
                        var drive = Path.GetPathRoot(filename);
                        if (drive.ToUpper().StartsWith("C:"))
                            msg += "\n" + LanguageResourceHelper.Get("LID_MSGBOX_ExportPathError");
                    }
                    DialogEventBus.DialogBoxOk(this, msg);
                    return;
                }

                if (ReportWindowModel.SaveToPdf(ReportViewer, filename, deviceInfo))
                {
                    MessageBus.Default.Publish(new SystemMessageDomain
                    {
                        IsMessageActive = true,
                        Message = LanguageResourceHelper.Get("LID_MSGBOX_ExportSucces"),
                        MessageType = MessageType.Normal
                    });
                }
            }
        }

        public virtual void LoadReport()
        {
            InitializeLists();
            RetrieveData();
            LoadReportViewer();
        }

        protected virtual void InitializeLists()
        {

        }

        protected virtual void RetrieveData()
        {

        }

        protected virtual void LoadReportViewer()
        {

        }

        protected virtual string GetResourceKeyName(string keyName)
        {
            return !string.IsNullOrEmpty(keyName) ? LanguageResourceHelper.Get(keyName) : null;
        }

        public virtual void AddReportDataSource<T>(string tableName, List<T> reportDataList)
        {
            var reportDataSource = new ReportDataSource
            {
                Name = tableName,
                Value = reportDataList
            };
            ReportViewer.LocalReport.DataSources.Add(reportDataSource);
        }

        public virtual void RefreshAndSetReportContent(string reportPath)
        {
            ReportViewer.LocalReport.ReportPath = reportPath;
            ReportViewer.ShowExportButton = false;
            DisableReportViewerPrintOption();
            ReportViewer.SetDisplayMode(DisplayMode.PrintLayout);
            ReportViewer.AutoScroll = true;
            ReportViewer.VerticalScroll.Enabled = true;
            ReportViewer.VerticalScroll.Visible = true;
            ReportViewer.HorizontalScroll.Visible = true;
            ReportViewer.HorizontalScroll.Enabled = true;

            var toolStrip = (ToolStrip)ReportViewer.Controls.Find("toolStrip1", true)[0];
            if (toolStrip != null)
            {
                toolStrip.GripStyle = ToolStripGripStyle.Visible;
                toolStrip.ImageScalingSize = new Size(45, 45);
            }

            ReportViewer.ShowBackButton = false;
            ReportViewer.ShowStopButton = false;
            ReportViewer.ShowRefreshButton = false;
            ReportViewer.ShowToolBar = true;

            using (var adminViewModel = new ResultsRunResultsViewModel())
            {
                if (!adminViewModel.IsAutoExportPDF)
                {
                    ReportViewer.LocalReport.Refresh();
                    ReportViewer.RefreshReport();
                }
            }
            ReportContent = new ContentControl {Content = new WindowsFormsHost {Child = ReportViewer}};
        }

        #endregion

        #region Private Methods

        private void DisableReportViewerPrintOption()
        {
            foreach (var list in ReportViewer.LocalReport.ListRenderingExtensions().ToList())
            {
                if (list.Name != "Excel" && list.Name != "EXCELOPENXML" && list.Name != "WORD" && list.Name != "WORDOPENXML")
                    continue;

                var exportOption = list.Name;
                var extension = ReportViewer.LocalReport.ListRenderingExtensions().ToList()
                    .Find(x => x.Name.Equals(exportOption, StringComparison.CurrentCultureIgnoreCase));

                if (extension == null) continue;

                var fieldInfo = extension.GetType().GetField("m_isVisible",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                if (fieldInfo != null) fieldInfo.SetValue(extension, false);
            }

            ReportViewer.ShowPrintButton = false;
            ReportViewer.ShowFindControls = false;
        }

        #endregion
    }
}
