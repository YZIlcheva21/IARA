// IARA.Domain/Models/Fisher.cs
namespace IARA.Domain.Models
{
    public class Fisher
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PersonalNumber { get; set; } // ЕГН или идентификатор
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Навигационни свойства
        public virtual ICollection<Ship> Ships { get; set; } = new List<Ship>();
        public virtual ICollection<License> Licenses { get; set; } = new List<License>();
        public virtual ICollection<AmateurTicket> AmateurTickets { get; set; } = new List<AmateurTicket>();
    }
}