using log4net;
using ScoutDomains;
using ScoutModels.Service;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace ScoutModels.Reports
{
    public class LogsModel
    {
        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public LogsModel()
        {
        }

        public void RetrieveAuditLog()
        {
            var auditLogDomain = new List<AuditLogDomain>();
            var hawkeyeError = HawkeyeCoreAPI.AuditLog.RetrieveAuditTrailLogAPI(ref auditLogDomain);
            Log.Debug("RetrieveAuditLog:: hawkeyeError: " + hawkeyeError + ", count: " + auditLogDomain.Count);
            AuditLogDomainList = auditLogDomain;
        }

        public void RetrieveErrorLog()
        {
            var errorLogDomain = new List<ErrorLogDomain>();
            var hawkeyeError = HawkeyeCoreAPI.ErrorLog.RetrieveErrorLogAPI(ref errorLogDomain);
            Log.Debug("RetrieveErrorLog:: hawkeyeError: " + hawkeyeError + ", count: " + errorLogDomain.Count);
            ErrorLogDomainList = errorLogDomain;
        }

        public void RetrieveSampleActivityLog()
        {
            var sampleActivityDomain = new List<SampleActivityDomain>();
            var hawkeyeError = HawkeyeCoreAPI.SampleLog.RetrieveSampleActivityLogAPI(ref sampleActivityDomain);
            Log.Debug("RetrieveSampleActivityLog:: hawkeyeError: " + hawkeyeError + ", count: " + sampleActivityDomain.Count);
            SampleActivityLogList = sampleActivityDomain;
        }

        public void RetrieveCalibrationActivityLog(calibration_type cal_type)
        {
            CalibrationErrorLogList = new List<CalibrationActivityLogDomain>();
            var calibrationErrorLogList = CalibrationModel.RetrieveCalibrationActivityLog(cal_type);
            foreach (var calibrationErrorLog in calibrationErrorLogList)
            {
                if (calibrationErrorLog.Consumable.Count > 0)
                {
                    foreach (var consumable in calibrationErrorLog.Consumable)
                    {
                        var calibrationErrorLogDomain = new CalibrationActivityLogDomain();

                        calibrationErrorLogDomain.UserId = calibrationErrorLog.UserId;
                        calibrationErrorLogDomain.Intercept = calibrationErrorLog.Intercept;
                        calibrationErrorLogDomain.Slope = calibrationErrorLog.Slope;
                        calibrationErrorLogDomain.NumberOfConsumables = calibrationErrorLog.NumberOfConsumables;
                        calibrationErrorLogDomain.CalibrationType = calibrationErrorLog.CalibrationType;
                        calibrationErrorLogDomain.Date = calibrationErrorLog.Date;
                        calibrationErrorLogDomain.ImageCount = calibrationErrorLog.ImageCount;
                        calibrationErrorLogDomain.AssayValue = Misc.UpdateDecimalPoint(Misc.ConvertToPowerSix(consumable.AssayValue));
                        calibrationErrorLogDomain.Label = consumable.Label;
                        calibrationErrorLogDomain.LotId = consumable.LotId;
                        calibrationErrorLogDomain.ExpirationDate = consumable.ExpirationDate;
                        CalibrationErrorLogList.Add(calibrationErrorLogDomain);
                    }
                }
                else
                {
                    CalibrationErrorLogList.Add(calibrationErrorLog);
                }
            }
        }

        private IList<AuditLogDomain> _auditLogDomainList;

        public IList<AuditLogDomain> AuditLogDomainList
        {
            get { return _auditLogDomainList; }
            set { _auditLogDomainList = value; }
        }

		private IList<ErrorLogDomain> _errorLogList;
        public IList<ErrorLogDomain> ErrorLogDomainList
        {
            get { return _errorLogList; }
            set { _errorLogList = value; }
        }

        private IList<SampleActivityDomain> _sampleActiveLogList;
        public IList<SampleActivityDomain> SampleActivityLogList
        {
            get { return _sampleActiveLogList; }
            set { _sampleActiveLogList = value; }
        }

		private IList<CalibrationActivityLogDomain> _calibrationActivityLogList;
		public IList<CalibrationActivityLogDomain> CalibrationErrorLogList
        {
            get { return _calibrationActivityLogList; }
            set { _calibrationActivityLogList = value; }
        }

    }
}
