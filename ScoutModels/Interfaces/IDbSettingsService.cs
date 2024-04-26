using ScoutUtilities.Enums;
using ScoutUtilities.Structs;

namespace ScoutModels.Interfaces
{
    public interface IDbSettingsService
    {
        DbConfig GetDbConfig();
        bool SetDbConfig(uint port, string ipAddress, string hostName);
        bool SetDbReadPassword(string password);
        bool SetOpticsConfiguration(OpticalHardwareConfig type);
        OpticalHardwareConfig GetOpticalHardwareConfig();
    }
}