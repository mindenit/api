using Microsoft.AspNetCore.Identity;

namespace Api.Classes;

public class AuthUser: IdentityUser
{
    public List<string>? Schedules { get; set; }
}