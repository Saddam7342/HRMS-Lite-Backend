using HRMS.Application.Common.Interfaces;
using BC = BCrypt.Net.BCrypt;

namespace HRMS.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        return BC.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
            return false;
            
        return BC.Verify(password, hashedPassword);
    }
}
