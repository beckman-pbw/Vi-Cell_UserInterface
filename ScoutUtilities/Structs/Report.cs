using ScoutUtilities.Enums;
using System;
using System.Runtime.InteropServices;

namespace ScoutUtilities.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct error_log_entry
    {
        public UInt64 timestamps;
        public IntPtr user_id;
        public IntPtr message;
        public UInt32 error_code;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct audit_log_entry
    {
        public UInt64 timestamp; // seconds since 1/1/1970 UTC
        public IntPtr active_user_id; // involved user (logged-in or account triggering audit entry)
        public audit_event_type event_type;
        public IntPtr event_message;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct workqueue_sample_entry
    {
        public UInt64 timestamp;
        public IntPtr sample_label;
        public IntPtr celltype_name;
        public IntPtr analysis_name;
        public sample_completion_status completion;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct sample_activity_entry
    {
        public UInt64 timestamp; // seconds since 1/1/1970 UTC
        public IntPtr user_id;
        public IntPtr workqueuelabel;
        public UInt32 number_of_samples;
        public IntPtr samples; // convert into workqueue_sample_entry
    }
}