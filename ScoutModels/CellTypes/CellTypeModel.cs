using HawkeyeCoreAPI;
using ScoutDomains.Analysis;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScoutModels
{
    public class CellTypeModel : BaseNotifyPropertyChanged
    {
        public static List<eCellDeclusterSetting> GetDeclusterList()
        {
            return Enum.GetValues(typeof(eCellDeclusterSetting)).Cast<eCellDeclusterSetting>().Select(r => r).ToList();
        }

        /// <summary>
        /// ToDo: Should be moved to and wrapped in user SettingsModel
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cellTypeIndices"></param>
        /// <returns></returns>
        public static HawkeyeError SetUserCellTypeIndices(string userId, List<UInt32> cellTypeIndices)
        {
            Log.Debug("SetUserCellTypeIndices:: userId: " + userId + ", cellTypeIndices size: " + cellTypeIndices.Count);
            cellTypeIndices.ForEach(cell => { Log.Info("Cell type Index" + cell); });
            var hawkeyeError = CellType.SetUserCellTypeIndicesAPI(userId, cellTypeIndices);
            Misc.LogOnHawkeyeError("SetUserCellTypeIndices::", hawkeyeError);
            return hawkeyeError;
        }


        public static HawkeyeError svc_SetTemporaryCellType(CellTypeDomain cellDomain)
        {
            var temp_cellType = cellDomain.PopulateNewCellType();
            var hawkeyeError = CellType.SetTemporaryCellTypeAPI(temp_cellType);
            temp_cellType.label.ReleaseIntPtr();
            Misc.LogOnHawkeyeError("svc_SetTemporaryCellType::", hawkeyeError);
            return hawkeyeError;
        }

        public static List<CellTypeDomain> svc_GetTemporaryCellType()
        {
            var cellTypeList = new List<CellTypeDomain>();
            var hawkeyeError = CellType.GetTemporaryCellTypeAPI(ref cellTypeList);
            Misc.LogOnHawkeyeError("svc_GetTemporaryCellType::", hawkeyeError);
            return cellTypeList;
        }

        public static HawkeyeError svc_SetTemporaryAnalysisDefinition(AnalysisDomain appType)
        {
            var appStr = appType.GetAnalysisDefinition();
            var hawkeyeError = Analysis.SetTemporaryAnalysisDefinitionAPI(appStr);
            Misc.LogOnHawkeyeError("svc_SetTemporaryAnalysisDefinition::", hawkeyeError);
            return hawkeyeError;
        }

        public static HawkeyeError SpecializeAnalysisForCellType(AnalysisDomain ad, uint ctIndex)
        {
            var analysizDefinition = ad.GetAnalysisDefinition();
            var hawkeyeError = Analysis.SpecializeAnalysisForCellTypeAPI(analysizDefinition, ctIndex);
            Misc.LogOnHawkeyeError("SpecializeAnalysisForCellType::", hawkeyeError);
            return hawkeyeError;
        }
    }
}