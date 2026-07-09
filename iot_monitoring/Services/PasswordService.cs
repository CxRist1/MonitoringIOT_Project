using System.Security.Cryptography;
using System.Text;

namespace iot_monitoring.Services
{
    public class PasswordService
    {
        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();

            byte[] bytes = Encoding.UTF8.GetBytes(password);
            byte[] hash = sha256.ComputeHash(bytes);

            return Convert.ToBase64String(hash);
        }
        public bool VerifyPassword(string password, string passwordHash)
        {
            string hash = HashPassword(password);

            return hash == passwordHash;
        }
    }
}
