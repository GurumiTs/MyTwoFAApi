using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyApi.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// 因此為小專案，故將Repository及Service層寫在一起。
/// </summary>
namespace MyApi.Repository
{
    public class TwoFARepository : ITwoFARepository
    {
        #region Repository
        private readonly string _connectionString;

        public TwoFARepository()
        {
            var builder = new ConfigurationBuilder()
                  .SetBasePath(Directory.GetCurrentDirectory())
                  .AddJsonFile("appsettings.json");
            var config = builder.Build();
            _connectionString = config.GetConnectionString("API_DB");
        }

        public async Task<int> CreateAsync(WebsiteTwoFASetting setting)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    var sql = @"INSERT INTO WebsiteTwoFASetting 
                        VALUES (@RowID, @UserId, @WebsiteName, @Email, @TwoFAEnabled, @SkipUntil, @LastVerifiedAt, @CreatedAt, @UpdatedAt)";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@RowID", setting.RowID);
                        cmd.Parameters.AddWithValue("@UserId", setting.UserId);
                        cmd.Parameters.AddWithValue("@WebsiteName", setting.WebsiteName);
                        cmd.Parameters.AddWithValue("@Email", setting.Email);
                        cmd.Parameters.AddWithValue("@TwoFAEnabled", setting.TwoFAEnabled);
                        cmd.Parameters.AddWithValue("@SkipUntil", setting.SkipUntil ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@LastVerifiedAt", setting.LastVerifiedAt ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@CreatedAt", setting.CreatedAt);
                        cmd.Parameters.AddWithValue("@UpdatedAt", (object)setting.UpdatedAt ?? (object)DBNull.Value);

