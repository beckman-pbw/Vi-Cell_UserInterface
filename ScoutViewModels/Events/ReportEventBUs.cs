using log4net;
using ScoutViewModels.ViewModels.Reports;
using System;
using System.Windows.Threading;

namespace ScoutViewModels.Events
{
    public static class ReportEventBus
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static Func<Dispatcher> GetUiDispatcher { get; set; }

        public static EventHandler<CellTypesReportViewModel> CellTypesReportRequested;
        public static EventHandler<RunResultsReportViewModel> RunResultsReportRequested;
        public static EventHandler<RunSummaryReportViewModel> RunSummaryReportRequested;
        public static EventHandler<InstrumentStatusReportViewModel> InstrumentStatusReportRequested;
        public static EventHandler<QualityControlsReportViewModel> QualityControlsReportRequested;

        public static bool? CellTypesReport(object caller, CellTypesReportViewModel vm)
        {
            CellTypesReportRequested?.Invoke(caller, vm);
            return vm.DialogResult;
        }

        public static bool? RunResultsReport(object caller, RunResultsReportViewModel vm)
        {
            RunResultsReportRequested?.Invoke(caller, vm);
            return vm.DialogResult;
        }

        public static bool? RunSummaryReport(object caller, RunSummaryReportViewModel vm)
        {
            RunSummaryReportRequested?.Invoke(caller, vm);
            return vm.DialogResult;
        }

        public static bool? InstrumentStatusReport(object caller, InstrumentStatusReportViewModel vm)
        {
            InstrumentStatusReportRequested?.Invoke(caller, vm);
            return vm.DialogResult;
        }

        public static bool? QualityControlsReport(object caller, QualityControlsReportViewModel vm)
        {
            QualityControlsReportRequested?.Invoke(caller, vm);
            return vm.DialogResult;
        }
    }
}