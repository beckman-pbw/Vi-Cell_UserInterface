using log4net;
using ScoutDomains;
using System.Collections.Generic;

namespace ScoutModels.QualityControl
{
    public class QualityControlModel
    {
        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Public Function        

        public static List<SampleRecordDomain> RetrieveSampleRecordsForQualityControl(string qcName)
        {
            Log.Debug($"RetrieveSampleRecordsForQualityControl:: qcName: {qcName}");
            var hawkeyeError = HawkeyeCoreAPI.QualityControl.RetrieveSampleRecordsForQualityControlAPI(
                qcName, out var qualityControlDomainList, out var numSamples);
            Log.Debug($"RetrieveSampleRecordsForQualityControl:: hawkeyeError: {hawkeyeError}, numSamples: {numSamples}");
            return qualityControlDomainList;
        }

        #endregion
    }
}