using HawkeyeCoreAPI.Interfaces;
using JetBrains.Annotations;
using log4net;
using ScoutDomains.Analysis;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ScoutUtilities;

namespace HawkeyeCoreAPI
{
    public class CellType : ICellType
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region API_Declarations

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError AddCellType(string username, string password, out ScoutUtilities.Structs.CellType cellType, out UInt32 cellTypeIndex, string retiredCellTypeName);

        // Deprecated
        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError ModifyCellType(string username, string password, ScoutUtilities.Structs.CellType cellType);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError RemoveCellType(string username, string password, UInt32 cellTypeIndex);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError GetAllCellTypes(out UInt32 ncount, out IntPtr ptrUAllCellTypes);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError GetAllowedCellTypes(string username, out UInt32 ncount, out IntPtr ptrUAllCellTypes);

        [DllImport("HawkeyeCore.dll")]
        static extern void FreeListOfCellType(IntPtr ptrList, UInt32 nCelltypes);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError SetUserCellTypeIndices(string userName, UInt32 nCells, IntPtr userCellIndices);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError GetFactoryCellTypes(out UInt32 ncount, out IntPtr ptrFactoryCellTypes);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError GetTemporaryCellType(out IntPtr temp_cell);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError SetTemporaryCellType(IntPtr temp_cell);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError SetTemporaryCellTypeFromExisting(UInt32 ct_index);

        #endregion

        #region API_Calls

        #region ICellType API Call Methods

        // These methods should only be accessed through the CellTypeFacade.cs to ensure proper caching of the CellTypes
        // for broad application use.

        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError AddCellTypeAPI(string username, string password, ScoutUtilities.Structs.CellType cellType, string retiredCellTypeName, ref uint cellTypeIndex)
        {
            return AddCellType(username, password, out cellType, out cellTypeIndex, retiredCellTypeName);
        }

