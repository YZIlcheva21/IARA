// IARA.Domain/Models/AmateurTicket.cs
namespace IARA.Domain.Models
{
    public class AmateurTicket
    {
        public int Id { get; set; }
        public int FisherId { get; set; }
        public string TicketNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Status { get; set; } = "Active";
        public string? IssuingAuthority { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационни свойства
        public virtual Fisher Fisher { get; set; }
        public virtual ICollection<AmateurCatch> AmateurCatches { get; set; } = new List<AmateurCatch>();
    }
}