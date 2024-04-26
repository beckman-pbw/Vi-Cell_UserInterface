using HawkeyeCoreAPI.Facade;
using JetBrains.Annotations;
using log4net;
using ScoutDomains;
using ScoutDomains.Reports.ScheduledExports;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace HawkeyeCoreAPI
{
    public class ScheduledExportApi
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region API_Declarations

        [DllImport(ApplicationConstants.DllName)]
        [MustUseReturnValue(ApplicationConstants.Justification)]
        static extern HawkeyeError AddScheduledExport(ScheduledExport scheduledExport, out uuidDLL exportUuid);

        [DllImport(ApplicationConstants.DllName)]
        [MustUseReturnValue(ApplicationConstants.Justification)]
        static extern HawkeyeError EditScheduledExport(ScheduledExport scheduledExport);

        [DllImport(ApplicationConstants.DllName)]
        [MustUseReturnValue(ApplicationConstants.Justification)]
        static extern HawkeyeError DeleteScheduledExport(uuidDLL uuid);

        [DllImport(ApplicationConstants.DllName)]
        [MustUseReturnValue(ApplicationConstants.Justification)]
        static extern HawkeyeError GetScheduledExports(ScheduledExportType exportType, 
            out IntPtr ptrScheduledExports, out uint count);

        [DllImport(ApplicationConstants.DllName)]
        static extern void FreeListOfScheduledExports(IntPtr ptrScheduledExports, uint count);

        #endregion

        #region API_Calls

        [MustUseReturnValue(ApplicationConstants.Justification)]
        public static HawkeyeError AddScheduledExportApi(SampleResultsScheduledExportDomain domain)
        {
            try
            {
                var export = domain.GetScheduledExport();
                var hawkeyeError = AddScheduledExport(export, out var uuid);
                Log.Debug($"SampleResultsScheduledExport::{nameof(AddScheduledExport)} result: '{hawkeyeError}', UUID: {uuid}");
                domain.Uuid = uuid;

                return hawkeyeError;
            }
            catch (Exception e)
            {
                Log.Error($"SampleResultsScheduledExport::Failed to execute '{nameof(AddScheduledExportApi)}'", e);
                return HawkeyeError.eInvalidArgs;
            }
            
        }

        [MustUseReturnValue(ApplicationConstants.Justification)]
        public static HawkeyeError AddScheduledExportApi(AuditLogScheduledExportDomain domain)
        {
            try
            {
                var export = domain.GetScheduledExport();
                var hawkeyeError = AddScheduledExport(export, out var uuid);
                Log.Debug($"AuditLogScheduledExport::{nameof(AddScheduledExport)} result: '{hawkeyeError}', UUID: {uuid}");
                domain.Uuid = uuid;

                return hawkeyeError;
            }
            catch (Exception e)
            {
                Log.Error($"AuditLogScheduledExport::Failed to execute '{nameof(AddScheduledExportApi)}'", e);
                return HawkeyeError.eInvalidArgs;
            }
        }

        [MustUseReturnValue(ApplicationConstants.Justification)]
        public static HawkeyeError EditScheduledExportApi(SampleResultsScheduledExportDomain domain)
        {
            try
            {
                var export = domain.GetScheduledExport();
                var hawkeyeError = EditScheduledExport(export);
                Log.Debug($"SampleResultsScheduledExport::{nameof(EditScheduledExport)} result: '{hawkeyeError}'");
            
                return hawkeyeError;
            }
            catch (Exception e)
            {
                Log.Error($"SampleResultsScheduledExport::Failed to execute '{nameof(EditScheduledExportApi)}'", e);
                return HawkeyeError.eInvalidArgs;
            }
        }

        [MustUseReturnValue(ApplicationConstants.Justification)]
        public static HawkeyeError EditScheduledExportApi(AuditLogScheduledExportDomain domain)
        {
            try
            {
                var export = domain.GetScheduledExport();
                var hawkeyeError = EditScheduledExport(export);
                Log.Debug($"AuditLogScheduledExport::{nameof(EditScheduledExport)} result: '{hawkeyeError}'");
            
                return hawkeyeError;
            }
            catch (Exception e)
            {
                Log.Error($"AuditLogScheduledExport::Failed to execute '{nameof(EditScheduledExportApi)}'", e);
                return HawkeyeError.eInvalidArgs;
            }
        }
        
        [MustUseReturnValue(ApplicationConstants.Justification)]
        public static HawkeyeError DeleteScheduledExportApi(uuidDLL uuid)
        {
            try
            {
                var hawkeyeError = DeleteScheduledExport(uuid);
                Log.Debug($"{nameof(DeleteScheduledExport)} result: '{hawkeyeError}'");
            
                return hawkeyeError;
            }
            catch (Exception e)
            {
                Log.Error($"Failed to execute '{nameof(DeleteScheduledExportApi)}'", e);
                return HawkeyeError.eInvalidArgs;
            }
        }
        
        [MustUseReturnValue(ApplicationConstants.Justification)]
        public static HawkeyeError GetScheduledExportsApi(out List<SampleResultsScheduledExportDomain> scheduledExportDomains)
        {
            var ptrScheduledExports = IntPtr.Zero;
            var count = (uint) 0;
            try
            {
                var hawkeyeError = GetScheduledExports(ScheduledExportType.SampleResults,
                    out ptrScheduledExports, out count);

                if (hawkeyeError != HawkeyeError.eSuccess)
                {
                    Log.Debug($"SampleResultsScheduledExport::{nameof(GetScheduledExports)} result: '{hawkeyeError}'");
                    scheduledExportDomains = new List<SampleResultsScheduledExportDomain>();
                    return hawkeyeError;
                }
                var ctQcList = CellTypeFacade.Instance.GetAllCtQcGroupList_BECall();
                scheduledExportDomains = ptrScheduledExports.MarshalToScheduledExportDomainList(count, ctQcList);
                return HawkeyeError.eSuccess;
            }
            catch (Exception e)
            {
                Log.Error($"SampleResultsScheduledExport::Failed to execute '{nameof(GetScheduledExportsApi)}'", e);
                scheduledExportDomains = new List<SampleResultsScheduledExportDomain>();
                return HawkeyeError.eInvalidArgs;
            }
            finally
            {
                try
                {
                    FreeListOfScheduledExports(ptrScheduledExports, count);
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to FreeListOfScheduledExports", e);
                }
            }
        }
        
        [MustUseReturnValue(ApplicationConstants.Justification)]
        public static HawkeyeError GetAuditLogScheduledExportsApi(out List<AuditLogScheduledExportDomain> scheduledExportDomains)
        {
            var ptrScheduledExports = IntPtr.Zero;
            var count = (uint) 0;

            try
            {
                var hawkeyeError = GetScheduledExports(ScheduledExportType.AuditLogs,
                    out ptrScheduledExports, out count);

                if (hawkeyeError != HawkeyeError.eSuccess)
                {
                    Log.Debug($"AuditLogScheduled::{nameof(GetScheduledExports)} result: '{hawkeyeError}'");
                    scheduledExportDomains = new List<AuditLogScheduledExportDomain>();
                    return hawkeyeError;
                }
                scheduledExportDomains = ptrScheduledExports.MarshalToAuditLogScheduledExportDomainList(count);
                return hawkeyeError;
            }
            catch (Exception e)
            {
                Log.Error($"AuditLogScheduled::Failed to execute '{nameof(GetAuditLogScheduledExportsApi)}'", e);
                scheduledExportDomains = new List<AuditLogScheduledExportDomain>();
                return HawkeyeError.eInvalidArgs;
            }
            finally
            {
                try
                {
                    FreeListOfScheduledExports(ptrScheduledExports, count);
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to FreeListOfScheduledExports", e);
                }
            }
        }
        
#endregion
    }
}