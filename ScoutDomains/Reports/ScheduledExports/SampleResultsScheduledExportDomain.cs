namespace ScoutDomains.Reports.ScheduledExports
{
    public class SampleResultsScheduledExportDomain : BaseScheduledExportDomain
    {
        /// <summary>
        /// Boolean for whether the export will be encrypted or not.
        /// This is only used with SampleResult Scheduled Exports.
        /// </summary>
        public bool IsEncrypted
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public override bool IsValid()
        {
            // RecurrenceRuleDomain has it's own IsValid
            // DataFilterCriteriaDomain has it's own IsValid

            return true;
        }

        public override object Clone()
        {
            var cloneObj = (SampleResultsScheduledExportDomain) base.Clone();
            return cloneObj;
        }
    }
}
