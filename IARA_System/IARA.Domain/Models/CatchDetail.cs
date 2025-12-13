// IARA.Domain/Models/CatchDetail.cs
using System;

namespace IARA.Domain.Models
{
    public class CatchDetail
    {
        public int Id { get; set; }
        public int LogbookEntryId { get; set; }
        public string FishSpecies { get; set; } = string.Empty;
        public decimal? WeightKgs { get; set; } // Килограми улов
        public int? Quantity { get; set; } // Брой риби
        public string? FishingGear { get; set; } // Риболовен уред
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационни свойства
        public virtual LogbookEntry LogbookEntry { get; set; }
    }
}