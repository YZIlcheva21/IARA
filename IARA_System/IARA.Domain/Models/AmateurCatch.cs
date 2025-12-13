// IARA.Domain/Models/AmateurCatch.cs
namespace IARA.Domain.Models
{
    public class AmateurCatch
    {
        public int Id { get; set; }
        public int AmateurTicketId { get; set; }
        public DateTime CatchDate { get; set; }
        public string FishSpecies { get; set; }
        public decimal? WeightKgs { get; set; }
        public int? Quantity { get; set; }
        public string? FishingLocation { get; set; }
        public string? FishingMethod { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационни свойства
        public virtual AmateurTicket AmateurTicket { get; set; }
    }
}