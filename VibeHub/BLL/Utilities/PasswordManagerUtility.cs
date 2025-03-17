using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Utilities
{
    public static class PasswordManagerUtility
    {
        public static (string hashedPassword, string salt) HashPassword(string password)
        {
            using var hmac = new HMACSHA512();
            var salt = Convert.ToBase64String(hmac.Key);
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var hashedPassword = Convert.ToBase64String(hmac.ComputeHash(passwordBytes));

            return (hashedPassword, salt);
        }

        public static bool VerifyPasswordHash(string password , string storedHash, string storedSalt)
        {
            var saltBytes = Convert.FromBase64String(storedSalt);
            using var hmac = new HMACSHA512(saltBytes);
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var computedHash = Convert.ToBase64String(hmac.ComputeHash(passwordBytes));

            return computedHash == storedHash;
        }
    }
}
