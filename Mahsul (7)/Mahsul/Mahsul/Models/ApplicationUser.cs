using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public string Address { get; set; }
}
