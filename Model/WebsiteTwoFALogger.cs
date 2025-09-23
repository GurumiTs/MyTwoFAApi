using System;
using System.ComponentModel.DataAnnotations;

namespace MyApi.Model
{
    public enum TokenTypeEnum
    {
        Email,
        App
    }

    public enum languageEnum
    {
        zh_TW,
        en_US,
        vi_VN
    }

    public class WebsiteTwoFA_Auth_Logger
    {
        public WebsiteTwoFA_Auth_Logger()
        {
            AuthRowID = Guid.NewGuid();
            System_Date = DateTime.UtcNow;
        }

        [Required]
        public Guid AuthRowID { get; set; }   // UNIQUEIDENTIFIER

        [Required]
        public DateTime System_Date { get; set; }

        [Required]
        public string UserId { get; set; }   // 使用者ID

        [Required]
        public string WebsiteName { get; set; }   // 對應網站名稱

        [Required]
        public string Email { get; set; }   // Email

        public languageEnum EmailLanguage { get; set; }   // 驗證Email語系

        [Required]
        public TokenTypeEnum TokenType { get; set; }   // Type

        [Required]
        public string CurrentToken { get; set; }   // 最新驗證碼

        [Required]
        public DateTime TokenExpireAt { get; set; }   // 驗證碼過期時間

        public bool? TokenVerified { get; set; }   // 是否已驗證

        //public Guid DeviceId {  get; set; }  // 裝置ID

        //public string ClientIP {  get; set; }  // IP

        //public string ClientUA { get; set; }  // UserAgent
    }

    public class WebsiteTwoFAErrorLogger
    {
        public WebsiteTwoFAErrorLogger()
        {
            LogID = Guid.NewGuid();
            System_Date = DateTime.UtcNow;
        }

        public WebsiteTwoFAErrorLogger(string userId, string website, string eventName, string message)
        {
            LogID = Guid.NewGuid();
            System_Date = DateTime.UtcNow;
            UserId = userId;
            WebsiteName = website;
            EventName = eventName;
            Message = message;
        }

        [Required]
        public Guid LogID { get; set; }   // UNIQUEIDENTIFIER

        [Required]
        public DateTime System_Date { get; set; }

        [Required]
        public string UserId { get; set; }   // 使用者ID

        [Required]
        public string WebsiteName { get; set; }   // 對應網站名稱

        [Required]
        public string EventName { get; set; }   // 事件名稱

        public string Message { get; set; }   // 錯誤訊息

    }
}
