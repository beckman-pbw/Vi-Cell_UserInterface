using log4net;
using ScoutLanguageResources;
using ScoutUtilities.Enums;
using ScoutUtilities.UIConfiguration;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ScoutUtilities.Common;

namespace ScoutUtilities
{
    public class Misc
    {
        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static bool IsHistogramEnable { get; set; }

        public static bool IsRowWiseAddPosition { get; set; }

        public static TrailingPoint ConcDisplayDigits { get; set; } = TrailingPoint.Two;

        public static byte BoolToByte(bool b)
        {
            return b ? (byte) 1 : (byte) 0;
        }

        public static bool ByteToBool(byte b)
        {
            return b != 0;
        }

        public static string UpdateTrailingPoint(double value, TrailingPoint point, CultureInfo culture = null)
        {
            if (culture == null)
                culture = LanguageResourceHelper.CurrentFormatCulture;
            return UpdateTrailingPoint(value, culture, point);
        }

        public static string UpdateTrailingPoint(double value, CultureInfo culture, TrailingPoint point)
        {
            switch (point)
            {
                case TrailingPoint.One:
                    return value.ToString("F1", culture);
                case TrailingPoint.Two:
                    return value.ToString("F2", culture);
                case TrailingPoint.Three:
                    return value.ToString("F3", culture);
                case TrailingPoint.Four:
                    return value.ToString("F4", culture);
            }

            return value.ToString("F2", culture);
        }
       
        public static double ConvertToPowerSix(double value)
        {
            // Convert given value to 10 to the power 6
            var result = value / 1000000;
            return result;
        }

        public static string ConvertToPower(double value, CultureInfo culture = null)
        {
            // Convert given value to 10 to the power 6
            var result = value / 1000000;
            return UpdateTrailingPoint(result, culture, TrailingPoint.Two);
        }

        public static string ConvertToConcPower(double value, CultureInfo culture = null)
        {
            // Convert given concentration value to 10 to the power 6
            var result = value / 1000000;
            return UpdateTrailingPoint(result, culture, ConcDisplayDigits);
        }

        public static string ConvertBytesToSize(double bytes)
        {
            const long byteConversion = 1024;

            // Validating for TB
            if (bytes >= Math.Pow(byteConversion, 4))
            {
                return ConvertToSize(bytes, byteConversion, 4, " TB");
            }

            // Validating for GB
            if (bytes >= Math.Pow(byteConversion, 3))
            {
                return ConvertToSize(bytes, byteConversion, 3, " GB");
            }

            // Validating for MB
            if (bytes >= Math.Pow(byteConversion, 2))
            {
                return ConvertToSize(bytes, byteConversion, 2, " MB");
            }

            // Validating for KB
            if (bytes >= bytes / byteConversion)
            {
                return ConvertToSize(bytes, byteConversion, 1, " KB");
            }

            // Convert to Bytes
            return ConvertToSize(bytes, 1, 1, " Bytes");
        }


        private static string ConvertToSize(double bytes, double byteConversion, int power, string format)
        {
            return $"{Math.Truncate(bytes / Math.Pow(byteConversion, power) * 10) / 10:N1}".ToString(LanguageResourceHelper.CurrentFormatCulture) + format;
        }

        public static string ConvertToString(byte? input)
        {
            return !input.HasValue ? string.Empty : input.Value.ToString(LanguageResourceHelper.CurrentFormatCulture);
        }

        public static string ConvertToString(int? input)
        {
            return !input.HasValue ? string.Empty : input.Value.ToString(LanguageResourceHelper.CurrentFormatCulture);
        }

        public static string ConvertToString(uint? input)
        {
            return !input.HasValue ? string.Empty : input.Value.ToString(LanguageResourceHelper.CurrentFormatCulture);
        }

        public static string ConvertToString(ushort? input)
        {
            return !input.HasValue ? string.Empty : input.Value.ToString(LanguageResourceHelper.CurrentFormatCulture);
        }

        public static string ConvertToString(long? input)
        {
            return !input.HasValue ? string.Empty : input.Value.ToString(LanguageResourceHelper.CurrentFormatCulture);
        }

        public static string ConvertToString(ulong? input)
        {
            return !input.HasValue ? string.Empty : input.Value.ToString(LanguageResourceHelper.CurrentFormatCulture);
        }

        public static string ConvertToString(float? input)
        {
            return !input.HasValue ? string.Empty : UpdateTrailingPoint(input.Value, LanguageResourceHelper.CurrentFormatCulture, TrailingPoint.Two);
        }

