namespace IARA.Domain.DTOs
{
    public class ShipFuelEfficiencyDto
    {
        public string ShipInternationalNumber { get; set; } = string.Empty;
        public double TotalCatchKgs { get; set; }
        public double TotalFuelUsed { get; set; }
        public double TotalFishingHours { get; set; }
        public double FuelPerKgCatch { get; set; }
        public double AvgFuelPerHour { get; set; }
    }
}