using System.Management;

namespace ScoutUtilities.Common
{
    public static class RemovableDeviceHelper
    {
        public static string GetRemovableDrive()
        {
            var deviceSearcher = new ManagementObjectSearcher("SELECT Caption, DeviceID FROM Win32_DiskDrive WHERE InterfaceType='USB'");
            foreach (var usbDevice in deviceSearcher.Get())
            {
                var queryStr = $"ASSOCIATORS OF {{Win32_DiskDrive.DeviceID='{usbDevice["DeviceID"]}'}} WHERE AssocClass = Win32_DiskDriveToDiskPartition";
                foreach (var partition in new ManagementObjectSearcher(queryStr).Get())
                {
                    var query2Str = $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{partition["DeviceID"]}'}} WHERE AssocClass = Win32_LogicalDiskToPartition";
                    foreach (var disk in new ManagementObjectSearcher(query2Str).Get())
                    {
                        return disk["Name"] + "\\";
                    }
                }
            }
            return string.Empty;
        }
    }
}
