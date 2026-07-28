using Microsoft.AspNetCore.Identity;

namespace SweetFlowerShop.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public Guid? CustomerId { get; set; }
}
