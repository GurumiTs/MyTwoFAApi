# MyTwoFAApi

Email二次驗證API程式碼。
Controller包含建立當前Token及驗證Token。
內含裝置紀錄功能。

必要功能:
1. MsSQL Database
2. SMTP Server

使用方式:
1. 於MsSQL DB中執行TwoFA_SQLTable.sql，並創建必要的SQL Table。
2. 調整Appsettings.json中SQL DB連線字串。 
3. 調整Appsettings.json中的SMTP資訊。
4. 將網站IP替換到Startup.cs中的Domain位置。

## 免責聲明 / Disclaimer

本專案/程式碼僅供學習與研究之用，**使用風險請自行承擔**。作者/貢獻者對因使用本專案所造成的任何直接、間接、偶發、特殊或衍生之損害（包含但不限於資料遺失、系統故障、商業中斷或其他損失）**概不負責**。本專案以 **「現狀（AS IS）」** 提供，不附帶任何明示或暗示之保證，包括但不限於適銷性、特定目的適用性與非侵權之保證。  
使用者應自行確保遵守所在國家/地區之法律法規與相關第三方服務條款。若您不同意上述條款，請勿使用本專案。


This project/code is provided **for learning and research purposes only** and is used **at your own risk**. The author(s)/contributors **shall not be liable** for any direct, indirect, incidental, special, or consequential damages arising from the use of this project, including but not limited to data loss, system failure, business interruption, or other losses. The project is provided **“AS IS”**, **without warranty of any kind**, express or implied, including but not limited to warranties of merchantability, fitness for a particular purpose, and noninfringement.  
You are responsible for complying with applicable laws and any third-party terms. If you do not agree with these terms, do not use this project.
