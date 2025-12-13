// IARA.Domain/Models/LogbookEntry.cs
namespace IARA.Domain.Models
{
    public class LogbookEntry
    {
        public int Id { get; set; }
        public int LicenseId { get; set; }
        public DateTime FishingDate { get; set; } // Дата на риболов
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string? FishingArea { get; set; } // Риболовна зона
        public decimal? FuelConsumptionLiters { get; set; }
        public decimal? DistanceTraveled { get; set; }
        public string? WeatherConditions { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационни свойства
        public virtual License License { get; set; }
        public virtual ICollection<CatchDetail> CatchDetails { get; set; } = new List<CatchDetail>();
    }
}