        public static string ConvertToString(double? input)
        {
            return !input.HasValue ? string.Empty : UpdateTrailingPoint(input.Value, LanguageResourceHelper.CurrentFormatCulture, TrailingPoint.Two);
        }

        public static string ConvertToConcString(double? input)
        {
            return !input.HasValue ? string.Empty : UpdateTrailingPoint(input.Value, LanguageResourceHelper.CurrentFormatCulture, ConcDisplayDigits);
        }

        public static string ConvertToString(DateTime? input)
        {
            return !input.HasValue ? string.Empty : input.Value.ToString("d", LanguageResourceHelper.CurrentFormatCulture);
        }

        public static string ConvertToString(TimeSpan input)
        {
            return input.ToString();
        }

        public static string ConvertToCustomDateFormat(DateTime value, string format)
        {
            return value.ToString(format, LanguageResourceHelper.CurrentDisplayCulture);
        }

        public static string ConvertToCustomLongDateTimeFormat(DateTime value)
        {
            return value.ToString("G", LanguageResourceHelper.CurrentDisplayCulture);
        }

        public static string ConvertToCustomShortDateTimeFormat(DateTime value)
        {
            return value.ToString("g", LanguageResourceHelper.CurrentDisplayCulture);
        }
     
        public static string ConvertToCustomDateOnlyFormat(DateTime value)
        {
            return value.ToString("d", LanguageResourceHelper.CurrentDisplayCulture);
        }
     
        public static string ConvertToSampleSetNameDefaultDateOnlyFormat(DateTime value)
        {
            //    d Format Specifier      de-DE Culture                               01.10.2008
            //    d Format Specifier      en-US Culture                                10/1/2008
            //    d Format Specifier      es-ES Culture                               01/10/2008
            //    d Format Specifier      fr-FR Culture                               01/10/2008
            var str = value.ToString("d", LanguageResourceHelper.CurrentDisplayCulture);

            //    d Format Specifier      de-DE Culture                               01.10.2008
            //    d Format Specifier      en-US Culture                                10-1-2008
            //    d Format Specifier      es-ES Culture                               01-10-2008
            //    d Format Specifier      fr-FR Culture                               01-10-2008
            return str.Replace('/', '-');
        }
     
        public static string ConvertToFileNameFormat(DateTime value)
        {
            return ConvertToCustomDateFormat(value, "yyyyMMdd_HHmmss");
        }

        public static double UpdateDecimalPoint(double input, TrailingPoint? tp = TrailingPoint.Two)
        {
            var digits = 0;
            if(tp.HasValue)
            {
                switch (tp)
                {
                    case TrailingPoint.One:
                        digits = 1;
                        break;
                    case TrailingPoint.Two:
                        digits = 2;
                        break;
                    case TrailingPoint.Three:
                        digits = 3;
                        break;
                    case TrailingPoint.Four:
                        digits = 4;
                        break;
                }
            }
            return Math.Round(input, digits, MidpointRounding.AwayFromZero);
        }

        public static double? DoubleTryParse(string doubleStr)
        {
            if(string.IsNullOrEmpty(doubleStr) ||
               !double.TryParse(doubleStr, NumberStyles.Any, LanguageResourceHelper.CurrentFormatCulture, out var value))
            {
                return null;
            }
            return value;
        }

        public static void LogOnHawkeyeError(string msg, HawkeyeError hawkeyeError)
        {
            if (hawkeyeError != HawkeyeError.eSuccess)
                Log.Error(msg + " hawkeyeError: " + hawkeyeError);
        }

        public static string GetBaseQualityControlName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            var qcName = string.Empty;
            if (!name.Contains("(") && !name.Contains(")")) return name;

            var splitParams = new[] { '(', ')' };
            var splitResult = name.Split(splitParams);
            if (splitResult.Any())
            {
                qcName = splitResult[0];
            }
            return qcName.Trim();
        }

        public static string GetParenthesisQualityControlName(string qcName, string cellTypeName)
        {
            if (qcName.Contains("(") && qcName.Contains(")")) return qcName;

            return $"{qcName} ({cellTypeName})";
        }

        public static string GetUiVersionString()
        {
            return $"{LanguageResourceHelper.Get("LID_Title_ViCellBluVersion")}{UISettings.SoftwareVersion}";
        }

        public static string GetCopyright()
        {
            return string.Format(LanguageResourceHelper.Get("LID_Label_CopyRight"), UISettings.CopyrightYear);
        }

