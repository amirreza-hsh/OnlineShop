using Microsoft.AspNetCore.Identity;

namespace OnlineShop.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}