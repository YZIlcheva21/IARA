// IARA.Domain/Models/License.cs
namespace IARA.Domain.Models
{
    public class License
    {
        public int Id { get; set; }
        public string LicenseNumber { get; set; } // Номер на разрешителното
        public int FisherId { get; set; } // Рибар-собственик
        public int? ShipId { get; set; } // Кораб (може да е null)
        public DateTime IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Status { get; set; } = "Active"; // Active, Expired, Revoked, Suspended
        public string? LicenseType { get; set; } // Вид разрешително
        public string? IssuingAuthority { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационни свойства
        public virtual Fisher Fisher { get; set; }
        public virtual Ship? Ship { get; set; }
        public virtual ICollection<LogbookEntry> LogbookEntries { get; set; } = new List<LogbookEntry>();
        public virtual ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();
    }
}