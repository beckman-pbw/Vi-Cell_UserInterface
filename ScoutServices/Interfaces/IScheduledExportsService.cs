using ScoutDomains.Reports.ScheduledExports;
using ScoutUtilities.Structs;
using System.Collections.Generic;

namespace ScoutServices.Interfaces
{
    public interface IScheduledExportsService
    {
        List<SampleResultsScheduledExportDomain> GetSampleResultsScheduledExports();
        bool AddScheduledExport(SampleResultsScheduledExportDomain scheduledExport);
        bool EditScheduledExport(SampleResultsScheduledExportDomain scheduledExport);

        List<AuditLogScheduledExportDomain> GetAuditLogScheduledExports();
        bool AddScheduledExport(AuditLogScheduledExportDomain scheduledExport);
        bool EditScheduledExport(AuditLogScheduledExportDomain scheduledExport);

        bool DeleteScheduledExport(uuidDLL scheduledExportUuid);
    }
}