                        return await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ex.StackTrace);
            }
        }

        public async Task<WebsiteTwoFASetting> GetByUserIdAsync(string userId, string websiteName)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    var sql = @"SELECT TOP(1) * FROM WebsiteTwoFASetting WHERE UserId = @UserId AND WebsiteName = @WebsiteName";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@WebsiteName", websiteName);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new WebsiteTwoFASetting
                                {
                                    RowID = reader.GetGuid("RowID"),
                                    UserId = reader.GetString("UserId"),
                                    WebsiteName = reader.GetString("WebsiteName"),
                                    Email = reader.GetString("Email"),
                                    TwoFAEnabled = reader.GetBoolean("TwoFAEnabled"),
                                    SkipUntil = reader.IsDBNull(reader.GetOrdinal("SkipUntil")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("SkipUntil")),
                                    LastVerifiedAt = reader.IsDBNull(reader.GetOrdinal("LastVerifiedAt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("LastVerifiedAt")),
                                    CreatedAt = reader.GetDateTime("CreatedAt"),
                                    UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? DateTime.Now : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))

                                };
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ex.StackTrace);
            }
        }

        public async Task<int> UpdateByRowIdAsync(WebsiteTwoFASetting setting)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    var sql = @"UPDATE WebsiteTwoFASetting
                                SET 
                                    Email          = @Email,
                                    TwoFAEnabled   = @TwoFAEnabled,
                                    SkipUntil      = @SkipUntil,
                                    LastVerifiedAt = @LastVerifiedAt,
                                    UpdatedAt      = @UpdatedAt
                                WHERE RowID = @RowID";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", setting.Email);
                        cmd.Parameters.AddWithValue("@TwoFAEnabled", setting.TwoFAEnabled);
                        cmd.Parameters.AddWithValue("@SkipUntil", (object)setting.SkipUntil ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@LastVerifiedAt", (object)setting.LastVerifiedAt ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@UpdatedAt", setting.UpdatedAt);
                        cmd.Parameters.AddWithValue("@RowID", setting.RowID);

                        return await cmd.ExecuteNonQueryAsync();
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ex.StackTrace);
            }

        }

        public async Task<int> DeleteAsync(string rowId)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    var sql = @"DELETE WebsiteTwoFASetting WHERE RowID = @RowID";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@RowID", rowId);

                        return await cmd.ExecuteNonQueryAsync();
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ex.StackTrace);
            }
        }

        #region Auth_Logger
        public async Task<int> Auth_CreateAsync(WebsiteTwoFA_Auth_Logger logger)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    var sql = @"INSERT INTO WebsiteTwoFA_Auth_Logger 
                        VALUES (@AuthRowID, @System_Date, @UserId, @WebsiteName, @Email, @EmailLanguage, @TokenType, @CurrentToken, @TokenExpireAt, @TokenVerified)";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@AuthRowID", logger.AuthRowID);
                        cmd.Parameters.AddWithValue("@System_Date", logger.System_Date);
                        cmd.Parameters.AddWithValue("@UserId", logger.UserId);
                        cmd.Parameters.AddWithValue("@WebsiteName", logger.WebsiteName);
                        cmd.Parameters.AddWithValue("@Email", logger.Email);
                        cmd.Parameters.AddWithValue("@EmailLanguage", logger.EmailLanguage.ToString());
                        cmd.Parameters.AddWithValue("@TokenType", logger.TokenType.ToString());
                        cmd.Parameters.AddWithValue("@CurrentToken", logger.CurrentToken);
                        cmd.Parameters.AddWithValue("@TokenExpireAt", logger.TokenExpireAt);
                        cmd.Parameters.AddWithValue("@TokenVerified", logger.TokenVerified ?? (object)DBNull.Value);

                        return await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ex.StackTrace);
            }
        }

        public async Task<WebsiteTwoFA_Auth_Logger> Auth_GetLatestTokenAsync(string userId, string websiteName)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    var sql = @"SELECT TOP(1) * FROM WebsiteTwoFA_Auth_Logger WHERE UserId = @UserId AND WebsiteName = @WebsiteName ORDER BY System_Date DESC";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@WebsiteName", websiteName);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new WebsiteTwoFA_Auth_Logger
                                {
                                    AuthRowID = reader.GetGuid("AuthRowID"),
                                    System_Date = reader.GetDateTime("System_Date"),
                                    UserId = reader.GetString("UserId"),
                                    WebsiteName = reader.GetString("WebsiteName"),
                                    Email = reader.GetString("Email"),
                                    EmailLanguage = Enum.Parse<languageEnum>(reader.GetString("EmailLanguage")),
                                    TokenType = Enum.Parse<TokenTypeEnum>(reader.GetString("TokenType")),
                                    CurrentToken = reader.GetString("CurrentToken"),
                                    TokenExpireAt = reader.GetDateTime("TokenExpireAt"),
                                    TokenVerified = reader.IsDBNull(reader.GetOrdinal("TokenVerified")) ? (bool?)null : reader.GetBoolean(reader.GetOrdinal("TokenVerified")),

                                };
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ex.StackTrace);
            }
        }

        public async Task<int> Auth_UpdateVerifiedAsync(WebsiteTwoFA_Auth_Logger logger)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    var sql = @"UPDATE WebsiteTwoFA_Auth_Logger
                                SET 
                                    TokenVerified = @TokenVerified
                                WHERE AuthRowID = @AuthRowID";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@TokenVerified", logger.TokenVerified);
                        cmd.Parameters.AddWithValue("@AuthRowID", logger.AuthRowID);

                        return await cmd.ExecuteNonQueryAsync();
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ex.StackTrace);
            }
        }

        /// <summary>
        /// 刪除除了logger以外的Token資料，確保資料庫只留存最新一筆驗證紀錄。
        /// </summary>
        /// <param name="logger">需保留的紀錄</param>
        /// <param name=""></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<int> Auth_DeleteLegacyLoggerAsync(WebsiteTwoFA_Auth_Logger logger)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    var sql = @"DELETE WebsiteTwoFA_Auth_Logger
                                WHERE UserId = @UserId AND WebsiteName = @WebsiteName AND AuthRowID <> @AuthRowID";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", logger.UserId);
                        cmd.Parameters.AddWithValue("@WebsiteName", logger.WebsiteName);
                        cmd.Parameters.AddWithValue("@AuthRowID", logger.AuthRowID);

                        return await cmd.ExecuteNonQueryAsync();
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ex.StackTrace);
            }
        }
        #endregion

        #region Error_Logger
        public async Task<int> Error_WriteAsync(WebsiteTwoFAErrorLogger error)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    var sql = @"INSERT INTO WebsiteTwoFA_Error_Logger 
                        VALUES (@LogID, @System_Date, @UserId, @WebsiteName, @Event_Name, @Message)";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@LogID", error.LogID);
                        cmd.Parameters.AddWithValue("@System_Date", error.System_Date);
                        cmd.Parameters.AddWithValue("@UserId", error.UserId);
                        cmd.Parameters.AddWithValue("@WebsiteName", error.WebsiteName);
                        cmd.Parameters.AddWithValue("@Event_Name", error.EventName);
                        cmd.Parameters.AddWithValue("@Message", error.Message);

                        return await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ex.StackTrace);
            }
        }
        #endregion

        #region Trusted Device
        public async Task<bool> TD_IsTrustedAsync(Guid settingRowId, Guid deviceId, DateTime nowUtc)
        {
            try
            {
                if (settingRowId == Guid.Empty || deviceId == Guid.Empty) return false;

                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(@"
                SELECT TOP 1 1 FROM WebsiteTwoFA_TrustedDevice WITH (READPAST)
                WHERE Setting_RowID = @SettingRowID
                  AND DeviceId      = @DeviceId
                  AND IsRevoked     = 0
                  AND (RememberUntil IS NOT NULL AND RememberUntil > @NowUtc)", conn))
                {
                    cmd.Parameters.Add("@SettingRowID", SqlDbType.UniqueIdentifier).Value = settingRowId;
                    cmd.Parameters.Add("@DeviceId", SqlDbType.UniqueIdentifier).Value = deviceId;
                    cmd.Parameters.Add("@NowUtc", SqlDbType.DateTime).Value = nowUtc;

                    await conn.OpenAsync();
                    var o = await cmd.ExecuteScalarAsync();
                    return o != null;
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ex.StackTrace);
            }
        }

        public async Task<int> TD_UpsertAsync(Guid settingRowId, Guid deviceId, DateTime nowUtc, DateTime? rememberUntil, string ip, string ua)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(@"
                IF EXISTS (SELECT 1 FROM WebsiteTwoFA_TrustedDevice
                           WHERE Setting_RowID=@SettingRowID AND DeviceId=@DeviceId)
                BEGIN
                    UPDATE WebsiteTwoFA_TrustedDevice
                       SET LastSeenAt=@NowUtc,
                           LastIP=@IP,
                           LastUA=@UA,
                           RememberUntil = CASE WHEN @RememberUntil IS NOT NULL THEN @RememberUntil ELSE RememberUntil END,
                           IsRevoked=0, RevokedAt=NULL
                     WHERE Setting_RowID=@SettingRowID AND DeviceId=@DeviceId;
                END
                ELSE
                BEGIN
                    INSERT INTO WebsiteTwoFA_TrustedDevice
                      (Setting_RowID, DeviceId, RememberUntil,
                       FirstSeenAt, LastSeenAt, FirstIP, LastIP, FirstUA, LastUA, IsRevoked)
                    VALUES
                      (@SettingRowID, @DeviceId, @RememberUntil,
                       @NowUtc, @NowUtc, @IP, @IP, @UA, @UA, 0);
                END", conn))
                {
                    cmd.Parameters.Add("@SettingRowID", SqlDbType.UniqueIdentifier).Value = settingRowId;
                    cmd.Parameters.Add("@DeviceId", SqlDbType.UniqueIdentifier).Value = deviceId;
                    cmd.Parameters.Add("@RememberUntil", SqlDbType.DateTime).Value = (object)rememberUntil ?? DBNull.Value;
                    cmd.Parameters.Add("@NowUtc", SqlDbType.DateTime).Value = nowUtc;
                    cmd.Parameters.Add("@IP", SqlDbType.NVarChar, 45).Value = (object)ip ?? DBNull.Value;
                    cmd.Parameters.Add("@UA", SqlDbType.NVarChar, 512).Value = (object)ua ?? DBNull.Value;

                    await conn.OpenAsync();
                    return await cmd.ExecuteNonQueryAsync();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ex.StackTrace);
            }
        }

        /// <summary>
        /// 撤銷某一User白名單裝置
        /// </summary>
        /// <param name="settingRowId"></param>
        /// <param name="deviceId"></param>
        /// <param name="nowUtc"></param>
        /// <returns></returns>
        public async Task<int> TD_RevokeAsync(Guid settingRowId, Guid deviceId, DateTime nowUtc)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(@"
                UPDATE WebsiteTwoFA_TrustedDevice
                   SET IsRevoked=1, RevokedAt=@NowUtc
                 WHERE Setting_RowID=@SettingRowID AND DeviceId=@DeviceId", conn))
                {
                    cmd.Parameters.Add("@SettingRowID", SqlDbType.UniqueIdentifier).Value = settingRowId;
                    cmd.Parameters.Add("@DeviceId", SqlDbType.UniqueIdentifier).Value = deviceId;
                    cmd.Parameters.Add("@NowUtc", SqlDbType.DateTime).Value = nowUtc;

                    await conn.OpenAsync();
                    return await cmd.ExecuteNonQueryAsync();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ex.StackTrace);
            }
        }

        /// <summary>
        /// 撤銷全部白名單裝置
        /// </summary>
        /// <param name="settingRowId"></param>
        /// <param name="nowUtc"></param>
        /// <returns></returns>
        public async Task<int> TD_RevokeAllAsync(Guid settingRowId, DateTime nowUtc)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(@"
                UPDATE WebsiteTwoFA_TrustedDevice
                   SET IsRevoked=1, RevokedAt=@NowUtc
                 WHERE Setting_RowID=@SettingRowID", conn))
                {
                    cmd.Parameters.Add("@SettingRowID", SqlDbType.UniqueIdentifier).Value = settingRowId;
                    cmd.Parameters.Add("@NowUtc", SqlDbType.DateTime).Value = nowUtc;

                    await conn.OpenAsync();
                    return await cmd.ExecuteNonQueryAsync();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ex.StackTrace);
            }
        }

        #endregion

        #endregion

        #region Static function
        private const string BASECODE = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        static Random ranNum = new Random();
        public static string GenerateCurrentToken(int length)
        {
            if (length <= 0)
                throw new ArgumentException("Token length must be greater than 0.");

            StringBuilder token = new StringBuilder(length);
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] randomBytes = new byte[4];
                for (int i = 0; i < length; i++)
                {
                    rng.GetBytes(randomBytes);
                    int randIndex = BitConverter.ToInt32(randomBytes, 0) & int.MaxValue; // 轉成正數
                    randIndex %= BASECODE.Length; // 限制索引範圍
                    token.Append(BASECODE[randIndex]);
                }
            }

            return token.ToString();
        }

        #endregion

        #region Send Email
        public async Task SendAuthEmailAsync(WebsiteTwoFA_Auth_Logger auth_token)
        {
            try
            {
                string mailFrom = "Support@yourdomain.com";
                var builder = new ConfigurationBuilder()
                  .SetBasePath(Directory.GetCurrentDirectory())
                  .AddJsonFile("appsettings.json");
                var config = builder.Build().GetSection("MailServer");
                var smtpHost = config["SMTPHOST"];
                var smtpUser = config["SMTPUser"];
                var smtpPwd = config["SMTPPassword"];
                var smtpPort = config["SMTPPort"];

                string subject = "";
                StringBuilder body = new StringBuilder();

                // 後續可調整成語系檔配置
                switch (auth_token.EmailLanguage)
                {
                    case languageEnum.en_US:
                        subject = String.Format("{0} System Login Verification Code", auth_token.WebsiteName);
                        body.Append("Dear ")
                            .Append(auth_token.UserId)
                            .Append(",")
                            .Append("<br/><br/>");
                        body.Append("Here is your ")
                            .Append(auth_token.WebsiteName)
                            .Append(" verification code to log in to your account ")
                            .Append(auth_token.UserId)
                            .Append(":<br/>");
                        body.Append("<span style=\"font-size:48px;line-height:52px;font-family:Arial,sans-serif,'Motiva Sans';color:#3a9aed;font-weight:bold;text-align:center;\">")
                            .Append(auth_token.CurrentToken)
                            .Append("</span><br/>");

                        body.Append("<p style=\"font-weight:bold;\">").Append("This code is valid for 3 minutes.").Append("</p>");
                        body.Append("<p>This email was generated because a login attempt was made from a web or mobile device. The login attempt included your correct account name and password.</p>");
                        body.Append("<p>The ")
                            .Append(auth_token.WebsiteName)
                            .Append(" verification code is required to complete the login. Your account cannot be accessed without this code.</p> ");
                        body.Append("<div style=\"background-color:#fff3cd;border:1px solid #f5c2c7;padding:12px;border-radius:6px;margin:15px 0;\">")
                            .Append("<p style=\"color:#b02a37;font-weight:bold;font-size:14px;margin:0;\">Do not share this verification code with anyone. Your account security depends on it!</p>")
                            .Append("</div>");
                        body.Append("<p>If you are not attempting to log in then please change your ")
                            .Append(auth_token.WebsiteName)
                            .Append(" password to ensure your account security.</p>");
                        body.Append("<br/><br/>Cheers,<br/><span style=\"font-weight:bold;color:#0d6efd;\">The MIS Team</span>");
                        break;
                    case languageEnum.vi_VN:
                        subject = String.Format("{0} Mã xác minh đăng nhập hệ thống", auth_token.WebsiteName);
                        body.Append("Kính gửi người dùng ")
                            .Append(auth_token.UserId)
                            .Append("，")
                            .Append("<br/><br/>");
                        body.Append("Sau đây là mã xác minh để đăng nhập vào tài khoản ")
                            .Append(auth_token.UserId)
                            .Append(" của bạn trong ")
                            .Append(auth_token.WebsiteName)
                            .Append(" <br/><br/>");
                        body.Append("<span style=\"font-size:48px;line-height:52px;font-family:Arial,sans-serif,'Motiva Sans';color:#3a9aed;font-weight:bold;text-align:center;\">")
                            .Append(auth_token.CurrentToken)
                            .Append("</span><br/>");
                        body.Append("<p style=\"font-weight:bold;\">").Append("Mã xác minh này có hiệu lực trong 3 phút").Append("</p>");
                        body.Append("<p>Email này là do một thiết bị đã cố gắng đăng nhập vào tài khoản của bạn bằng tên người dùng và mật khẩu chính xác, do đó hệ thống yêu cầu xác minh bổ sung.</p>")
                            .Append("<P>Mã xác minh này là thông tin bắt buộc. Nếu bạn không nhập đúng mã xác minh, bạn sẽ không thể hoàn tất đăng nhập.</P>");
                        body.Append("<div style=\"background-color:#fff3cd;border:1px solid #f5c2c7;padding:12px;border-radius:6px;margin:15px 0;\">")
                            .Append("<p style=\"color:#b02a37;font-weight:bold;font-size:14px;margin:0;\">Vui lòng không cung cấp mã xác minh này cho bất kỳ ai để đảm bảo an toàn cho tài khoản của bạn!</p>")
                            .Append("</div>");

                        body.Append("<p>Nếu bạn chưa thử đăng nhập, bạn nên thay đổi mật khẩu <b>")
                            .Append(auth_token.WebsiteName)
                            .Append("</b> ngay lập tức để đảm bảo an toàn cho tài khoản.</p>");

                        body.Append("<br/><br/>Trân trọng<br/><span style=\"font-weight:bold;color:#0d6efd;\">Đội MIS</span>");
                        break;
                    default:
                        subject = String.Format("{0} 系統登入驗證碼", auth_token.WebsiteName);
                        body.Append("親愛的使用者 ")
                            .Append(auth_token.UserId)
                            .Append(" 您好，")
                            .Append("<br/><br/>");
                        body.Append("以下是您在 ")
                            .Append(auth_token.WebsiteName)
                            .Append(" 登入帳號 ")
                            .Append(auth_token.UserId)
                            .Append(" 時所需的驗證碼：<br/><br/>");
                        body.Append("<span style=\"font-size:48px;line-height:52px;font-family:Arial,sans-serif,'Motiva Sans';color:#3a9aed;font-weight:bold;text-align:center;\">")
                            .Append(auth_token.CurrentToken)
                            .Append("</span><br/>");
                        body.Append("<p style=\"font-weight:bold;\">").Append("此驗證碼有效期限為3分鐘").Append("</p>");
                        body.Append("<p>這封信是因為有裝置嘗試使用正確的帳號與密碼登入您的帳號，因此系統要求額外驗證。</p>")
                            .Append("<P>此驗證碼為必填資訊，若沒有輸入正確的驗證碼，將無法完成登入。</P>");
                        body.Append("<div style=\"background-color:#fff3cd;border:1px solid #f5c2c7;padding:12px;border-radius:6px;margin:15px 0;\">")
                            .Append("<p style=\"color:#b02a37;font-weight:bold;font-size:14px;margin:0;\">請勿將此驗證碼提供給任何人，以確保帳號安全！</p>")
                            .Append("</div>");

                        body.Append("<p>若您並未嘗試登入，建議立即修改 <b>")
                            .Append(auth_token.WebsiteName)
                            .Append("</b> 的密碼，以保障帳號安全。</p>");

                        body.Append("<br/><br/>此致<br/><span style=\"font-weight:bold;color:#0d6efd;\">MIS 團隊敬上</span>");
                        break;
                }

                await SendMailAsync(new string[] { auth_token.Email }, null, null, mailFrom, subject, body.ToString(), smtpHost, int.Parse(smtpPort), smtpUser, smtpPwd, null);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public async Task SendMailAsync(string[] tos, string[] Ccs, string[] bcc, string from, string subject, string body, string smtpHost, int smtpPort, string userName, string password, Attachment file)
        {
            try
            {

                using (MailMessage Message = new MailMessage())
                {
                    Message.Subject = subject;
                    Message.IsBodyHtml = true;
                    Message.Body = body;
                    Message.From = new MailAddress(from);

                    if (tos != null)
                    {
                        foreach (var to in tos.Where(t => !string.IsNullOrWhiteSpace(t)))
                            Message.To.Add(new MailAddress(to.Trim()));
                    }
                    if (Ccs != null)
                    {
                        foreach (var cc in Ccs.Where(c => !string.IsNullOrWhiteSpace(c)))
                            Message.CC.Add(new MailAddress(cc.Trim()));
                    }
                    if (bcc != null)
                    {
                        foreach (var b in bcc.Where(bx => !string.IsNullOrWhiteSpace(bx)))
                            Message.Bcc.Add(new MailAddress(b.Trim()));
                    }
                    if (file != null)
                    {
                        Message.Attachments.Add(file);
                    }

                    using (SmtpClient client = new SmtpClient(smtpHost, smtpPort))
                    {
                        client.EnableSsl = true;
                        client.Credentials = new NetworkCredential(userName, password);
                        await client.SendMailAsync(Message);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("SMTP Error: " + ex.Message);
            }
        }

        #endregion

        #region Service
        public async Task<WebsiteTwoFASetting> CheckSettingsAsync(WebsiteTwoFASetting _2FARequest, WebsiteTwoFASetting twoFASetting, DateTime timeNow)
        {
            try
            {
                bool isNewSetting = false;
                if (twoFASetting == null)
                {
                    isNewSetting = true;
                    twoFASetting = new WebsiteTwoFASetting
                    {
                        RowID = Guid.NewGuid(),
                        UserId = _2FARequest.UserId,
                        WebsiteName = _2FARequest.WebsiteName,
                        TwoFAEnabled = true,
                        LastVerifiedAt = null,
                        CreatedAt = timeNow

                    };
                }
                twoFASetting.Email = _2FARequest.Email;

                if (isNewSetting)
                {
                    await CreateAsync(twoFASetting);
                }
                else
                {
                    twoFASetting.UpdatedAt = timeNow;
                    await UpdateByRowIdAsync(twoFASetting);
                }

            }
            catch (Exception ex)
            {
                await Error_WriteAsync(new WebsiteTwoFAErrorLogger(_2FARequest.UserId, _2FARequest.WebsiteName, ex.Message, ex.StackTrace));
                return null;
            }
            return twoFASetting;
        }

        /// <summary>
        /// 若該ID不須驗證則回傳Null
        /// </summary>
        /// <param name="_2FARequest"></param>
        /// <returns></returns>
        public async Task<WebsiteTwoFA_Auth_Logger> Generate2FATokenAsync(WebsiteTwoFASetting twoFASetting, string mailLanguage, DateTime timeNow)
        {
            WebsiteTwoFA_Auth_Logger auth = null;
            try
            {
                if (twoFASetting != null && twoFASetting.TwoFAEnabled)
                {
                    if (twoFASetting.SkipUntil == null || (timeNow < twoFASetting.SkipUntil))
                    {
                        string token = TwoFARepository.GenerateCurrentToken(6);

                        auth = new WebsiteTwoFA_Auth_Logger();
                        auth.System_Date = timeNow;
                        auth.UserId = twoFASetting.UserId;
                        auth.WebsiteName = twoFASetting.WebsiteName;
                        auth.Email = twoFASetting.Email;
                        auth.EmailLanguage = mailLanguage != null ? Enum.Parse<languageEnum>(mailLanguage) : languageEnum.zh_TW;// 預設發中文信
                        auth.TokenType = TokenTypeEnum.Email;
                        auth.CurrentToken = token;
                        auth.TokenExpireAt = timeNow.AddMinutes(3); // 3分鐘到期
                        await Auth_CreateAsync(auth);
                    }
                }

                return auth;
            }
            catch (Exception ex)
            {
                await Error_WriteAsync(new WebsiteTwoFAErrorLogger(twoFASetting.UserId, twoFASetting.WebsiteName, ex.Message, ex.StackTrace));
                return null;
            }
        }

        #endregion
    }
}