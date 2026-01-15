namespace IARA.Domain.Models
{
    public class AmateurCatch
    {
        public int Id { get; set; }

        // 👇 1. Сложи въпросителна тук (ако вече не си)
        public int? AmateurTicketId { get; set; }

        // 👇 2. ВАЖНО: Добави UserId, за да знаем чия е рибата
        public string UserId { get; set; } = string.Empty;

        public DateTime CatchDate { get; set; }

        // 👇 3. Увери се, че името е FishSpecies (а не FishType)
        public string FishSpecies { get; set; } = string.Empty;

        public decimal? WeightKgs { get; set; }
        public int? Quantity { get; set; }
        public string? FishingLocation { get; set; }
        public string? FishingMethod { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 👇 4. Сложи въпросителна и тук! Това казва на валидатора: "Не ми трябва обект Билет при запис"
        public virtual AmateurTicket? AmateurTicket { get; set; }
    }
}