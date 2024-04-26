using System;

namespace ScoutDataAccessLayer.IDAL
{
    public interface IDataAccess
    {
        T ReadConfigurationData<T>(string userId, string settingsName, out bool userFound, bool removeDomainTextFromSettingsName = false);
        T ReadMasterData<T>();
        void WriteToConfigurationXML<T>(T inputObject, string userId, DateTime? dateTime) where T : class;
    }
}