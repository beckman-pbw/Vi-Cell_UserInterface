using System;
using System.Collections.Generic;
using GrpcService;
using ScoutDomains;
using ScoutDomains.Analysis;

namespace ScoutServices.Interfaces
{
    public interface ICellTypeManager
    {
        bool InstrumentStateAllowsEdit { get; }
        bool CreateCellType(string username, string password, CellTypeDomain selectedCellType, string retiredCellTypeName, bool showDialogPrompt);
        bool DeleteCellType(string username, string password, string cellTypeName, bool showDialogPrompt);
        bool DeleteCellType(string username, string password, CellTypeDomain cellType, bool showDialogPrompt);
        bool CanPerformDelete(CellTypeDomain cellType);
        bool SaveCellTypeValidation(CellTypeDomain selectedCellType, bool showDialogPrompt);
        bool SaveCellTypeValidation(CellTypeDomain selectedCellType, bool showDialogPrompt, out string invalidParameter);
        bool QualityControlValidation(QualityControlDomain qualityControl, bool showDialogPrompt, out string failureReason, string username = "", string password = "");
        bool QualityControlValidation(QualityControlDomain qualityControl, bool showDialogPrompt);
        bool CreateQualityControl(string username, string password, QualityControlDomain qualityControl, bool showDialogPrompt);
        bool CanAddAdjustmentFactor(string username, CellTypeDomain selectedCellType, out string invalidPermissionLevel);
        List<CellTypeDomain> GetAllCellTypes();

        List<CellTypeDomain> GetAllowedCellTypes(string username);
        List<QualityControlDomain> GetAllowedQualityControls(string username, List<CellTypeDomain> cells);

        IObservable<List<CellTypeDomain>> SubscribeCellTypeRetrieval();
        IObservable<List<QualityControlDomain>> SubscribeQualityControlRetrieval();
        CellTypeDomain GetCellTypeDomain(string username, string password, string cellTypeName);

        QualityControlDomain GetQualityControlDomain(string username, string password, string qcName);
        CellTypeDomain GetCellType(uint cellTypeIndex);
    }
}
