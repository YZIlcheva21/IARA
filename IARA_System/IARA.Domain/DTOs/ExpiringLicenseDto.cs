namespace IARA.Domain.DTOs
{
    public class ExpiringLicenseDto
    {
        public int LicenseId { get; set; }
        public string LicenseNumber { get; set; } = string.Empty;
        public string ShipInternationalNumber { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public int DaysRemaining { get; set; }
    }
}