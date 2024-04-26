using System;
using ScoutDomains;
using ScoutUtilities.Enums;
using System.Collections.Generic;
using ScoutUtilities.Structs;

namespace HawkeyeCoreAPI.Interfaces
{
    public interface IQualityControl
    {
        HawkeyeError AddQualityControlAPI(string username, string password, qualitycontrol_t qualityControl, string retiredQCName);
        HawkeyeError GetQualityControlListAPI(string username, string password, bool allFlag, ref List<QualityControlDomain> qualityDoaminControls, out UInt32 num_qcs);
    }
}