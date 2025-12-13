// IARA.Domain/Models/Ship.cs
using System;
using System.Collections.Generic;

namespace IARA.Domain.Models
{
    public class Ship
    {
        public int Id { get; set; }
        
        // Basic identification
        public string Name { get; set; }
        public string InternationalNumber { get; set; } // Международен номер (IMO)
        public string? CallSign { get; set; } // Позивна (от заданието)
        public string? Marking { get; set; } // Маркировка (от заданието)
        public string? RegistrationNumber { get; set; } // Регистрационен номер
        
        // Location
        public string? HomePort { get; set; }
        
        // Technical parameters (от заданието: "технически параметри")
        public decimal? Length { get; set; } // Дължина (в метри)
        public decimal? Width { get; set; } // Ширина (в метри)
        public decimal? GrossTonnage { get; set; } // Бруто тонаж
        public decimal? Draught { get; set; } // Газене (от заданието)
        public decimal? EnginePower { get; set; } // Мощност на двигател (в kW или HP)
        
        // Engine and fuel properties (КРИТИЧНО за Report 4 - въглероден отпечатък)
        public string? EngineType { get; set; } // Тип двигател (Diesel, Gasoline, Electric, Hybrid)
        public string? FuelType { get; set; } // Вид гориво (Diesel, Gasoline, LPG, CNG)
        public decimal? AverageFuelConsumptionPerHour { get; set; } // Среден разход (литри/час)
        public decimal? MaxFuelCapacity { get; set; } // Максимален капацитет на гориво
        
        // Additional properties
        public DateTime? BuiltYear { get; set; }
        public int? MaxCrew { get; set; } // Максимален екипаж
        public bool IsLargeShip { get; set; } = false; // Над 10м (от заданието: "голям кораб (над 10м)")
        
        // Ownership and management
        public int? OwnerFisherId { get; set; } // Собственик (от заданието: "собствениците")
        public int? CaptainFisherId { get; set; } // Капитан на кораба (от заданието: "капитан на кораба")
        public int? OperatorFisherId { get; set; } // Ползвател/оператор
        
        // Status and timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public string Status { get; set; } = "Active"; // Active, Inactive, Decommissioned

        // Навигационни свойства
        public virtual Fisher? Owner { get; set; } // Собственик
        public virtual Fisher? Captain { get; set; } // Капитан
        public virtual Fisher? Operator { get; set; } // Оператор/ползвател
        
        public virtual ICollection<License> Licenses { get; set; } = new List<License>();
        public virtual ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();
        public virtual ICollection<LogbookEntry> LogbookEntries { get; set; } = new List<LogbookEntry>();
        
        // Helper properties (calculated)
        public decimal LengthInMeters => Length ?? 0;
        public bool IsOver10Meters => LengthInMeters > 10;
        
        // Method to calculate if ship requires electronic logbook (от заданието)
        public bool RequiresElectronicLogbook() => IsOver10Meters;
        
        // Method to estimate fuel consumption for a trip duration (for Report 4)
        public decimal? EstimateFuelConsumption(decimal hours)
        {
            if (AverageFuelConsumptionPerHour.HasValue)
                return AverageFuelConsumptionPerHour.Value * hours;
            return null;
        }
    }
}