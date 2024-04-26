using ApiProxies.Generic;
using log4net;
using ScoutDomains.Analysis;
using ScoutModels.Common;
using ScoutModels.Service;
using ScoutUtilities.Enums;
using System;
using System.Linq;
using ScoutLanguageResources;

namespace ScoutModels
{
    public class SelectCellTypeModel
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static CellTypeDomain GetReanalyzeSampleCellType(uint currentCellTypeIndex)
        {
            try
            {
                using (var tempOpticsManualServiceModel = new OpticsManualServiceModel())
                {
                    var setTempCellStatus = tempOpticsManualServiceModel.svc_SetTemporaryCellTypeFromExisting(currentCellTypeIndex);
                    if (setTempCellStatus.Equals(HawkeyeError.eSuccess))
                    {
                        var tempCell = CellTypeModel.svc_GetTemporaryCellType();
                        if (tempCell != null && tempCell.Any())
                        {
                            return tempCell.FirstOrDefault();
                        }
                    }
                    else
                    {
                        ApiHawkeyeMsgHelper.ErrorCommon(setTempCellStatus);
                        return null;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_REANALYZE_SAMPLE"));
                return null;
            }
        }
    }
}