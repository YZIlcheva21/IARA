namespace IARA.Domain.DTOs
{
    public class AmateurRankingDto
    {
        public int FisherId { get; set; }
        public string FisherName { get; set; } = string.Empty;
        public double TotalCatchInKgs { get; set; }
    }
}