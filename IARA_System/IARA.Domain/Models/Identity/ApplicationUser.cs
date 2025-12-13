// IARA.Domain/Models/Identity/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;

namespace IARA.Domain.Models.Identity
{
    public class ApplicationUser : IdentityUser
    {
        // Разширени свойства (ако са необходими)
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Връзки с други модели (optional)
        public int? FisherId { get; set; }
        public virtual Fisher? Fisher { get; set; }
        
        public int? InspectorId { get; set; }
        public virtual Inspector? Inspector { get; set; }
    }
}