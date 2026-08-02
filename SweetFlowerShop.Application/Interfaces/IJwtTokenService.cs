namespace SweetFlowerShop.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(Guid userId, Guid? customerId, string email, IEnumerable<string> roles);
}
