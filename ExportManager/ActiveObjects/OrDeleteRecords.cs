using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

using System.Runtime.InteropServices;
using ScoutUtilities.Enums;
using ScoutUtilities.Delegate;
using ScoutUtilities.Structs;

using BAFW;

namespace ExportManager
{
    // ***********************************************************************
    public class OrDeleteRecords : BOrthoRegion
    {
        internal const UInt32 kORTHO_ID_MASK = 0x00000400;

        #region Private_Members
        private LdDataRecords _ldDataRecords = null;
        #endregion

        internal bool InReset { get; set; } = false;

        #region Construct_Destruct
        // ***********************************************************************
        public OrDeleteRecords(AoExportMgr parent, UInt32 orthoId, LdDataRecords ldDataRecords)
            : base(parent, orthoId)
        {
            SetRootState(ST_Root);
            _tmrDeleteTimeout = new BTimer(parent.GetEventQueue(), (UInt32)TimerIds.Timeout);
            _ldDataRecords = ldDataRecords;
            this.InitStateMachine();
        }
        #endregion

        #region Trace_Debug
        public static byte LogSubSysId = 0;
        private const string kMODULE_NAME = "OrDeleteRecords";
        // ***********************************************************************
        private void TraceThis(string strData)
        {
            EvAppLogReq.Publish(kMODULE_NAME, EvAppLogReq.LogLevel.Trace, strData, LogSubSysId);
        }
        private void DebugThis(string strData)
        {
            EvAppLogReq.Publish(kMODULE_NAME, EvAppLogReq.LogLevel.Debug, strData, LogSubSysId);
        }
        private void WarnThis(string strData)
        {
            EvAppLogReq.Publish(kMODULE_NAME, EvAppLogReq.LogLevel.Warning, strData, LogSubSysId);
        }
        private void ErrorThis(string strData)
        {
            EvAppLogReq.Publish(kMODULE_NAME, EvAppLogReq.LogLevel.Error, strData, LogSubSysId);
        }
        #endregion

        #region Timers
        private const int kRECORD_DELETE_TIMEOUT_SECS = 47;

        private BTimer _tmrDeleteTimeout;
        // ***********************************************************************
        private enum TimerIds : UInt32
        {
            Timeout = kORTHO_ID_MASK
        }
        #endregion

        #region Private_Events
        private enum PrivateEvIds : UInt32
        {
            iReset = kORTHO_ID_MASK
        }

        internal void ResetNow()
        {
            DeliverEvent(new BPrivateEvent((uint)PrivateEvIds.iReset));
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
                    TraceThis("ST_Root Init => ST_Reset");
                    SetState(ST_Reset);
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Private:
                {
                    if (ev.Id == (uint)PrivateEvIds.iReset)
                    {
                        TraceThis("ST_Root Private iReset Trans => ST_Reset");
                        DoTransition(ST_Reset);
                        return null;
                    }
                    break;
                }
                //...................................................................
                case BEvent.EvType.Public:
                case BEvent.EvType.Timer:
                case BEvent.EvType.None:
                {
                    break;
                }
            }
            return null;
        }

