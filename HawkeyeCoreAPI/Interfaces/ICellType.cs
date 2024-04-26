using ScoutDomains.Analysis;
using ScoutUtilities.Enums;
using System;
using System.Collections.Generic;

namespace HawkeyeCoreAPI.Interfaces
{
    public interface ICellType
    {
        HawkeyeError AddCellTypeAPI(string username, string password, ScoutUtilities.Structs.CellType cellType, string retiredCellTypeName, ref uint cellTypeIndex);

        // Deprecated
        HawkeyeError ModifyCellTypeAPI(string username, string password, ScoutUtilities.Structs.CellType cellType);
        
        HawkeyeError RemoveCellTypeAPI(string username, string password, uint index);
        HawkeyeError GetAllCellTypesAPI(ref uint num_ct, ref List<CellTypeDomain> cellTypeDomainList);
        HawkeyeError GetAllowedCellTypesAPI(string username, ref List<CellTypeDomain> cellTypeDomainList);
        HawkeyeError GetFactoryCellTypesAPI(ref List<CellTypeDomain> cellTypeDomainList);
    }
}