        public static string ObjectToString(object obj)
        {
            var sb = new StringBuilder();
            var name = string.Empty;
            
            try
            {
                var type = obj.GetType();
                sb.Append($"{type.Name} object{Environment.NewLine}");
                foreach (var propInfo in type.GetProperties())
                {
                    name = propInfo.Name ?? "[NULL PROPERTY NAME]";
                    if (propInfo.GetGetMethod(false) == null) continue;
                    var value = propInfo.GetValue(obj)?.ToString() ?? string.Empty;
                    sb.Append($"\t{name}: '{value}'{Environment.NewLine}");
                }

                return sb.ToString();
            }
            catch (Exception e)
            {
                Log.Warn($"Unable to create generic ToString for object (Property name at error: '{name}')", e);
                return sb?.ToString() ?? "[Failed to get string for object]";
            }
        }

        public static string GetConcentrationSampleName(AssayValueEnum assayValue, int sampleIndex)
        {
            switch (assayValue)
            {
                case AssayValueEnum.M2:
                    return $"{ApplicationConstants.ConcSlope2M}.{sampleIndex:D3}";
                case AssayValueEnum.M4:
                    return $"{ApplicationConstants.ConcSlope4M}.{sampleIndex:D3}";
                case AssayValueEnum.M10:
                    return $"{ApplicationConstants.ConcSlope10M}.{sampleIndex:D3}";
            }

            return string.Empty;
        }

        public static bool ContainsInvalidCharacter(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;
            var invalidChar = Path.GetInvalidFileNameChars();
            var textArray = text.ToCharArray();
            return textArray.Any(x => invalidChar.Any(y => y.Equals(x)));
        }

