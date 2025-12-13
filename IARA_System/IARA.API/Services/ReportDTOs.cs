using System;

namespace ARA.API.Services
{
    // Using partial classes to allow splitting across files if needed
    public partial class InspectionReportDto
    {
        public Guid InspectionId { get; set; }
        public DateTime InspectionDate { get; set; }
        public string InspectionName { get; set; }
        public string ShipName { get; set; }
        public string LicenseNumber { get; set; }
        // Add other properties as needed
    }

    public partial class FisherStatisticsDto
    {
        public Guid FisherId { get; set; }
        public string FisherName { get; set; }
        public int TotalInspections { get; set; }
        public int PassedInspections { get; set; }
        public int FailedInspections { get; set; }
        // Add other properties as needed
    }
}