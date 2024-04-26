using System;
using log4net;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutUtilities;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HawkeyeCoreAPI.Interfaces;

namespace HawkeyeCoreAPI.Facade
{
    public class CellTypeFacade
    {
        #region Singleton Stuff

        private static readonly object _instanceLock = new object();
        private static CellTypeFacade _instance;
        public static CellTypeFacade Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_instanceLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new CellTypeFacade();
                            _instance._cellTypeAccess = new CellType();
                            _instance._qualityControlAccess = new QualityControl();
                        }
                    }
                }

                return _instance;
            }
        }

        private CellTypeFacade()
        {
        }

        #endregion

        #region Properties & Fields
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private ICellType _cellTypeAccess;
        private IQualityControl _qualityControlAccess;
        #endregion

        #region Public Methods

        public void UnitTest_SetCellTypeAccess(ICellType cellTypeAccess, IQualityControl qualityControlAccess)
        {
            _cellTypeAccess = cellTypeAccess;
            _qualityControlAccess = qualityControlAccess;
        }

        #region Cell Type Methods

        public List<CellTypeDomain> GetFactoryCellTypes_BECall()
        {
            var allCts = GetAllCellTypes_BECall();
            return allCts.Where(c => !c.IsUserDefineCellType).ToList();
        }

        public CellTypeDomain GetCellTypeCopy_BECall(uint cellTypeIndex)
        {
            var allCts = GetAllCellTypes_BECall();
            var foundCt = allCts.FirstOrDefault(c => c.CellTypeIndex == cellTypeIndex);
            return (CellTypeDomain)foundCt?.Clone();
        }

        public CellTypeDomain GetCellTypeCopy_BECall(string cellTypeName)
        {
            var allCts = GetAllCellTypes_BECall();
            var foundCt = allCts.FirstOrDefault(c => c.CellTypeName == cellTypeName);
            return (CellTypeDomain)foundCt?.Clone();
        }

        /// <summary>
        /// Save the new user defined cell type to the database.
        /// </summary>
        /// <param name="cellType">A new, already cloned cell type. Will not be copied again.</param>
        /// <returns>HawkeyeError.eSuccess if successful, or an error code.</returns>
        public HawkeyeError Add(string username, string password, CellTypeDomain cellType, string retiredCellTypeName)
        {
            cellType.IsUserDefineCellType = true;
            cellType.TempCellName = cellType.TempCellName.Trim();
            cellType.CellTypeName = cellType.CellTypeName.Trim();
            retiredCellTypeName = retiredCellTypeName.Trim();

			var result = AddCellType(username, password, cellType, retiredCellTypeName);
            if (result != HawkeyeError.eSuccess) return result;
            MessageBus.Default.Publish(new Notification(MessageToken.CellTypesUpdated));
            return result;
        }

        public HawkeyeError Remove(string username, string password, CellTypeDomain cellType)
        {
            var result = RemoveCellType(username, password, cellType.CellTypeIndex);
            if (result != HawkeyeError.eSuccess) return result;
            MessageBus.Default.Publish(new Notification(MessageToken.CellTypesUpdated));
            return result;
        }

        public HawkeyeError Edit(string username, string password, CellTypeDomain cellTypeDomain)
        {
            var result = ModifyCellType(username, password, cellTypeDomain);
            if (result != HawkeyeError.eSuccess) return result;
            MessageBus.Default.Publish(new Notification(MessageToken.CellTypesUpdated));
            return result;
        }
        #endregion

        #region Quality Control Methods

        public HawkeyeError AddQc(string username, string password, QualityControlDomain qc, string retiredQCName)
        {
            qc.QcName = qc.QcName.Trim();
            qc.LotInformation = qc.LotInformation.Trim();
            qc.CommentText = qc.CommentText != null ? qc.CommentText.Trim() : string.Empty;
            var result = AddQualityControl(username, password, qc, retiredQCName);
            if (result != HawkeyeError.eSuccess) return result;

            MessageBus.Default.Publish(new Notification(MessageToken.QualityControlsUpdated));
            return result;
        }
        #endregion

        #region ICellTypeQualityControlGroupDomain Methods

        public void GetAllowedCtQc(
            string username,
            ref List<CellTypeDomain> ctList,
            ref List<QualityControlDomain> qcList,
            ref List<CellTypeQualityControlGroupDomain> bpqcGroupList)
        {
            ctList = CellTypeFacade.Instance.GetAllowedCellTypes_BECall(username);
            qcList = CellTypeFacade.Instance.GetAllowedQualityControls_BECall(username, ctList);

            qcList?.RemoveAll(x => !x.NotExpired);

            bpqcGroupList.Clear();
            bpqcGroupList.Add(GetCellTypeGroup(ctList));
            if (qcList.Count > 0)
                bpqcGroupList.Add(GetQualityControlGroup(ctList, qcList));
            return;
        }

        public void GetAllQcs(ref List<QualityControlDomain> qcList)
        {
	        var result = _qualityControlAccess.GetQualityControlListAPI("", "", true, ref qcList, out var numberOfQcs);
			if (result != HawkeyeError.eSuccess)
	        {
		        Log.Debug($"No QCs found");
	        }
        }

        public List<CellTypeQualityControlGroupDomain> GetAllCtQcGroupList_BECall()
        {
            var allQcs = GetAllQualityControls_BECall(out var allCts);
            var bpqcGroupList = new List<CellTypeQualityControlGroupDomain>();
            bpqcGroupList.Add(GetCellTypeGroup(allCts));
            var qcGrp = GetQualityControlGroup(allCts, allQcs);
            bpqcGroupList.Add(qcGrp);
            return bpqcGroupList;
        }

        public CellTypeQualityControlGroupDomain GetCellTypeGroup(List<CellTypeDomain> cellList)
        {
            var cellTypeList = new CellTypeQualityControlGroupDomain();
            cellList?.ForEach(cell =>
            {
                var cellType = new CellTypeQualityControlGroupDomain
                {
                    Name = cell.CellTypeName,
                    CellTypeIndex = (uint)cell.CellTypeIndex,
                    AppTypeIndex = cell.AnalysisDomain.AnalysisIndex,
                    SelectedCtBpQcType = CtBpQcType.CellType,
                    HasValueCount = true
                };
                cellTypeList.CellTypeQualityControlChildItems.Add(cellType);
                cellTypeList.HasValueCount = true;
                cellTypeList.HasValue = true;
                cellTypeList.SelectedCtBpQcType = CtBpQcType.CellType;
                cellTypeList.IsSelectionActive = false;
                cellTypeList.Name = ScoutLanguageResources.LanguageResourceHelper.Get("LID_Icon_CellType");
            });

            return cellTypeList;
        }

        public CellTypeQualityControlGroupDomain GetQualityControlGroup(List<CellTypeDomain> allCellTypes, List<QualityControlDomain> qualityControlList)
        {
            var qcList = new CellTypeQualityControlGroupDomain();
            qcList.SelectedCtBpQcType = CtBpQcType.QualityControl;
            qcList.Name = ScoutLanguageResources.LanguageResourceHelper.Get("LID_MSGBOX_QualityControl");

            foreach (var qc in qualityControlList)
            {
                var cellType = allCellTypes.Find(a => a.CellTypeIndex == qc.CellTypeIndex);
                if (cellType == null)
                    continue;

                var bpQc = new CellTypeQualityControlGroupDomain
                {
                    Name = Misc.GetParenthesisQualityControlName(qc.QcName, cellType.CellTypeName),
                    KeyName = qc.QcName,
                    SelectedCtBpQcType = CtBpQcType.QualityControl,
                    CellTypeIndex = (uint)cellType.CellTypeIndex,
                    AppTypeIndex = cellType.AnalysisDomain.AnalysisIndex,
                    HasValueCount = true
                };

                qcList.CellTypeQualityControlChildItems.Add(bpQc);
                qcList.HasValueCount = true;
                qcList.HasValue = true;
                qcList.IsSelectionActive = false;
            }

            return qcList;
        }

        #endregion

        #endregion

        #region Private Methods

        private static void UpdateQcCtInfo(
            ref List<QualityControlDomain> qcList, 
            ref List<CellTypeDomain> cellTypeDomains)
        {
            foreach (var qc in qcList)
            {
                var cellType = cellTypeDomains.FirstOrDefault(x => x.CellTypeIndex == qc.CellTypeIndex);
                if (cellType != null)
                {
                    cellType.IsQualityControlCellType = true;
                    qc.CellTypeName = cellType.CellTypeName;
                }
            }
        }

        #endregion

        #region Private API calling Methods

        #region Cell Type API Calls

        private HawkeyeError AddCellType(string username, string password, CellTypeDomain cellTypeDomain, string retiredCellTypeName)
        {
            var cellType = cellTypeDomain.PopulateNewCellType();
            var hawkeyeError = _cellTypeAccess.AddCellTypeAPI(username, password, cellType, retiredCellTypeName, ref cellType.celltype_index);
            Log.Debug("celltype_index: " + cellType.celltype_index);

            cellTypeDomain.CellTypeIndex = cellType.celltype_index;

            Misc.LogOnHawkeyeError("AddCellType", hawkeyeError);
            return hawkeyeError;
        }

        private HawkeyeError RemoveCellType(string username, string password, uint cellTypeIndex)
        {
            Log.Debug("cellTypeIndex: " + cellTypeIndex);
            var hawkeyeError = _cellTypeAccess.RemoveCellTypeAPI(username, password, cellTypeIndex);
            Misc.LogOnHawkeyeError("RemoveCellType", hawkeyeError);
            return hawkeyeError;
        }

        private HawkeyeError ModifyCellType(string username, string password, CellTypeDomain cellTypeDomain)
        {
            var cellType = cellTypeDomain.PopulateNewCellType();
            Log.Debug("cellTypeIndex:  " + cellTypeDomain.CellTypeIndex);
            var hawkeyeError = _cellTypeAccess.ModifyCellTypeAPI(username, password, cellType);
            Misc.LogOnHawkeyeError("ModifyCellType", hawkeyeError);
            return hawkeyeError;
        }

        public List<CellTypeDomain> GetAllCellTypes_BECall()
        {
            uint numberOfCellTypes = 0;
            var allCells = new List<CellTypeDomain>();
            var hawkeyeError = _cellTypeAccess.GetAllCellTypesAPI(ref numberOfCellTypes, ref allCells);
            Misc.LogOnHawkeyeError("GetAllCellTypes::", hawkeyeError);
            return allCells;
        }

        public List<CellTypeDomain> GetAllowedCellTypes_BECall(string username)
        {
            var finalCellList = new List<CellTypeDomain>();
            try
            {
                var result = _cellTypeAccess.GetAllowedCellTypesAPI(username, ref finalCellList);
                if (result != HawkeyeError.eSuccess)
                {
	                Log.Debug($"Exit: results: {result}");
					return null;
                }
			}
            catch
            {
	            Log.Debug($"Exit: catch...");
                return null;
            }

			return finalCellList;
        }

        #endregion

        #region Quality Control API Calls

        public List<QualityControlDomain> GetAllowedQualityControls_BECall(string username, List<CellTypeDomain> cells)
        {
            var qcList = new List<QualityControlDomain>();
            var result = _qualityControlAccess.GetQualityControlListAPI(username, "", false, ref qcList, out var numberOfQcs);
            if (cells == null)
            {
	            Log.Debug($"Exit: cells is null");
                return qcList;
            }
            UpdateQcCtInfo(ref qcList, ref cells);
            return qcList;
        }

        public List<QualityControlDomain> GetAllQualityControls_BECall(out List<CellTypeDomain> allCells)
        {
            uint numberOfCellTypes = 0;
            allCells = new List<CellTypeDomain>();
            var hawkeyeError = _cellTypeAccess.GetAllCellTypesAPI(ref numberOfCellTypes, ref allCells);
            Misc.LogOnHawkeyeError("GetAllQualityControls_BECall", hawkeyeError);
            var qcList = GetAllowedQualityControls_BECall(ScoutUtilities.Common.ApplicationConstants.SilentAdmin, allCells);
            return qcList;
        }

        private HawkeyeError AddQualityControl(string username, string password, QualityControlDomain qualityControlDomain, string retiredQCName)
        {
            var qualityControl = qualityControlDomain.CreateQualityControlStruct();
#if DEBUG
            Log.Debug($"Adding Quality Control...: {qualityControlDomain.QcName}");
#endif
            var result = _qualityControlAccess.AddQualityControlAPI(username, password, qualityControl, retiredQCName);
			Misc.LogOnHawkeyeError($"{nameof(AddQualityControl)}", result);
            return result;
        }

        #endregion

        #endregion
    }
}