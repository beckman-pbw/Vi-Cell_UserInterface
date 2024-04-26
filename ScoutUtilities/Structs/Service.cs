using ScoutUtilities.Enums;
using System;
using System.Runtime.InteropServices;

namespace ScoutUtilities.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct calibration_consumable
    {
        public IntPtr label;
        public IntPtr lot_id;
        public UInt32 expiration_date; // days since 1/1/1970
        public double assay_value;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct calibration_history_entry
    {
        public UInt64 timestamp;
        public IntPtr user_id;
        public calibration_type cal_type;

        public UInt16 num_consumables;
        public IntPtr consumable_list;

        public double slope;
        public double intercept;
        public UInt32 image_count;
        public uuidDLL uuid;
    }
}