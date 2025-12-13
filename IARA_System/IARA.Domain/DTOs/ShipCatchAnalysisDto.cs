namespace IARA.Domain.DTOs
{
    public class ShipCatchAnalysisDto
    {
        public string ShipInternationalNumber { get; set; } = string.Empty;
        public int TotalTrips { get; set; }
        public double TotalCatchKgs { get; set; }
        public double MaxCatchPerTripKgs { get; set; }
        public double MinCatchPerTripKgs { get; set; }
        public double AvgCatchPerTripKgs { get; set; }
    }
}