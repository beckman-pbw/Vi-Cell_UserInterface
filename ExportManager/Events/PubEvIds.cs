using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExportManager
{
    public enum PubEvIds
    {
        None = 0,
        //
        SysReset,
        SystemStatus,
        //
        //
        ExportMgr_CancelOffline,
        ExportMgr_StartOffline,
        //
        ExportMgr_StartCsv,
        ExportMgr_CancelCsv,
        //
        ExportMgr_StartLogs,
        //
		ExportMgr_StartLogDataExport,
		//
        ScheduledExportChanged,
        //
        SampleDataMgr_Delete,
        SampleDataMgr_CancelDelete,
        //
        AppLog_AddEntry,
        AppLog_SetLevel,
        AppLog_LevelReq,
        AppLog_LevelResp

    };
}
