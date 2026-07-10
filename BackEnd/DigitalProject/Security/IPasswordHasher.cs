using System.Security.Cryptography;

namespace DigitalProject.Security
{
    public interface IPasswordHasher
    {
        // 產生隨機鹽值並雜湊密碼（註冊用）
        // 輸入: password = 明文密碼
        // 輸出: salt.hash 格式的雜湊字串
        string Hash(string password);
        // 驗證密碼（登入用）
        // 輸入: password = 使用者輸入的明文密碼, storedHash = 資料庫存的雜湊字串
        // 輸出: 驗證成功回傳 true，失敗回傳 false
        bool Verify(string password, string hash);


    }
}