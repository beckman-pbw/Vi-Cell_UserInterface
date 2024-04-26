using HawkeyeCoreAPI;
using ScoutDomains.Reports.ScheduledExports;
using ScoutServices.Interfaces;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System.Collections.Generic;

namespace ScoutServices
{
    public class ScheduledExportsService : IScheduledExportsService
    {
        #region Sample Result Scheduled Exports

        public List<SampleResultsScheduledExportDomain> GetSampleResultsScheduledExports()
        {
            var error = ScheduledExportApi.GetScheduledExportsApi(out var list);
            return list;
        }

        public bool AddScheduledExport(SampleResultsScheduledExportDomain scheduledExport)
        {
            var error = ScheduledExportApi.AddScheduledExportApi(scheduledExport);
            return error == HawkeyeError.eSuccess;
        }

        public bool EditScheduledExport(SampleResultsScheduledExportDomain scheduledExport)
        {
            var error = ScheduledExportApi.EditScheduledExportApi(scheduledExport);
            return error == HawkeyeError.eSuccess;
        }

        #endregion

        #region Audit Log Scheduled Exports

        public List<AuditLogScheduledExportDomain> GetAuditLogScheduledExports()
        {
            var error = ScheduledExportApi.GetAuditLogScheduledExportsApi(out var list);
            return list;
        }

        public bool AddScheduledExport(AuditLogScheduledExportDomain scheduledExport)
        {
            var error = ScheduledExportApi.AddScheduledExportApi(scheduledExport);
            return error == HawkeyeError.eSuccess;
        }

        public bool EditScheduledExport(AuditLogScheduledExportDomain scheduledExport)
        {
            var error = ScheduledExportApi.EditScheduledExportApi(scheduledExport);
            return error == HawkeyeError.eSuccess;
        }

        #endregion

        public bool DeleteScheduledExport(uuidDLL scheduledExportUuid)
        {
            var error = ScheduledExportApi.DeleteScheduledExportApi(scheduledExportUuid);
            return error == HawkeyeError.eSuccess;
        }
    }
}
