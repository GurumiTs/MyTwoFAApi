using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for WebsiteTwoFASetting
/// </summary>
namespace MyApi.Model
{
    public enum WebsiteNameEnum
    {
        Workflow,
        WService,
        EGP,
        SCM,
        QPS,
    }

    public class WebsiteTwoFASetting
    {
        public WebsiteTwoFASetting()
        {
            RowID = Guid.NewGuid();
        }

        [Required]
        public Guid RowID { get; set; }   // UNIQUEIDENTIFIER

        [Required]
        public string UserId { get; set; }   // 使用者ID

        [Required]
        public string WebsiteName { get; set; }   // 對應網站名稱

        [Required]
        public string Email { get; set; }   // Email

        [Required]
        public bool TwoFAEnabled { get; set; }   // 是否啟用2FA

        public DateTime? SkipUntil { get; set; }   // 免驗證到期時間

        public DateTime? LastVerifiedAt { get; set; }   // 最後一次驗證成功時間

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

    }

    public class _2FAReturnContent
    {
        public bool APISuccess { get; set; }

        public string UserId { get; set; }

        public string WebsiteName { get; set; }

        public DateTime? SkipUntil { get; set; }

        public bool? NeedVerify { get; set; }

        public bool? VerifiedStatus { get; set; }

        public WebsiteTwoFA_Auth_Logger AuthInfo { get; set; }

        public string Msg { get; set; }
    }

    public class WebsiteTwoFA_TrustedDevice
    {
        public WebsiteTwoFA_TrustedDevice()
        {
            DeviceRowID = Guid.NewGuid();
            FirstSeenAt = LastSeenAt = DateTime.UtcNow;
        }

        [Required] public Guid DeviceRowID { get; set; }

        [Required] public Guid Setting_RowID { get; set; }

        //[Required] public string WebsiteName { get; set; }

        [Required] public Guid DeviceId { get; set; }

        public DateTime? RememberUntil { get; set; }

        public DateTime FirstSeenAt { get; set; }

        public DateTime LastSeenAt { get; set; }

        public string FirstIP { get; set; }

        public string LastIP { get; set; }

        public string FirstUA { get; set; }

        public string LastUA { get; set; }

        public bool IsRevoked { get; set; }

        public DateTime? RevokedAt { get; set; }
    }

    public class Verify2FARequest
    {
        [Required]
        public string UserId { get; set; }   // 使用者ID

        [Required]
        public string WebsiteName { get; set; }   // 對應網站名稱

        [Required]
        public string CurrentToken { get; set; }   // 最新驗證碼

        public bool? TrustThisDevice { get; set; } // 是否信任此裝置7天

    }

}