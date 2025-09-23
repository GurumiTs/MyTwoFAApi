using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;

namespace MyApi.Repository
{
    public interface ITwoFARepository
    {
        #region Repository
        Task<int> CreateAsync(WebsiteTwoFASetting setting);

        Task<WebsiteTwoFASetting> GetByUserIdAsync(string userId, string websiteName);

        Task<int> UpdateByRowIdAsync(WebsiteTwoFASetting setting);

        Task<int> DeleteAsync(string rowId);
        
        Task<int> Auth_CreateAsync(WebsiteTwoFA_Auth_Logger logger);

        Task<WebsiteTwoFA_Auth_Logger> Auth_GetLatestTokenAsync(string userId, string websiteName);

        Task<int> Auth_UpdateVerifiedAsync(WebsiteTwoFA_Auth_Logger logger);

        Task<int> Auth_DeleteLegacyLoggerAsync(WebsiteTwoFA_Auth_Logger logger);

        Task<int> Error_WriteAsync(WebsiteTwoFAErrorLogger error);
        
        Task<bool> TD_IsTrustedAsync(Guid settingRowId, Guid deviceId, DateTime nowUtc);
        
        Task<int> TD_UpsertAsync(Guid settingRowId, Guid deviceId, DateTime nowUtc, DateTime? rememberUntil, string ip, string ua);
        
        Task<int> TD_RevokeAsync(Guid settingRowId, Guid deviceId, DateTime nowUtc);
        
        Task<int> TD_RevokeAllAsync(Guid settingRowId, DateTime nowUtc);

        #endregion

        #region Send Email
        Task SendAuthEmailAsync(WebsiteTwoFA_Auth_Logger auth_token);

        Task SendMailAsync(string[] tos, string[] Ccs, string[] bcc, string from, string subject, string body, string smtpHost, int smtpPort, string userName, string password, Attachment file);

        #endregion

        #region Service
        Task<WebsiteTwoFASetting> CheckSettingsAsync(WebsiteTwoFASetting _2FARequest, WebsiteTwoFASetting twoFASetting, DateTime timeNow);

        Task<WebsiteTwoFA_Auth_Logger> Generate2FATokenAsync(WebsiteTwoFASetting twoFASetting, string mailLanguage, DateTime timeNow);

        #endregion

    }
}
