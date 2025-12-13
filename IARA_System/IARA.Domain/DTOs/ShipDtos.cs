// IARA.Domain/DTOs/ShipDtos.cs
using System;

namespace IARA.Domain.DTOs
{
    public class ShipDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string InternationalNumber { get; set; } = string.Empty;
        public string? CallSign { get; set; }
        public string? Marking { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? HomePort { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? GrossTonnage { get; set; }
        public decimal? Draught { get; set; }
        public decimal? EnginePower { get; set; }
        public string? EngineType { get; set; }
        public string? FuelType { get; set; }
        public decimal? AverageFuelConsumptionPerHour { get; set; }
        public DateTime? BuiltYear { get; set; }
        public bool IsActive { get; set; }
        public bool IsLargeShip { get; set; }
        public int? OwnerId { get; set; }
        public int? CaptainId { get; set; }
        public int? OperatorId { get; set; }
        public string? OwnerName { get; set; }
        public string? CaptainName { get; set; }
        public string? OperatorName { get; set; }
        public string? ActiveLicenseNumber { get; set; }
        public int? LicenseCount { get; set; }
        public int? LogbookEntryCount { get; set; }
        public int? InspectionCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateShipDto
    {
        public string Name { get; set; } = string.Empty;
        public string InternationalNumber { get; set; } = string.Empty;
        public string? CallSign { get; set; }
        public string? Marking { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? HomePort { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? GrossTonnage { get; set; }
        public decimal? Draught { get; set; }
        public decimal? EnginePower { get; set; }
        public string? EngineType { get; set; }
        public string? FuelType { get; set; }
        public decimal? AverageFuelConsumptionPerHour { get; set; }
        public DateTime? BuiltYear { get; set; }
        public int? OwnerId { get; set; }
        public int? CaptainId { get; set; }
        public int? OperatorId { get; set; }
    }

    public class UpdateShipDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string InternationalNumber { get; set; } = string.Empty;
        public string? CallSign { get; set; }
        public string? Marking { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? HomePort { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? GrossTonnage { get; set; }
        public decimal? Draught { get; set; }
        public decimal? EnginePower { get; set; }
        public string? EngineType { get; set; }
        public string? FuelType { get; set; }
        public decimal? AverageFuelConsumptionPerHour { get; set; }
        public DateTime? BuiltYear { get; set; }
        public bool IsActive { get; set; }
        public int? OwnerId { get; set; }
        public int? CaptainId { get; set; }
        public int? OperatorId { get; set; }
    }
}