// IARA.Domain/Models/Identity/UserRole.cs
using Microsoft.AspNetCore.Identity;

namespace IARA.Domain.Models.Identity
{
    public class UserRole : IdentityRole
    {
        // Разширени свойства (ако са необходими)
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}