using JetBrains.Annotations;
using log4net;
using ScoutDomains;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using HawkeyeCoreAPI.Interfaces;

namespace HawkeyeCoreAPI
{
    public class QualityControl : IQualityControl
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region API_Declarations

        [DllImport(ApplicationConstants.DllName)]
        static extern void FreeListOfQualityControl(IntPtr list, UInt32 n_items);

        [DllImport(ApplicationConstants.DllName)]
        [MustUseReturnValue(ApplicationConstants.Justification)]
        static extern HawkeyeError GetQualityControlList(string username, string password, bool allFlag, out IntPtr qualityControls, out UInt32 num_qcs);

        [DllImport(ApplicationConstants.DllName)]
        [MustUseReturnValue(ApplicationConstants.Justification)]
        static extern HawkeyeError AddQualityControl(string username, string password, qualitycontrol_t qualityControl, string retiredQCName);

        [DllImport(ApplicationConstants.DllName)]
        [MustUseReturnValue(ApplicationConstants.Justification)]
        public static extern HawkeyeError RetrieveSampleRecordsForQualityControl(string quality_Name,
            out IntPtr reclist, out UInt32 list_size);

        #endregion

        #region API_Calls

        #region IQualityControl API Calls

        // These methods should only be accessed through the CellTypeFacade.cs to ensure proper caching of the CellTypes
        // for broad application use.

        [MustUseReturnValue(ApplicationConstants.Justification)]
        public HawkeyeError AddQualityControlAPI(string username, string password, qualitycontrol_t qualityControl, string retiredQCName)
        {
            var result = AddQualityControl(username, password, qualityControl, retiredQCName);
            qualityControl.qc_name.ReleaseIntPtr();
            qualityControl.comment_text.ReleaseIntPtr();
            qualityControl.lot_information.ReleaseIntPtr();
            return result;
        }

        [MustUseReturnValue(ApplicationConstants.Justification)]
        public HawkeyeError GetQualityControlListAPI(string username, string password, bool allFlag, ref List<QualityControlDomain> qualityDomainControls,
            out UInt32 num_qcs)
        {
            IntPtr buffer_qualityControl;
            var qualityControls = new List<qualitycontrol_t>();
            var hawkeyeError = GetQualityControlList(username, password, allFlag, out buffer_qualityControl, out num_qcs);
            var qcPtr = buffer_qualityControl;
            for (int i = 0; i < num_qcs; i++)
            {
                qualityControls.Add((qualitycontrol_t)Marshal.PtrToStructure(qcPtr, typeof(qualitycontrol_t)));
                qcPtr = new IntPtr(qcPtr.ToInt64() + Marshal.SizeOf(typeof(qualitycontrol_t)));
            }

            if (qualityControls.Count > 0)
            {
                qualityDomainControls = CreateQualityControlList(qualityControls);
            }

            FreeListOfQualityControlAPI(buffer_qualityControl, num_qcs);

            return hawkeyeError;
        }

        #endregion

        #region Public API Calls

        [MustUseReturnValue(ApplicationConstants.Justification)]
        public static HawkeyeError RetrieveSampleRecordsForQualityControlAPI(string qcName, out List<SampleRecordDomain> recList, out uint listSize)
        {
            var cleanQcName = Misc.GetBaseQualityControlName(qcName);
            var hawkeyeError = RetrieveSampleRecordsForQualityControl(cleanQcName, out var ptrRecList, out listSize);
            recList = Sample.ConvertSampleRecords_toManagedList(ptrRecList, listSize);
            return hawkeyeError;
        }

        #endregion

        #endregion

        #region Private Methods

        private static List<QualityControlDomain> CreateQualityControlList(List<qualitycontrol_t> qualityControlStructList)
        {
            var qualityControls = new List<QualityControlDomain>();
            if (qualityControlStructList.Any())
            {
                qualityControlStructList.ForEach(qualityControl =>
                {
                    qualityControls.Add(new QualityControlDomain
                    {
                        AssayParameter = qualityControl.assay_type,
                        AssayValue = qualityControl.assay_value,
                        CellTypeIndex = qualityControl.cell_type_index,
                        CommentText = qualityControl.comment_text.ToSystemString(),
                        ExpirationDate = DateTimeConversionHelper.FromDaysUnixToDateTime(qualityControl.expiration_date),
                        LotInformation = qualityControl.lot_information.ToSystemString(),
                        AcceptanceLimit = Convert.ToInt16(qualityControl.plusminus_percentage),
                        QcName = qualityControl.qc_name.ToSystemString(),
                    });
                });
            }

            return qualityControls;
        }

        private static void FreeListOfQualityControlAPI(IntPtr list, UInt32 n_items)
        {
            if (n_items > 0)
            {
                if (list == IntPtr.Zero)
                {
                }
                else
                {
                    FreeListOfQualityControl(list, n_items);
                }
            }
            else
            {
#if DEBUG
                Log.Debug("FreeListOfQualityControlAPI: n_items is zero");
#endif
            }
        }

        #endregion
    }
}
