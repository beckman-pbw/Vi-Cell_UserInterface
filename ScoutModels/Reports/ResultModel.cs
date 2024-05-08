using System;
using log4net;
using ScoutDomains;
using ScoutModels.Admin;
using ScoutModels.QualityControl;
using ScoutModels.Settings;
using ScoutUtilities.Structs;
using System.Collections.Generic;
using ScoutUtilities;

namespace ScoutModels.Reports
{
    public class ResultModel
    {
        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public UserModel UserModel { get; set; }

        public ResultModel()
        {
            UserModel = new UserModel();
        }

        public List<SampleRecordDomain> RetrieveSampleRecordsForQualityControl(string qcName)
        {
            return QualityControlModel.RetrieveSampleRecordsForQualityControl(qcName);
        }

        public static List<SampleRecordDomain> RetrieveSampleRecords(ulong startDate, ulong endDate, string username)
        {
            Log.Debug("RetrieveSampleRecords:: startDate: " + startDate + ", endDate: " + endDate + ", username: " + username);
            var hawkeyeError = HawkeyeCoreAPI.Sample.RetrieveSampleRecordsAPI(startDate, endDate, username, out var sampleRecordList, out var listSize);
            Log.Debug("RetrieveSampleRecords:: hawkeyeError: " + hawkeyeError + ", listSize: " + listSize);
            return sampleRecordList;
        }

        public static SampleRecordDomain RetrieveSampleRecord(uuidDLL id)
        {
            try
            {
                Log.Debug($"RetrieveSampleRecord:: uuid: {id}");
                var hawkeyeError = HawkeyeCoreAPI.Sample.RetrieveSampleRecordAPI(id, out var sampleRecord);
                Misc.LogOnHawkeyeError($"RetrieveSampleRecord", hawkeyeError);
                return sampleRecord;
            }
            catch (Exception e)
            {
                Log.Error($"Failed to get Sample Record: '{id}'", e);
                return null;
            }
        }
    }
}
