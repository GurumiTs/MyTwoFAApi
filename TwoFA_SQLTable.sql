USE YourDatabase;

-- =========================================
-- WebsiteTwoFASetting: 2FA 設定表
-- =========================================
Create Table WebsiteTwoFASetting(
	RowID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY, -- 系統用唯一ID
    UserId NVARCHAR(100) NOT NULL,                   -- 各網站傳來的使用者ID
    WebsiteName NVARCHAR(200) NOT NULL,              -- 呼叫API的網站名稱
    Email NVARCHAR(100) NOT NULL,                    -- Email
    
    TwoFAEnabled BIT NOT NULL DEFAULT 1,             -- 是否啟用2FA
    SkipUntil DATETIME NULL,                         -- 免驗證到期時間 (ex: 7天免驗證)
    --CurrentToken NVARCHAR(10) NULL,                  -- 最新產生的驗證碼
    --TokenExpireAt DATETIME NULL,                     -- 驗證碼過期時間
    --TokenVerified BIT NOT NULL DEFAULT 0,            -- 是否已驗證成功
    LastVerifiedAt DATETIME NULL,                    -- 最後一次驗證成功時間
    
    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME NULL DEFAULT GETUTCDATE()
)
ALTER TABLE WebsiteTwoFASetting
ADD CONSTRAINT UX_WebsiteTwoFASetting_User_Website
UNIQUE (UserId, WebsiteName);

-- =========================================
-- WebsiteTwoFAAuthLogger: 驗證紀錄表
-- =========================================
Create Table WebsiteTwoFA_Auth_Logger(
    AuthRowID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(), -- 系統用唯一ID
    System_Date Datetime NOT NULL,                   -- 系統時間UTC
    UserId NVARCHAR(100) NOT NULL,                   -- 各網站傳來的使用者ID
    WebsiteName NVARCHAR(200) NOT NULL,              -- 呼叫API的網站名稱
    Email NVARCHAR(200) NOT NULL,                    -- Email
    EmailLanguage NVARCHAR(20) NOT NULL,             -- 驗證Email語系
    TokenType NVARCHAR(20) NOT NULL,                 --（Email、SMS、App）
    CurrentToken NVARCHAR(10) NOT NULL,              -- 最新產生的驗證碼
    TokenExpireAt DATETIME NOT NULL,                 -- 驗證碼過期時間
    TokenVerified BIT NULL,                          -- 是否已驗證成功
    --DeviceId UNIQUEIDENTIFIER NOT NULL,              -- 裝置ID
    --ClientIP NVARCHAR(45) NULL,
    --ClientUA NVARCHAR(512) NULL

)

-- =========================================
-- WebsiteTwoFAErrorLogger: 錯誤紀錄表
-- =========================================
Create Table WebsiteTwoFA_Error_Logger(
    LogID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    System_Date Datetime NOT NULL,                   -- 系統時間UTC
    UserId NVARCHAR(100) NOT NULL,                   -- 各網站傳來的使用者ID
    WebsiteName NVARCHAR(200) NOT NULL,              -- 呼叫API的網站名稱
    Event_Name NVARCHAR(200) NOT NULL,               -- 異常程式
    Message NVARCHAR(2000) NULL,                     -- 錯誤訊息
)

-- =========================================
-- WebsiteTwoFASetting: 2FA 設定表
-- =========================================
CREATE TABLE WebsiteTwoFA_TrustedDevice (
    DeviceRowID     UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    Setting_RowID   UNIQUEIDENTIFIER NOT NULL,
    DeviceId        UNIQUEIDENTIFIER NOT NULL,                      -- 由伺服器產生的隨機不透明ID（寫在第一方 Cookie）
    RememberUntil   DATETIME         NULL,                          -- 此裝置免驗證到期（例如 7 天）
    FirstSeenAt     DATETIME         NOT NULL DEFAULT GETUTCDATE(), -- 第一次把這台裝置登記為受信時的 UTC 時間
    LastSeenAt      DATETIME         NOT NULL DEFAULT GETUTCDATE(), -- 最後一次成功使用這台裝置
    FirstIP         NVARCHAR(45)     NULL,
    LastIP          NVARCHAR(45)     NULL,
    FirstUA         NVARCHAR(512)    NULL,
    LastUA          NVARCHAR(512)    NULL,
    IsRevoked       BIT              NOT NULL DEFAULT 0,            -- 是否撤銷此裝置白名單
    RevokedAt       DATETIME         NULL                           -- 撤銷白名單時間
)
CREATE UNIQUE INDEX UX_TD_User_Web_Device
ON WebsiteTwoFA_TrustedDevice(Setting_RowID, DeviceId);

CREATE INDEX IX_TD_TrustCheck
ON WebsiteTwoFA_TrustedDevice(Setting_RowID, DeviceId)
INCLUDE (RememberUntil)
WHERE IsRevoked = 0;

-- 外鍵（刪掉 Setting 時連動清除其所有裝置白名單）
ALTER TABLE WebsiteTwoFA_TrustedDevice WITH CHECK
ADD CONSTRAINT FK_TD_Setting_RowID
FOREIGN KEY (Setting_RowID)
REFERENCES WebsiteTwoFASetting(RowID)
ON DELETE CASCADE;