		public static string AuditEventString(audit_event_type evt)
		{
			switch (evt)
			{
				case audit_event_type.evt_login: return "Login";
				case audit_event_type.evt_logout: return "Logout";
				case audit_event_type.evt_logoutforced: return "Logout Forced";
				case audit_event_type.evt_loginfailure: return "Login Failure";
				case audit_event_type.evt_accountlockout: return "Account Lockout";
				case audit_event_type.evt_useradd: return "User Added";
				case audit_event_type.evt_userremove: return "User Removed";
				case audit_event_type.evt_userenable: return "User Enable";
				case audit_event_type.evt_userdisable: return "User Disable";
				case audit_event_type.evt_passwordchange: return "Password Change";
				case audit_event_type.evt_passwordreset: return "Password Reset";
				case audit_event_type.evt_userpermissionschange: return "User Permission Change";
				case audit_event_type.evt_securityenable: return "Security Enable";
				case audit_event_type.evt_securitydisable: return "Security Disable";
				case audit_event_type.evt_celltypecreate: return "CellType Create";
				case audit_event_type.evt_celltypemodify: return "CellType Modify";
				case audit_event_type.evt_celltypedelete: return "CellType Delete";
				case audit_event_type.evt_analysiscreate: return "Analysis Create";
				case audit_event_type.evt_analysismodify: return "Analysis Modify";
				case audit_event_type.evt_analysisdelete: return "Analysis Delete";
				case audit_event_type.evt_bioprocesscreate: return "Bioprocess Create";
				case audit_event_type.evt_bioprocessdelete: return "Bioprocess Delete";
				case audit_event_type.evt_qcontrolcreate: return "QualityControl Create";
				case audit_event_type.evt_qcontroldelete: return "QualityControl Delete";
				case audit_event_type.evt_fluidicsflush: return "Fluidics Flush";
				case audit_event_type.evt_fluidicsprime: return "Fluidics Prime";
				case audit_event_type.evt_fluidicsdrain: return "Fluidics Drain";
				case audit_event_type.evt_fluidicsdecontaminate: return "Fluidics Decontaminate";
				case audit_event_type.evt_fluidicsnightlyclean: return "Fluidics Nightly Clean";
				case audit_event_type.evt_fluidicspurge: return "Fluidics Purge";
				case audit_event_type.evt_reagentload: return "Reagent Pack Load";
				case audit_event_type.evt_reagentunload: return "Reagent Pack Unload";
				case audit_event_type.evt_reagentinvalid: return "Reagent Invalid";
				case audit_event_type.evt_reagentunusable: return "Reagent Unusable";
				case audit_event_type.evt_firmwareupdate: return "Firmware Update";
				case audit_event_type.evt_auditlogarchive: return "AuditLog Archive";
				case audit_event_type.evt_errorlogarchive: return "ErrorLog Archive";
				case audit_event_type.evt_samplelogarchive: return "SampleLog Archive";
				case audit_event_type.evt_datavalidationfailure: return "Data Validation Failure";
				case audit_event_type.evt_signaturedefinitionadd: return "Signature Definition Added";
				case audit_event_type.evt_signaturedefinitionremove: return "Signature Definition Removed";
				case audit_event_type.evt_concentrationinterceptset: return "Concentration Intercept Set";
				case audit_event_type.evt_concentrationinterceptnotset: return "Concentration Intercept Not Set";
				case audit_event_type.evt_concentrationslopeset: return "Concentration Slope Set";
				case audit_event_type.evt_concentrationslopenotset: return "Concentration Slope Not Set";
				case audit_event_type.evt_sizeinterceptset: return "Size Intercept Set";
				case audit_event_type.evt_sizeinterceptnotset: return "Size Intercept Not Set";
				case audit_event_type.evt_sizeslopeset: return "Size Slope Set";
				case audit_event_type.evt_sizeslopenotset: return "Size Slope Not Set";
				case audit_event_type.evt_notAuthorized: return "Unauthorized Access Attempt";
				case audit_event_type.evt_instrumentconfignotfound: return "Instrument Configuration Not Found";
				case audit_event_type.evt_instrumentconfigimported: return "Instrument Configuration Imported";
				case audit_event_type.evt_instrumentconfigexported: return "Instrument Configuration Exported";
				case audit_event_type.evt_autofocusaccepted: return "Auto Focus Accepted";
				case audit_event_type.evt_clearedexportdata: return "Cleared Export Data";
				case audit_event_type.evt_clearedcalibrationfactors: return "Cleared Slope and Intercept Offset Factors";
				case audit_event_type.evt_dustsubtractionaccepted: return "Dust Subtraction Accepted";
				case audit_event_type.evt_deletesamplerecord: return "Delete Sample Record";
				case audit_event_type.evt_deleteworklistrecord: return "Delete Worklist Record";
				case audit_event_type.evt_manualfocusoperation: return "Manual Focus Operation";
				case audit_event_type.evt_bioprocessactivate: return "Bioprocess Activate";
				case audit_event_type.evt_bioprocessdeactivate: return "Bioprocess Deactivate";
				case audit_event_type.evt_datasignatureapplied: return "Result Signed";
				case audit_event_type.evt_deleteresultrecord: return "Delete Result Record";
				case audit_event_type.evt_sampleprocessingerror: return "Sample Processing Error";
				case audit_event_type.evt_offlinemode: return "Offline Mode";
				case audit_event_type.evt_sampleresultcreated: return "Sample Result Created";
				case audit_event_type.evt_Instrumentdataexported: return "Instrument Data Exported";
				case audit_event_type.evt_setuserpasswordexpiration: return "Set User Password Expiration";
				case audit_event_type.evt_setuserinactivitytimeout: return "Sample User Inactivity Timeout";

				case audit_event_type.evt_setusercfg: return "Set User Configuration";
				case audit_event_type.evt_updatefailed: return "Firmware Update Failed";
				case audit_event_type.evt_fluidicsautomationnightlyclean: return "ACup Nightly Clean";
				case audit_event_type.evt_acupusingstandardconcentrationintercept: return "ACup Using Standard Concentration Slope";
				case audit_event_type.evt_serialnumber: return "Serial number ";
				case audit_event_type.evt_worklist: return "Worklist ";
				case audit_event_type.evt_automation: return "Automation ";
				case audit_event_type.evt_acup: return "A-Cup ";
                case audit_event_type.evt_qcontrolmodify: return "QualityControl Modification";
                case audit_event_type.evt_deletecampaigndata: return "Campaign Data Deleted";
                case audit_event_type.evt_flowcelldepthupdate: return "FlowCell Depth Update";
			}
			return evt.ToString();
		}

		public static object DateFormatConverter(object value, object parameter)
		{
			try
			{
				if (!(value is DateTime dt)) return null;

				if (!(parameter is string strParameter)) return Misc.ConvertToString(dt);
				if (strParameter.Equals("Min_NotRun"))
				{
					if ((dt.Equals(DateTime.MinValue) || dt.Equals(DateTime.MaxValue)))
					{
						// if the parameter is "Min_NotRun" and the date hasn't been set, then return "Not run"
						return LanguageResourceHelper.Get("LID_Status_NotRun");
					}
					return Misc.ConvertToCustomShortDateTimeFormat(dt);
				}

				var paramStr = parameter.ToString();
				if (paramStr.Equals("LongDate")) return Misc.ConvertToCustomLongDateTimeFormat(dt);
				if (paramStr.Equals("ShortDate")) return Misc.ConvertToCustomShortDateTimeFormat(dt);
				if (paramStr.Equals("DateOnly")) return Misc.ConvertToCustomDateOnlyFormat(dt);
				return Misc.ConvertToCustomDateFormat(dt, paramStr);
			}
			catch (Exception ex)
			{
				Log.Error("Invalid date time format", ex);
				return null;
			}
		}
    }
}