        private EvDeleteSamplesReq _currentRequest = null;
        private UInt32 _currentIndex = 0;
        // ******************************************************************
        //                    ST_Reset
        // ******************************************************************
        private State ST_Reset(BEvent ev)
        {
            switch (ev.MyType)
            {
                //...................................................................
                case BEvent.EvType.Entry:
                {
                    TraceThis("ST_Reset Entry");
                    InReset = true;
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Exit:
                {
                    TraceThis("ST_Reset Exit");
                    InReset = false;
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Public:
                {
                    switch (ev.Id)
                    {
                        case (uint)PubEvIds.SampleDataMgr_Delete:
                        {
                            _currentRequest = (EvDeleteSamplesReq)ev;                            
                            TraceThis("ST_Reset Public EvDeleteSamplesReq Trans => ST_Deleting");
                            DoTransition(ST_Deleting);
                            return null;
                        }
                    }
                    break;
                }
                //...................................................................
                case BEvent.EvType.Private:
                {
                    if (ev.Id == (uint)PrivateEvIds.iReset)
                    {
                        TraceThis("ST_Reset Private iReset Do nothing");
                        return null;
                    }
                    break;
                }
                //...................................................................
                case BEvent.EvType.Init:
                case BEvent.EvType.Timer:
                case BEvent.EvType.None:
                {
                    break;
                }
            }
            return ST_Root;
        }

        bool _canceled = false;
        // ******************************************************************
        //                    ST_Deleting
        // ******************************************************************
        private State ST_Deleting(BEvent ev)
        {
            switch (ev.MyType)
            {
                //...................................................................
                case BEvent.EvType.Entry:
                {
                    TraceThis("ST_Deleting Entry");
                    _currentIndex = 0;
                    _canceled = false;
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Exit:
                {
                    TraceThis("ST_Deleting Exit");
                    try
                    {
                        Owner.PostInternalEvent((uint)AoExportMgr.PrivateEvIds.iDeleteComplete);
                        if (_currentRequest?.ProgressCB != null)
                        {
                            _currentRequest.ProgressCB(HawkeyeError.eSoftwareFault, new uuidDLL(), 100);
                            _currentRequest.ProgressCB = null;
                        }
                    } catch(Exception e)
                    {
                        ErrorThis("ST_Deleting Exit - exception " + e.Message);
                    }
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Init:
                {
                    TraceThis("ST_Deleting Init => ST_DeleteRecord");
                    SetState(ST_DeleteRecord);
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Public:
                {
                    switch (ev.Id)
                    {
                        case (uint)PubEvIds.SampleDataMgr_CancelDelete:
                        {
                            TraceThis("ST_Deleting Public SampleDataMgr_CancelDelete set flag");
                            _canceled = true;
                            return null;
                        }
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
            return ST_Root;
        }

        private uint _retryCount = 0;
        // ******************************************************************
        //                    ST_DeleteRecord
        // ******************************************************************
        private State ST_DeleteRecord(BEvent ev)
        {
            switch (ev.MyType)
            {
                //...................................................................
                case BEvent.EvType.Entry:
                {
                    try
                    {
                        if (_currentIndex < _currentRequest.SampleIds.Count)
                        {
                            uuidDLL currId = _currentRequest.SampleIds[(int)_currentIndex];
                            TraceThis("ST_DeleteRecord Entry ID " + currId.ToString());
                            _ldDataRecords.RequestDeleteRecord(
                                _currentRequest.Username,
                                _currentRequest.Password,
                                currId,
                                _currentRequest.KeepFirstImage);
                            _tmrDeleteTimeout.FireInSecs(kRECORD_DELETE_TIMEOUT_SECS);
                        }
                        else
                        {
                            ErrorThis("ST_DeleteRecord Entry invalid _currentIndex " + _currentIndex.ToString());
                            _tmrDeleteTimeout.FireInSecs(kRECORD_DELETE_TIMEOUT_SECS / 10);
                        }
                        _retryCount = 0;
                    }
                    catch (Exception e)
                    {
                        ErrorThis("ST_DeleteRecord Entry exception " + e.Message);
                    }
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Exit:
                {
                    TraceThis("ST_DeleteRecord Exit");
                    _tmrDeleteTimeout.Disarm();
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Private:
                {
                    switch (ev.Id)
                    {
                        case (uint)AoExportMgr.PrivateEvIds.pDelete_Status:
                        {
                            if (_canceled)
                            {
                                TraceThis("ST_DeleteRecord Private pDelete_Status canceled set Trans => ST_Reset");
                                DoTransition(ST_Reset);
                                return null;
                            }
                            _tmrDeleteTimeout.Disarm();
                            AoExportMgr.DeleteRecordStatusIndEv stev = (AoExportMgr.DeleteRecordStatusIndEv)ev;
                            if (stev.Status == HawkeyeError.eSuccess)
                            {
                                TraceThis("ST_DeleteRecord Private pDelete_Status eSuccess index: " + _currentIndex.ToString());
                                if (_currentRequest.ProgressCB != null)
                                {
                                    if ((_currentRequest.SampleIds.Count > 0) && (_currentIndex < _currentRequest.SampleIds.Count))
                                    {
                                        try
                                        {
                                            int pcnt = (int)(((_currentIndex + 1) * 100.0) / (_currentRequest.SampleIds.Count));
                                            _currentRequest.ProgressCB(HawkeyeError.eSuccess, _currentRequest.SampleIds[(int)_currentIndex], pcnt);
                                        }
                                        catch (Exception e)
                                        {
                                            ErrorThis("ST_DeleteRecord ProgressCB index " + _currentIndex.ToString() + " exception " + e.Message);
                                        }

                                    }
                                    else
                                    {
                                        try
                                        {
                                            _currentRequest.ProgressCB(HawkeyeError.eSuccess, new uuidDLL(), 100);
                                        }
                                        catch (Exception e)
                                        {
                                            ErrorThis("ST_DeleteRecord ProgressCB (done) exception " + e.Message);
                                        }

                                    }
                                }

                                _currentIndex++;

                                if (_currentIndex < _currentRequest.SampleIds.Count)
                                {
                                    try
                                    {
                                        _retryCount = 0;
                                        TraceThis("ST_DeleteRecord request delete: " + _currentRequest.SampleIds[(int)_currentIndex].ToString());
                                        _ldDataRecords.RequestDeleteRecord(
                                            _currentRequest.Username,
                                            _currentRequest.Password,
                                            _currentRequest.SampleIds[(int)_currentIndex],
                                            _currentRequest.KeepFirstImage);

                                        _tmrDeleteTimeout.FireInSecs(kRECORD_DELETE_TIMEOUT_SECS, OrthoId);
                                        return null;
                                    }
                                    catch (Exception e)
                                    {
                                        ErrorThis("ST_DeleteRecord delete next exception " + e.Message);
                                    }
                                }
                                TraceThis("ST_DeleteRecord Private pDelete_Status Done Trans => ST_Reset");
                                _currentRequest.ProgressCB = null;
                                DoTransition(ST_Reset);
                                return null;
                            }
                            try
                            {
                                if (_retryCount++ < 2)
                                {
                                    TraceThis("ST_DeleteRecord Private pDelete_Status: " + stev.Status.ToString() + " retry count: " + _retryCount.ToString());
                                    BAppFW.Delay(30);
                                    _ldDataRecords.RequestDeleteRecord(
                                        _currentRequest.Username,
                                        _currentRequest.Password,
                                        _currentRequest.SampleIds[(int)_currentIndex],
                                        _currentRequest.KeepFirstImage);
                                    _tmrDeleteTimeout.FireInSecs(kRECORD_DELETE_TIMEOUT_SECS, OrthoId);
                                    return null;
                                }
                                TraceThis("ST_DeleteRecord Private pDelete_Status: " + stev.Status.ToString() + " Trans => ST_Reset");
                                if (_currentRequest.ProgressCB != null)
                                {
                                    try
                                    {
                                        _currentRequest.ProgressCB(stev.Status, new uuidDLL(), 100);
                                    }
                                    catch (Exception e)
                                    {
                                        ErrorThis("ST_DeleteRecord ProgressCB(failed) exception " + e.Message);
                                    }
                                    _currentRequest.ProgressCB = null;
                                }
                            }
                            catch (Exception e)
                            {
                                ErrorThis("ST_DeleteRecord retry exception " + e.Message);
                            }

                            DoTransition(ST_Reset);
                            return null;
                        }
                    }
                    break;
                }
                //...................................................................
                case BEvent.EvType.Timer:
                {
                    if (ev.Id == (uint)TimerIds.Timeout)
                    {
                        TraceThis("ST_DeleteRecord Timer - Timeout Trans => ST_Reset");
                        if (_currentRequest?.ProgressCB != null)
                        {
                            try
                            {
                                _currentRequest.ProgressCB(HawkeyeError.eTimedout, new uuidDLL(), 100);
                            }
                            catch (Exception e)
                            {
                                ErrorThis("ST_DeleteRecord ProgressCB(timeout) exception " + e.Message);
                            }
                            _currentRequest.ProgressCB = null;
                        }                        
                        DoTransition(ST_Reset);
                        return null;
                    }
                    break;
                }
                //...................................................................
                case BEvent.EvType.Init:
                case BEvent.EvType.Public:
                case BEvent.EvType.None:
                {
                    break;
                }
            }
            return ST_Deleting;
        }
        #endregion
    }
}
