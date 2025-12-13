// IARA.Domain/Models/Inspection.cs - КОРИГИРАНА ВЕРСИЯ
namespace IARA.Domain.Models
{
    public class Inspection
    {
        public int Id { get; set; } // ✅ Добавено
        public int? InspectorId { get; set; } // ✅ Добавено
        public int? ShipId { get; set; } // ✅ Добавено
        public int? LicenseId { get; set; } // ✅ Добавено
        public DateTime InspectionDate { get; set; } // ✅ Добавено
        public string InspectionType { get; set; } = "Planned"; // ✅ Добавено
        public string? Location { get; set; } // ✅ Добавено
        public string? Findings { get; set; } // ✅ Добавено
        public string? Violations { get; set; } // ✅ Добавено
        public string? ActionsTaken { get; set; } // ✅ Добавено
        public string Status { get; set; } = "Planned"; // ✅ Добавено
        public string? Notes { get; set; } // ✅ Добавено
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационни свойства
        public virtual Inspector? Inspector { get; set; }
        public virtual Ship? Ship { get; set; }
        public virtual License? License { get; set; }
    }
}