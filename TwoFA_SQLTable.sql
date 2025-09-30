USE YourDatabase;

-- =========================================
-- WebsiteTwoFASetting: 2FA �]�w��
-- =========================================
Create Table WebsiteTwoFASetting(
	RowID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY, -- �t�Υΰߤ@ID
    UserId NVARCHAR(100) NOT NULL,                   -- �U�����ǨӪ��ϥΪ�ID
    WebsiteName NVARCHAR(200) NOT NULL,              -- �I�sAPI�������W��
    Email NVARCHAR(100) NOT NULL,                    -- Email
    
    TwoFAEnabled BIT NOT NULL DEFAULT 1,             -- �O�_�ҥ�2FA
    SkipUntil DATETIME NULL,                         -- �K���Ҩ���ɶ� (ex: 7�ѧK����)
    --CurrentToken NVARCHAR(10) NULL,                  -- �̷s���ͪ����ҽX
    --TokenExpireAt DATETIME NULL,                     -- ���ҽX�L���ɶ�
    --TokenVerified BIT NOT NULL DEFAULT 0,            -- �O�_�w���Ҧ��\
    LastVerifiedAt DATETIME NULL,                    -- �̫�@�����Ҧ��\�ɶ�
    
    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME NULL DEFAULT GETUTCDATE()
)
ALTER TABLE WebsiteTwoFASetting
ADD CONSTRAINT UX_WebsiteTwoFASetting_User_Website
UNIQUE (UserId, WebsiteName);

-- =========================================
-- WebsiteTwoFAAuthLogger: ���Ҭ�����
-- =========================================
Create Table WebsiteTwoFA_Auth_Logger(
    AuthRowID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(), -- �t�Υΰߤ@ID
    System_Date Datetime NOT NULL,                   -- �t�ήɶ�UTC
    UserId NVARCHAR(100) NOT NULL,                   -- �U�����ǨӪ��ϥΪ�ID
    WebsiteName NVARCHAR(200) NOT NULL,              -- �I�sAPI�������W��
    Email NVARCHAR(200) NOT NULL,                    -- Email
    EmailLanguage NVARCHAR(20) NOT NULL,             -- ����Email�y�t
    TokenType NVARCHAR(20) NOT NULL,                 --�]Email�BSMS�BApp�^
    CurrentToken NVARCHAR(10) NOT NULL,              -- �̷s���ͪ����ҽX
    TokenExpireAt DATETIME NOT NULL,                 -- ���ҽX�L���ɶ�
    TokenVerified BIT NULL,                          -- �O�_�w���Ҧ��\
    --DeviceId UNIQUEIDENTIFIER NOT NULL,              -- �˸mID
    --ClientIP NVARCHAR(45) NULL,
    --ClientUA NVARCHAR(512) NULL

)

-- =========================================
-- WebsiteTwoFAErrorLogger: ���~������
-- =========================================
Create Table WebsiteTwoFA_Error_Logger(
    LogID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    System_Date Datetime NOT NULL,                   -- �t�ήɶ�UTC
    UserId NVARCHAR(100) NOT NULL,                   -- �U�����ǨӪ��ϥΪ�ID
    WebsiteName NVARCHAR(200) NOT NULL,              -- �I�sAPI�������W��
    Event_Name NVARCHAR(200) NOT NULL,               -- ���`�{��
    Message NVARCHAR(2000) NULL,                     -- ���~�T��
)

-- =========================================
-- WebsiteTwoFASetting: 2FA �]�w��
-- =========================================
CREATE TABLE WebsiteTwoFA_TrustedDevice (
    DeviceRowID     UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    Setting_RowID   UNIQUEIDENTIFIER NOT NULL,
    DeviceId        UNIQUEIDENTIFIER NOT NULL,                      -- �Ѧ��A�����ͪ��H�����z��ID�]�g�b�Ĥ@�� Cookie�^
    RememberUntil   DATETIME         NULL,                          -- ���˸m�K���Ҩ���]�Ҧp 7 �ѡ^
    FirstSeenAt     DATETIME         NOT NULL DEFAULT GETUTCDATE(), -- �Ĥ@����o�x�˸m�n�O�����H�ɪ� UTC �ɶ�
    LastSeenAt      DATETIME         NOT NULL DEFAULT GETUTCDATE(), -- �̫�@�����\�ϥγo�x�˸m
    FirstIP         NVARCHAR(45)     NULL,
    LastIP          NVARCHAR(45)     NULL,
    FirstUA         NVARCHAR(512)    NULL,
    LastUA          NVARCHAR(512)    NULL,
    IsRevoked       BIT              NOT NULL DEFAULT 0,            -- �O�_�M�P���˸m�զW��
    RevokedAt       DATETIME         NULL                           -- �M�P�զW��ɶ�
)
CREATE UNIQUE INDEX UX_TD_User_Web_Device
ON WebsiteTwoFA_TrustedDevice(Setting_RowID, DeviceId);

CREATE INDEX IX_TD_TrustCheck
ON WebsiteTwoFA_TrustedDevice(Setting_RowID, DeviceId)
INCLUDE (RememberUntil)
WHERE IsRevoked = 0;

-- �~��]�R�� Setting �ɳs�ʲM����Ҧ��˸m�զW��^
ALTER TABLE WebsiteTwoFA_TrustedDevice WITH CHECK
ADD CONSTRAINT FK_TD_Setting_RowID
FOREIGN KEY (Setting_RowID)
REFERENCES WebsiteTwoFASetting(RowID)
ON DELETE CASCADE;


