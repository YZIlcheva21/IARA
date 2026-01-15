using IARA.Domain.Models.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema; // 👈 1. ВАЖНО: Добавихме това за атрибутите

namespace IARA.Domain.Models
{
    public class Inspector
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string BadgeNumber { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public DateTime? HireDate { get; set; }
        public bool IsActive { get; set; } = true;

        // Външен ключ към таблицата с потребители
        public string? UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационни свойства
        public virtual ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();

        // 👇 2. ВАЖНО: Този ред оправя грешката "Unable to determine relationship"
        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }
    }
}