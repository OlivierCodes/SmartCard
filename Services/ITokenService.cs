using SmartCard.Models;

namespace SmartCard.Services
{
    public interface ITokenService
    {
        string GenerateToken(ApplicationUser user);
    }
}