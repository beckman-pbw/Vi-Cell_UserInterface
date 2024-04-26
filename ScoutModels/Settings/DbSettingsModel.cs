using HawkeyeCoreAPI;
using HawkeyeCoreAPI.Interfaces;
using ScoutModels.Interfaces;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;

namespace ScoutModels.Settings
{
    public class DbSettingsModel : IDbSettingsService
    {
        private readonly DatabaseSettingsApi _dbSettingsApi;

        public DbSettingsModel()
        {
            _dbSettingsApi = new DatabaseSettingsApi();
        }

        public DbConfig GetDbConfig()
        {
            var dbConfig = _dbSettingsApi.GetDbConfigApi();
            if (string.IsNullOrEmpty(dbConfig.IpAddress)) dbConfig.IpAddress = ApplicationConstants.DefaultDbIpAddress;
            if (string.IsNullOrEmpty(dbConfig.Name)) dbConfig.Name = ApplicationConstants.DefaultDbName;
            if (dbConfig.Port == 0) dbConfig.Port = ApplicationConstants.DefaultDbPort;
            return dbConfig;
        }

        public bool SetDbConfig(uint port, string ipAddress, string hostName)
        {
            var dbConfig = new DbConfig(port, ipAddress, hostName);
            return _dbSettingsApi.SetDbConfigApi(dbConfig);
        }

        public bool SetDbReadPassword(string password)
        {
            return _dbSettingsApi.SetDbBackupUserPasswordAPI(password);
        }

        public bool SetOpticsConfiguration(OpticalHardwareConfig type)
        {
            return _dbSettingsApi.SetOpticalHardwareConfigAPI(type);
        }

        public OpticalHardwareConfig GetOpticalHardwareConfig()
        {
            return _dbSettingsApi.GetOpticalHardwareConfigAPI();
        }
    }
}