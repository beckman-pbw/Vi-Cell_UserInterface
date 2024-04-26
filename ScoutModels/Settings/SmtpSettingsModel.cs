using ApiProxies.Generic;
using HawkeyeCoreAPI;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using ScoutLanguageResources;
using ScoutModels.Interfaces;
using ScoutUtilities.Structs;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ScoutModels.Settings
{
    public class SmtpSettingsModel : ISmtpSettingsService
    {
        private readonly SmtpSettingsApi _smtpSettingsApi;

        public SmtpSettingsModel()
        {
            _smtpSettingsApi = new SmtpSettingsApi();
        }

        /// <summary>
        /// Taken from https://docs.microsoft.com/en-us/dotnet/standard/base-types/how-to-verify-that-strings-are-in-valid-email-format
        /// We need to check whether it is a valid email because the factory_admin user has the value "factory_admin" which is not empty,
        /// but is also not a valid email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                    RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    string domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public SmtpConfig GetSmtpConfig()
        {
            var smtpConfig = _smtpSettingsApi.GetSmtpConfigApi();
            return smtpConfig;
        }

        public bool SetSmtpConfig(uint port, string server, string username, string password, bool authEnabled)
        {
            var smtpConfig = new SmtpConfig(port, server, username, password, authEnabled);
            if (!ValidateConfig(smtpConfig))
                return false;
            return _smtpSettingsApi.SetSmtpConfigApi(smtpConfig);
        }

        public bool SendEmail(string userId, string email)
        {
            try
            {
                var smtpConfig = GetSmtpConfig();

                if (string.IsNullOrEmpty(smtpConfig.Server) || smtpConfig.Port == 0)
                    return false;

                var messageToSend = new MimeMessage
                {
                    Subject = LanguageResourceHelper.Get("LID_Password_Reset_Email_Subject"),
                };

                messageToSend.From.Add(new MailboxAddress(LanguageResourceHelper.Get("LID_Title_ViCellBlu"), smtpConfig.Username));
                messageToSend.Body = new TextPart(TextFormat.Plain) { Text = LanguageResourceHelper.Get("LID_Password_Reset_Email_Body") };
                messageToSend.To.Add(new MailboxAddress(userId, email));

                using (var client = new SmtpClient())
                {
                    client.Connect(smtpConfig.Server, (int)smtpConfig.Port);
                    if (ScoutUtilities.Misc.ByteToBool(smtpConfig.AuthEnabled))
                    {
                        client.Authenticate(smtpConfig.Username, smtpConfig.Password);
                    }
                    client.Send(messageToSend);
                    client.Disconnect(true);
                }

                return true;
            }
            catch (ServiceNotAuthenticatedException authException)
            {
                ExceptionHelper.HandleExceptions(authException, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_SEND_EMAIL_ERROR_AUTH"));
                return false;
            }
            catch (AuthenticationException authException)
            {
                ExceptionHelper.HandleExceptions(authException, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_SEND_EMAIL_ERROR_AUTH"));
                return false;
            }
            catch (Exception e)
            {
                ExceptionHelper.HandleExceptions(e, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_SEND_EMAIL_ERROR"));
                return false;
            }
        }

        public static bool ValidateConfig(SmtpConfig smtpConfig)
        {
            try
            {
                if (string.IsNullOrEmpty(smtpConfig.Server) || smtpConfig.Port == 0)
                    return false;

                using (var client = new SmtpClient())
                {
                    client.Connect(smtpConfig.Server, (int)smtpConfig.Port);
                    if (ScoutUtilities.Misc.ByteToBool(smtpConfig.AuthEnabled))
                    {
                        client.Authenticate(smtpConfig.Username, smtpConfig.Password);
                    }
                    client.Disconnect(true);
                }
                return true;
            }
            catch (ServiceNotAuthenticatedException authException)
            {
                ExceptionHelper.HandleExceptions(authException, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_SEND_EMAIL_ERROR_AUTH"));
                return false;
            }
            catch (AuthenticationException authException)
            {
                ExceptionHelper.HandleExceptions(authException, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_SEND_EMAIL_ERROR_AUTH"));
                return false;
            }
            catch (Exception e)
            {
                ExceptionHelper.HandleExceptions(e, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_SEND_EMAIL_ERROR"));
                return false;
            }
        }

    }
}