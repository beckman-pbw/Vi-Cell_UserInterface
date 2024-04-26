using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BAFW;

namespace ExportManager
{
    // ***********************************************************************
    public class EvAppLogReq : BPublicEvent
    {
        public enum LogLevel
        {
            InvlidLevel = 0,
            Trace,
            Debug,
            Information,
            Warning,
            Error,
            Critical            
        }
        public EvAppLogReq(string moduleName, LogLevel level, string logStr, byte subSys = 0) :
            base((uint)PubEvIds.AppLog_AddEntry)
        {
            ModuleName = moduleName;
            Level = level;
            LogStr = logStr;
            SubSys = subSys;
        }
        public string ModuleName { get; set; } = "";
        public string LogStr { get; set; } = "";
        public EvAppLogReq.LogLevel Level { get; set; } = EvAppLogReq.LogLevel.InvlidLevel;

        public byte SubSys { get; set; } = 0;

        public string GetLogEntry()
        {
            var now = DateTime.Now;
            string outstr = "";
            outstr += now.ToString("MM/dd/yyyy,hh:mm:ss.fff");
            outstr += ", " + SubSys.ToString();
            outstr += ", " + Level.ToString() + ", " + ModuleName + ", " + LogStr;
            return outstr;
        }

        public static void Publish(string moduleName, LogLevel level, string logStr, byte subSys = 0)
        {
            BAppFW.Publish(new EvAppLogReq(moduleName, level, logStr, subSys));
        }
    }

    // ***********************************************************************
    public class EvSetAppLogLevel : BPublicEvent
    {
        public EvSetAppLogLevel(byte subSys, EvAppLogReq.LogLevel level) :
            base((uint)PubEvIds.AppLog_SetLevel)
        {
            SubSys = subSys;
            Level = level;
        }
        public byte SubSys { get; set; } = 0;
        public EvAppLogReq.LogLevel Level { get; set; } = EvAppLogReq.LogLevel.InvlidLevel;

        public static void Publish(byte subSys, EvAppLogReq.LogLevel level) { BAppFW.Publish(new EvSetAppLogLevel(subSys, level)); }

    }
}
