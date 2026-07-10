// Security/PasswordHasher.cs
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace DigitalProject.Security
{
    /// <summary>
    /// Argon2id 密碼雜湊器
    /// 儲存格式：{salt_hex}:{hash_hex}，直接存入 User.PasswordHash 欄位
    /// </summary>
    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16;    // 128 bit
        private const int HashSize = 32;    // 256 bit
        private const int Iterations = 4;
        private const int MemorySize = 65536;  // 64 MB
        private const int DegreeOfParallelism = 4;

        // Hash 密碼（註冊用）
        // 格式：{salt hex}.{hash hex}  → 兩段都存在同一個字串裡
        public string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = ComputeHash(password, salt);
            return $"{Convert.ToHexString(salt)}.{Convert.ToHexString(hash)}";
        }

        // 驗證密碼（登入用）
        public bool Verify(string password, string storedHash)
        {
            var parts = storedHash.Split('.');
            if (parts.Length != 2) return false;

            var salt = Convert.FromHexString(parts[0]);
            var expectedHash = parts[1];
            var actualHash = Convert.ToHexString(ComputeHash(password, salt));

            // 使用 FixedTimeEquals 防止 timing attack
            return CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(expectedHash),
                Convert.FromHexString(actualHash)
            );
        }

        private static byte[] ComputeHash(string password, byte[] salt)
        {
            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = DegreeOfParallelism,
                Iterations = Iterations,
                MemorySize = MemorySize,
            };
            return argon2.GetBytes(HashSize);
        }
    }
}