namespace BLL.Abstractions.Utilities
{
    public interface IPasswordManagerUtility
    {
        (string hashedPassword, string salt) HashPassword(string password);
        bool VerifyPasswordHash(string password, string storedHash, string storedSalt);
    }
}