        // Deprecated
        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError ModifyCellTypeAPI(string username, string password, ScoutUtilities.Structs.CellType cellType)
        {
            return ModifyCellType(username, password, cellType);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError RemoveCellTypeAPI(string username, string password, uint index)
        {
            return RemoveCellType(username, password, index);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError GetAllCellTypesAPI(ref uint num_ct, ref List<CellTypeDomain> cellTypeDomainList)
        {
            var cellTypeList = new List<ScoutUtilities.Structs.CellType>();
            int sz = Marshal.SizeOf(typeof(ScoutUtilities.Structs.CellType));
            IntPtr ptrAllCellTypes;
            var hawkeyeError = GetAllCellTypes(out num_ct, out ptrAllCellTypes);
            var cellTypePointer = ptrAllCellTypes;
            for (int i = 0; i < num_ct; i++)
            {
                cellTypeList.Add((ScoutUtilities.Structs.CellType)Marshal.PtrToStructure(cellTypePointer, typeof(ScoutUtilities.Structs.CellType)));
                cellTypePointer += sz;
            }

            if (cellTypeList.Count > 0)
            {
                foreach (var cell in cellTypeList)
                {
                    cellTypeDomainList.Add(GetCellType(cell));
                }
            }

            FreeCellTypeAPI(ptrAllCellTypes, num_ct);

#if DEBUG 
            Misc.LogOnHawkeyeError($"{nameof(GetAllCellTypes)}", hawkeyeError); // logging to track PC3549-927
#else
            if (hawkeyeError != HawkeyeError.eSuccess)
            {
                Log.Warn($"{nameof(GetAllCellTypes)} result: {hawkeyeError}");
            }
#endif

            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError GetAllowedCellTypesAPI(string username, ref List<CellTypeDomain> cellTypeDomainList)
        {
            var cellTypeList = new List<ScoutUtilities.Structs.CellType>();
            int sz = Marshal.SizeOf(typeof(ScoutUtilities.Structs.CellType));
            IntPtr ptrAllCellTypes;
            UInt32 num_ct = 0;
            var hawkeyeError = GetAllowedCellTypes(username, out num_ct, out ptrAllCellTypes);
            var cellTypePointer = ptrAllCellTypes;
            for (int i = 0; i < num_ct; i++)
            {
                cellTypeList.Add((ScoutUtilities.Structs.CellType)Marshal.PtrToStructure(cellTypePointer, typeof(ScoutUtilities.Structs.CellType)));
                cellTypePointer += sz;
            }

            if (cellTypeList.Count > 0)
            {
                foreach (var cell in cellTypeList)
                {
                    cellTypeDomainList.Add(GetCellType(cell));
                }
            }

            FreeCellTypeAPI(ptrAllCellTypes, num_ct);

			Misc.LogOnHawkeyeError($"{nameof(GetAllowedCellTypesAPI)} result: ", hawkeyeError);

			return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError GetFactoryCellTypesAPI(ref List<CellTypeDomain> cellTypeDomainList)
        {
            uint num_ct = 0;
            int sz = Marshal.SizeOf(typeof(ScoutUtilities.Structs.CellType));
            var cellTypeList = new List<ScoutUtilities.Structs.CellType>();
            IntPtr ptrAllCellTypes;
            var hawkeyeError = GetFactoryCellTypes(out num_ct, out ptrAllCellTypes);

            var cellTypePointer = ptrAllCellTypes;
            for (int i = 0; i < num_ct; i++)
            {
                cellTypeList.Add((ScoutUtilities.Structs.CellType)Marshal.PtrToStructure(cellTypePointer, typeof(ScoutUtilities.Structs.CellType)));
                cellTypePointer += sz;
            }

            if (cellTypeList.Count > 0)
            {
                foreach (var cell in cellTypeList)
                {
                    cellTypeDomainList.Add(GetCellType(cell));
                }
            }

            FreeCellTypeAPI(ptrAllCellTypes, num_ct);

            return hawkeyeError;
        }

#endregion

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError SetUserCellTypeIndicesAPI(string username, List<UInt32> cellTypeIndices)
        {
            var wqiSize = Marshal.SizeOf(typeof(UInt32));
            IntPtr cellTypeIndicesPtr = Marshal.AllocCoTaskMem(cellTypeIndices.Count * wqiSize);
            for (int i = 0; i < cellTypeIndices.Count; i++)
            {
                var pTmp = cellTypeIndicesPtr + (i * wqiSize);
                Marshal.StructureToPtr(cellTypeIndices[i], pTmp, false);
            }

            var hawkeyeError = SetUserCellTypeIndices(username, (UInt32)cellTypeIndices.Count, cellTypeIndicesPtr);
            Marshal.FreeCoTaskMem(cellTypeIndicesPtr);

            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError GetTemporaryCellTypeAPI(ref List<CellTypeDomain> TemporaryCellTypeAll)
        {
            var cellTypeList = new List<ScoutUtilities.Structs.CellType>();
            IntPtr TemporaryCellType;
            var hawkeyeError = GetTemporaryCellType(out TemporaryCellType);
            if (TemporaryCellType != IntPtr.Zero)
            {
                cellTypeList.Add((ScoutUtilities.Structs.CellType)Marshal.PtrToStructure(TemporaryCellType, typeof(ScoutUtilities.Structs.CellType)));
            }

            if (cellTypeList.Count > 0)
            {
                foreach (var cell in cellTypeList)
                {
                    TemporaryCellTypeAll.Add(GetCellType(cell));
                }
            }

            FreeCellTypeAPI(TemporaryCellType, 1);

            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError SetTemporaryCellTypeAPI(ScoutUtilities.Structs.CellType cell)
        {
            var pTmp = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(ScoutUtilities.Structs.CellType)));
            Marshal.StructureToPtr(cell, pTmp, false);
            var hawkeyeStatus = SetTemporaryCellType(pTmp);
            Marshal.FreeCoTaskMem(pTmp);
            return hawkeyeStatus;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError SetTemporaryCellTypeFromExistingAPI(UInt32 ct_index)
        {
            return SetTemporaryCellTypeFromExisting(ct_index);
        }

        #endregion

        #region Private Methods

        private static CellTypeDomain GetCellType(ScoutUtilities.Structs.CellType cellType)
        {
            var cell = new CellTypeDomain();

            cell.CellTypeIndex = cellType.celltype_index;
            cell.CellTypeName = cellType.label.ToSystemString();
            cell.TempCellName = cellType.label.ToSystemString();
            cell.MinimumDiameter = cellType.minimum_diameter_um;
            cell.MaximumDiameter = cellType.maximum_diameter_um;
            cell.Images = cellType.max_image_count;
            cell.MinimumCircularity = cellType.minimum_circularity;
            cell.CellSharpness = cellType.sharpness_limit;
            cell.DeclusterDegree = cellType.decluster_setting;
            cell.AnalysisDomain = GetAnalysisDomain(cellType.analysis_specializations);
            cell.AspirationCycles = cellType.aspiration_cycles;
            cell.CalculationAdjustmentFactor = cellType.calculation_adjustment_factor;
            cell.IsUserDefineCellType = ((cell.CellTypeIndex & 0x80000000) == 0x80000000);

            return cell;
        }

        private static AnalysisDomain GetAnalysisDomain(IntPtr intPtr)
        {
            var appDomain = new AnalysisDomain();
            if (intPtr == IntPtr.Zero) return appDomain;

            var analysisDefinition = (AnalysisDefinition)Marshal.PtrToStructure(intPtr, typeof(AnalysisDefinition));
            appDomain = AnalysisDomain.GetAnalysisDomain(analysisDefinition);

            return appDomain;
        }

        private static void FreeCellTypeAPI(IntPtr cellTypePtr, uint count)
        {
            if (cellTypePtr == IntPtr.Zero || count <= 0) return;
            FreeListOfCellType(cellTypePtr, count);
        }

        private static void FreeCellTypeIndicesAPI(IntPtr ptrCellTypeIndices, NativeDataType tag)
        {
            if (ptrCellTypeIndices != IntPtr.Zero)
            {
                GenericFree.FreeTaggedBufferAPI(tag, ptrCellTypeIndices);
            }
        }

        #endregion

    }
}
