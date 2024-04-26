using ScoutUtilities.Structs;

namespace ScoutModels.Interfaces
{
    public interface ISmtpSettingsService
    {
        bool IsValidEmail(string email);
        SmtpConfig GetSmtpConfig();
        bool SetSmtpConfig(uint port, string server, string username, string password, bool authEnabled);
        bool SendEmail(string userId, string email);
    }
}