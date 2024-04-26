using ScoutUI.Views._6___Reports;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Events;
using ScoutViewModels.Events;
using ScoutViewModels.ViewModels.Reports;
using System.Windows;

namespace ScoutUI.Views
{
    public class ReportEventManager : BaseEventManager
    {
        public ReportEventManager(Window parent) : base(parent)
        {
            ReportEventBus.GetUiDispatcher = () => ParentWindow.Dispatcher;

            ReportEventBus.CellTypesReportRequested += HandleCellTypesReportRequest;
            ReportEventBus.RunResultsReportRequested += HandleRunResultsReportRequest;
            ReportEventBus.RunSummaryReportRequested += HandleRunSummaryReportRequest;
            ReportEventBus.InstrumentStatusReportRequested += HandleInstrumentStatusReportRequest;
            ReportEventBus.QualityControlsReportRequested += HandleQualityControlsReportRequest;
        }

        protected override void DisposeUnmanaged()
        {
            DialogEventBus.GetUiDispatcher = null;

            ReportEventBus.CellTypesReportRequested -= HandleCellTypesReportRequest;
            ReportEventBus.RunResultsReportRequested -= HandleRunResultsReportRequest;
            ReportEventBus.RunSummaryReportRequested -= HandleRunSummaryReportRequest;
            ReportEventBus.InstrumentStatusReportRequested -= HandleInstrumentStatusReportRequest;
            ReportEventBus.QualityControlsReportRequested -= HandleQualityControlsReportRequest;
            base.DisposeUnmanaged();
        }

        private void HandleCellTypesReportRequest(object sender, CellTypesReportViewModel vm)
        {
            ShowDialog(() => new ReportWindow(vm), vm, new BaseDialogEventArgs());
        }

        private void HandleRunResultsReportRequest(object sender, RunResultsReportViewModel vm)
        {
            ShowDialog(() => new ReportWindow(vm), vm, new BaseDialogEventArgs());
        }

        private void HandleRunSummaryReportRequest(object sender, RunSummaryReportViewModel vm)
        {
            ShowDialog(() => new ReportWindow(vm), vm, new BaseDialogEventArgs());
        }

        private void HandleInstrumentStatusReportRequest(object sender, InstrumentStatusReportViewModel vm)
        {
            ShowDialog(() => new ReportWindow(vm), vm, new BaseDialogEventArgs());
        }

        private void HandleQualityControlsReportRequest(object sender, QualityControlsReportViewModel vm)
        {
            ShowDialog(() => new ReportWindow(vm), vm, new BaseDialogEventArgs());
        }
    }
}