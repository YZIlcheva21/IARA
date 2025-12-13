// IARA.Domain/DTOs/InspectionDto.cs
namespace IARA.Domain.DTOs
{
    public class InspectionDto
    {
        public int Id { get; set; }
        public int? InspectorId { get; set; }
        public int? ShipId { get; set; }
        public int? LicenseId { get; set; }
        public DateTime InspectionDate { get; set; }
        public string InspectionType { get; set; }
        public string? Location { get; set; }
        public string? Findings { get; set; }
        public string? Violations { get; set; }
        public string? ActionsTaken { get; set; }
        public string Status { get; set; }
        public string? Notes { get; set; }
        
        // Допълнителни полета
        public string? InspectorName { get; set; }
        public string? ShipName { get; set; }
        public string? LicenseNumber { get; set; }
    }

    public class CreateInspectionDto
    {
        public int? InspectorId { get; set; }
        public int? ShipId { get; set; }
        public int? LicenseId { get; set; }
        public DateTime InspectionDate { get; set; }
        public string InspectionType { get; set; } = "Planned";
        public string? Location { get; set; }
        public string? Findings { get; set; }
        public string? Violations { get; set; }
        public string? ActionsTaken { get; set; }
        public string Status { get; set; } = "Planned";
        public string? Notes { get; set; }
    }
}