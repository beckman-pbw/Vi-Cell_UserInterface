using JetBrains.Annotations;
using log4net;
using ScoutUtilities;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using ScoutUtilities.Common;

namespace ScoutModels.Service
{
    public class LowLevelModel
    {
        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void svc_GetProbePosition()
        {
            Int32 pos = 0;
            var hawkeyeError = HawkeyeCoreAPI.Service.GetProbePositionAPI(out pos);
            Log.Debug("svc_GetProbePosition() hawkeyeError: " + hawkeyeError);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError svc_SetProbePosition(bool upDown, uint stepsToMove)
        {
            Log.Debug("svc_SetProbePosition:: upDown: " + upDown + ", stepsToMove: " + stepsToMove);
            var hawkeyeError = HawkeyeCoreAPI.Service.SetProbePositionAPI(upDown, stepsToMove);
            Misc.LogOnHawkeyeError("svc_SetProbePosition::", hawkeyeError);
            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError svc_AspirateSample(uint volume)
        {
            Log.Debug("svc_AspirateSample:: volume: " + volume);
            var hawkeyeError = HawkeyeCoreAPI.Service.AspirateSampleAPI(volume);
            Misc.LogOnHawkeyeError("svc_AspirateSample::", hawkeyeError);
            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError svc_DispenseSample(uint volume)
        {
            Log.Debug("svc_DispenseSample:: volume: " + volume);
            var hawkeyeError = HawkeyeCoreAPI.Service.DispenseSampleAPI(volume);
            Misc.LogOnHawkeyeError("svc_DispenseSample::", hawkeyeError);
            return hawkeyeError;
        }

        public static string svc_GetValvePort()
        {
            char pos = '\'';
            HawkeyeCoreAPI.Service.GetValvePortAPI(out pos);
            return pos.ToString();
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError SetValvePort(char port)
        {
            Log.Debug("SetValvePort:: port: " + port);
            var hawkeyeError = HawkeyeCoreAPI.Service.SetValvePortAPI(port);
            Misc.LogOnHawkeyeError("SetValvePort::", hawkeyeError);
            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError svc_MoveProbe(bool upDown)
        {
            Log.Debug("svc_MoveProbe:: upDown: " + upDown);
            var hawkeyeError = HawkeyeCoreAPI.Service.MoveProbeAPI(upDown);
            Misc.LogOnHawkeyeError("svc_MoveProbe::", hawkeyeError);
            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError MoveReagentArm(bool upDown)
        {
            Log.Debug("svc_MoveReagentArm:: upDown: " + upDown);
            var hawkeyeError = HawkeyeCoreAPI.Service.MoveReagentArmAPI(upDown);
            Misc.LogOnHawkeyeError("svc_MoveReagentArm::", hawkeyeError);
            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError InitializeSampleDeck()
        {
            var hawkeyeError = HawkeyeCoreAPI.Service.InitializeCarrierAPI();
            Misc.LogOnHawkeyeError("InitializeSampleDeck::", hawkeyeError);
            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError svc_SetSampleWellPosition(char row, uint pos)
        {
            var smaplePos = new SamplePosition(row, pos);
            Log.Debug("svc_SetSampleWellPosition:: row: " + row + ", pos: " + pos);
            var hawkeyeError = HawkeyeCoreAPI.Service.SetSampleWellPositionAPI(smaplePos);
            Misc.LogOnHawkeyeError("svc_SetSampleWellPosition::", hawkeyeError);
            return hawkeyeError;
        }
    }
}
