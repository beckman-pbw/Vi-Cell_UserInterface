using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

using BAFW;

namespace ExportManager
{
    public class AoLogger : BActive
    {

        #region Member_Variables
        // The sub-system id is a BYTE handle all 256 possible sub-systems
        private const int kNUM_SUB_SYSTEMS = 256;

        private EvAppLogReq.LogLevel[] _logLevels = null;
        private LdLogger _ldAppLog = null;
        private LdLogger _ldErrorLog = null;
        #endregion

        #region Construct_Destruct
        // ******************************************************************
        public AoLogger(string appLog, string errLog)
            : base()
        {
            SetRootState(ST_Root);

            BAppFW.Subscribe(this, new EvAppLogReq("", EvAppLogReq.LogLevel.Critical, "", 0));
            BAppFW.Subscribe(this, new EvSetAppLogLevel(0, EvAppLogReq.LogLevel.Critical));
            BAppFW.Subscribe(this, new BPublicEvent((uint)PubEvIds.AppLog_LevelReq, 0));

            _logLevels = new EvAppLogReq.LogLevel[kNUM_SUB_SYSTEMS];
            for (int j = 0; j < _logLevels.Length; j++)
                _logLevels[j] = EvAppLogReq.LogLevel.Trace;

            _ldAppLog = new LdLogger(appLog);
            _ldErrorLog = new LdLogger(errLog);

            StartThread();
        }

        // ******************************************************************
        ~AoLogger()
        {
            Shutdown();
        }

        // ******************************************************************
        public override void Shutdown()
        {
            try
            {
                BAppFW.UnsubscribeAll(this);
                if (_ldAppLog != null)
                {
                    _ldAppLog.Shutdown();
                    _ldAppLog = null;
                }
                if (_ldErrorLog != null)
                {
                    _ldErrorLog.Shutdown();
                    _ldErrorLog = null;
                }
            }
            catch
            {
                //Do nothing 
            }
            base.Shutdown();
        }

        #endregion

        #region Trace_Debug
        // ******************************************************************
        private void TraceThis(string strData)
        {
            if (_ldAppLog != null)
            {
                _ldAppLog.AddEntry("AoLogger: " + strData);
            }
        }
        #endregion

        #region State_Machine
        // ******************************************************************
        //                    ST_Root
        // ******************************************************************
        private State ST_Root(BEvent ev)
        {
            switch (ev.MyType)
            {
                //...................................................................
                case BEvent.EvType.Entry:
                {
                    TraceThis("ST_Root Entry");
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Exit:
                {
                    TraceThis("ST_Root Exit");
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Init:
                {
                    TraceThis("ST_Root Init => ST_Idle");
                    SetState(ST_Idle);
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Public:
                {
                    if (ev.Id == (UInt32)PubEvIds.SysReset)
                    {
                        TraceThis("ST_Root Public SysReset Trans => ST_Root");
                        DoTransition(ST_Root);
                        return null;
                    }
                    break;
                }
                //...................................................................
                case BEvent.EvType.Private:
                case BEvent.EvType.Timer:
                case BEvent.EvType.None:
                {
                    break;
                }
            }
            return null;
        }

        // ******************************************************************
        //                    ST_Idle
        // ******************************************************************
        private State ST_Idle(BEvent ev)
        {
            switch (ev.MyType)
            {
                //...................................................................
                case BEvent.EvType.Entry:
                {
                    TraceThis("ST_Idle Entry");
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Exit:
                {
                    TraceThis("ST_Idle Exit");
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Public:
                {
                    switch (ev.Id)
                    {
                        case (uint)PubEvIds.AppLog_LevelReq:
                        {
                            BPublicEvent pubEv = (BPublicEvent)ev;
                            byte subSys = (byte)pubEv.AppData;
                            BPublicEvent respEv = new BPublicEvent((uint)PubEvIds.AppLog_LevelResp, (uint)_logLevels[subSys]);
                            return null;
                        }
                        case (uint)PubEvIds.AppLog_SetLevel:
                        {
                            EvSetAppLogLevel setEv = (EvSetAppLogLevel)ev;
                            string strData = "AoLogger AppLog_SetLevel Subsys : Level " + setEv.SubSys.ToString() + " : " + setEv.Level.ToString();
                            if (_ldAppLog != null)
                            {
                                _ldAppLog.AddEntry(strData);
                            }
                            _logLevels[setEv.SubSys] = setEv.Level;
                            return null;
                        }
                        case (uint)PubEvIds.AppLog_AddEntry:
                        {
                            EvAppLogReq logEv = (EvAppLogReq)ev;
                            // All errors go into the error log regardless of the log level
                            if ((logEv.Level == EvAppLogReq.LogLevel.Critical) ||
                                (logEv.Level == EvAppLogReq.LogLevel.Error))
                            {
                                if (_ldErrorLog != null)
                                {
                                    _ldErrorLog.AddEntry(logEv.GetLogEntry());
                                }
                            }
                            // Log requests at or above the current set level are logged
                            if (logEv.Level >= _logLevels[logEv.SubSys])
                            {
                                if (_ldAppLog != null)
                                {
                                    _ldAppLog.AddEntry(logEv.GetLogEntry());
                                }
                            }
                            return null;
                        }
                    }
                    break;
                }
                case BEvent.EvType.Init:
                case BEvent.EvType.Private:
                case BEvent.EvType.Timer:
                case BEvent.EvType.None:
                    break;
            }
            return ST_Root;
        }
        #endregion
    }
}
