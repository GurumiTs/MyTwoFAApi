using MyApi.Attributes;
using MyApi.Repository;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace MyApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [InternalAuthorize]
    [EnableCors("TwoFA-Cors")]
    public class TwoFAController : ControllerBase
    {
        private readonly ITwoFARepository _repo;

        public TwoFAController(ITwoFARepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// 參數必須包含UserId, WebsiteName, Email, Lan(zh_TW, en_US)
        /// </summary>
        /// <param name="_2FARequest"></param>
        /// <param name="Lan">zh_TW, en_US</param>
        /// <param name="IsTest">若不需要寄信(測試區)則需發送"Y"，其餘可不傳遞此參數</param>
        /// <returns></returns>
        [HttpPost("Generate2FA")]
        [Consumes("application/json")]
        public async Task<IActionResult> Generate2FA([FromBody] WebsiteTwoFASetting _2FARequest, string Lan, string IsTest)
        {
            if (string.IsNullOrWhiteSpace(_2FARequest.UserId) || string.IsNullOrWhiteSpace(_2FARequest.WebsiteName) || string.IsNullOrEmpty(_2FARequest.Email))
                return BadRequest("Invalid request. Please check the parameters.");

            _2FARequest.WebsiteName = MatchEnumString<WebsiteNameEnum>(_2FARequest.WebsiteName);
            if (string.IsNullOrWhiteSpace(_2FARequest.WebsiteName))
                return BadRequest("Invalid request. Please check the parameters.");

            try
            {
                DateTime timeNow = DateTime.UtcNow;

                // 1. 全域帳號層判斷
                WebsiteTwoFASetting twoFASetting = await _repo.GetByUserIdAsync(_2FARequest.UserId, _2FARequest.WebsiteName);
                if (twoFASetting != null)
                {
                    // 帳號不須驗證
                    if (!twoFASetting.TwoFAEnabled)
                        return Ok(new _2FAReturnContent
                        {
                            APISuccess = true,
                            UserId = twoFASetting.UserId,
                            WebsiteName = twoFASetting.WebsiteName,
                            SkipUntil = twoFASetting.SkipUntil,
                            NeedVerify = twoFASetting.TwoFAEnabled,
                            AuthInfo = null,
                            Msg = "TwoFA disabled globally"
                        });

                    // 帳號層7天免驗證
                    if (twoFASetting.SkipUntil.HasValue && twoFASetting.SkipUntil > timeNow)
                        return Ok(new _2FAReturnContent
                        {
                            APISuccess = true,
                            UserId = twoFASetting.UserId,
                            WebsiteName = twoFASetting.WebsiteName,
                            SkipUntil = twoFASetting.SkipUntil,
                            NeedVerify = false,
                            AuthInfo = null,
                            Msg = "Account skip until"
                        });
                }

                // 2. 裝置層判斷: Cookie -> GUID; 需要Setting.RowID
                twoFASetting = await _repo.CheckSettingsAsync(_2FARequest, twoFASetting, timeNow); // Update user setting.
                if (twoFASetting != null && TryGetDeviceIdFromCookie(out var deviceId))
                {
                    var trusted = await _repo.TD_IsTrustedAsync(twoFASetting.RowID, deviceId, timeNow);
                    if (trusted) 
                        return Ok(new _2FAReturnContent
                        {
                            APISuccess = true,
                            UserId = twoFASetting.UserId,
                            WebsiteName = twoFASetting.WebsiteName,
                            SkipUntil = twoFASetting.SkipUntil,
                            NeedVerify = false,
                            AuthInfo = null,
                            Msg = "Trusted device"
                        });
                }

                // 3. 產驗證碼/寄信流程
                WebsiteTwoFA_Auth_Logger auth_token = await _repo.Generate2FATokenAsync(twoFASetting, MatchEnumString<languageEnum>(Lan), timeNow);
                if (auth_token != null)
                {
                    if (string.IsNullOrWhiteSpace(IsTest) || IsTest.ToUpper() != "Y")
                    {
                        await _repo.SendAuthEmailAsync(auth_token);
                    }
                }

                return Ok(new _2FAReturnContent
                {
                    APISuccess = true,
                    UserId = auth_token.UserId,
                    WebsiteName = auth_token.WebsiteName,
                    NeedVerify = auth_token != null,
                    AuthInfo = auth_token
                });
            }
            catch (Exception ex)
            {
                await _repo.Error_WriteAsync(new WebsiteTwoFAErrorLogger(_2FARequest.UserId, _2FARequest.WebsiteName, ex.Message, ex.StackTrace));

                return BadRequest(new _2FAReturnContent
                {
                    APISuccess = false,
                    UserId = _2FARequest.UserId,
                    WebsiteName = _2FARequest.WebsiteName,
                    Msg = ex.Message
                });
            }
        }

        [HttpPost("VerifiedEmail2FA")]
        [Consumes("application/json")]
        public async Task<IActionResult> VerifiedEmail2FA([FromBody] Verify2FARequest model)
        {
            if (string.IsNullOrWhiteSpace(model.UserId) || string.IsNullOrWhiteSpace(model.WebsiteName))
                return BadRequest("Invalid request. Please check the parameters.");

            model.WebsiteName = MatchEnumString<WebsiteNameEnum>(model.WebsiteName);
            if (string.IsNullOrWhiteSpace(model.WebsiteName))
                return BadRequest("Invalid request. Please check the parameters.");

            string message = "";
            var content = new _2FAReturnContent();
            try
            {
                DateTime timeNow = DateTime.UtcNow;

                // 1. 找到Setting.RowID (帳號層)
                var setting = await _repo.GetByUserIdAsync(model.UserId, model.WebsiteName);
                if (setting == null) return BadRequest("TwoFA setting not found.");
                
                // 2. 取得最新驗證碼
                WebsiteTwoFA_Auth_Logger auth_token = null;
                auth_token = await _repo.Auth_GetLatestTokenAsync(model.UserId, model.WebsiteName);
                if (auth_token != null)
                {
                    if (DateTime.UtcNow <= auth_token.TokenExpireAt)
                    {
                        if (auth_token.CurrentToken.ToUpper() == model.CurrentToken.ToUpper())
                        {
                            // 3. 驗證成功，更新驗證Logger及移除舊的logger
                            auth_token.TokenVerified = true;
                            await _repo.Auth_UpdateVerifiedAsync(auth_token);
                            await _repo.Auth_DeleteLegacyLoggerAsync(auth_token);

                            // 4. 預設7天免驗證
                            #region 帳號層免驗證(因新增裝置判定，故不再操作帳號層免驗證機制，但仍可透過資料庫修改來調整個案帳號免驗證)
                            //WebsiteTwoFASetting twoFASetting = await _repo.GetByUserIdAsync(auth_token.UserId, auth_token.WebsiteName);
                            //if (twoFASetting != null)
                            //{
                            //    twoFASetting.SkipUntil = timeNow.AddDays(7);
                            //    await _repo.UpdateByRowIdAsync(twoFASetting);
                            //    content.SkipUntil = timeNow.AddDays(7);
                            //}
                            #endregion

                            #region 裝置層免驗證(白名單)
                            if (Convert.ToBoolean(model.TrustThisDevice))
                            {
                                if (!TryGetDeviceIdFromCookie(out var deviceId) || deviceId == Guid.Empty)
                                {
                                    deviceId = Guid.NewGuid();
                                    SetDeviceCookie(deviceId); // 將DeviceId寫回 Cookie, 之後請求會自動帶入
                                }
                                
                                string ip = Request.Headers["X-Forwarded-For"].ToString() ?? string.Empty;
                                string ua = Request.Headers["User-Agent"].ToString() ?? string.Empty;

                                await _repo.TD_UpsertAsync(setting.RowID, deviceId, timeNow, timeNow.AddDays(7), ip, ua);
                                content.SkipUntil = timeNow.AddDays(7);
                            }
                            #endregion

                        }
                        else
                        {
                            message = "Wrong token";
                        }
                    }
                    else
                    {
                        message = "Token expired";
                    }
                }
                else
                {
                    message = "Token expired";
                }

                content.APISuccess = true;
                content.UserId = model.UserId;
                content.WebsiteName = model.WebsiteName;
                content.VerifiedStatus = string.IsNullOrEmpty(message) ? auth_token.TokenVerified : null;
                content.AuthInfo = auth_token;
                content.Msg = message;
                
                return Ok(content);
            }
            catch (Exception ex)
            {
                await _repo.Error_WriteAsync(new WebsiteTwoFAErrorLogger(model.UserId, model.WebsiteName, ex.Message, ex.StackTrace));
                return BadRequest(new _2FAReturnContent
                {
                    APISuccess = false,
                    UserId = model.UserId,
                    WebsiteName = model.WebsiteName,
                    Msg = ex.Message
                });
            }
        }

        #region Public Function
        public static string MatchEnumString<TEnum>(string input) where TEnum : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            // 轉成大寫後比對 Enum Key
            foreach (var name in Enum.GetNames(typeof(TEnum)))
            {
                if (name.ToUpper() == input.Trim().ToUpper())
                {
                    return name; // 回傳 Enum 的字串名稱
                }
            }

            return null; // 沒找到
        }

        private bool TryGetDeviceIdFromCookie(out Guid deviceId)
        {
            deviceId = Guid.Empty;
            var raw = Request.Cookies["EnnoEmailTwoFA_device_id"];
            if (string.IsNullOrWhiteSpace(raw)) return false;
            return Guid.TryParse(raw, out deviceId);
        }

        private void SetDeviceCookie(Guid deviceId)
        {
            // 若要跨子網域共用，把 Domain 換成你的母網域：.yourdomain.com
            Response.Cookies.Append("EnnoEmailTwoFA_device_id", deviceId.ToString("D"), new CookieOptions
            {
                //Domain = ".yourdomain.com",  // ← 依你環境調整；不跨子網域可移除
                Path = "/",
                HttpOnly = true,
                //Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddYears(1)
            });
        }

        #endregion

    }
}
