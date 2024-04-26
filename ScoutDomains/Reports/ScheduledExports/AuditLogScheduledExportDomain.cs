namespace ScoutDomains.Reports.ScheduledExports
{
    public class AuditLogScheduledExportDomain : BaseScheduledExportDomain
    {
        /// <summary>
        /// Whether the scheduled export will include the Audit Log.
        /// This is ONLY used for Audit Log Scheduled Exports.
        /// </summary>
        public bool IncludeAuditLog
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        /// <summary>
        /// Whether the scheduled export will include the Audit Log.
        /// This is ONLY used for Audit Log Scheduled Exports.
        /// </summary>
        public bool IncludeErrorLog
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public override object Clone()
        {
            var cloneObj = (AuditLogScheduledExportDomain) base.Clone();
            return cloneObj;
        }

        public override bool IsValid()
        {
            // RecurrenceRuleDomain has it's own IsValid
            // DataFilterCriteriaDomain has it's own IsValid

            if (!IncludeErrorLog && !IncludeAuditLog)
                return false;
            
            return true;
        }
    }
}