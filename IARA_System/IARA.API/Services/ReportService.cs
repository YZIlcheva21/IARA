using IARA.API.Data;
using IARA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IARA.API.Services
{
    // ===================== ИНТЕРФЕЙС =====================
    public interface IReportService
    {
        Task<IEnumerable<ExpiringLicenseDto>> GetExpiringLicensesAsync(int daysAhead = 30);
        Task<IEnumerable<AmateurRankingDto>> GetAmateurCatchRankingAsync(int lastMonths = 12);
        Task<IEnumerable<ShipCatchAnalysisDto>> GetShipCatchAnalysisAsync(int year);
        Task<IEnumerable<ShipFuelEfficiencyDto>> GetShipFuelEfficiencyAsync(int year);
        Task<IEnumerable<InspectionReportDto>> GetInspectionsByPeriodAsync(DateTime startDate, DateTime endDate, string? inspectorId = null);
        Task<IEnumerable<FisherStatisticsDto>> GetFisherStatisticsAsync(int year);
    }

    // ===================== РЕАЛНА ИМПЛЕМЕНТАЦИЯ =====================
    public class ReportService : IReportService
    {
        private readonly IARAContext _context;
        private readonly ILogger<ReportService> _logger;

        public ReportService(IARAContext context, ILogger<ReportService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // 1. Изтичащи разрешителни (Report 1)
        public async Task<IEnumerable<ExpiringLicenseDto>> GetExpiringLicensesAsync(int daysAhead = 30)
        {
            try
            {
                DateTime cutoffDate = DateTime.Today.AddDays(daysAhead);

                var result = await _context.Licenses
                    .Include(l => l.Ship)
                    .Include(l => l.Fisher)
                    .Where(l => l.Status == "Active" &&
                               l.ExpiryDate.HasValue &&
                               l.ExpiryDate.Value <= cutoffDate &&
                               l.ExpiryDate.Value >= DateTime.Today)
                    .Select(l => new ExpiringLicenseDto
                    {
                        LicenseId = l.Id,
                        LicenseNumber = l.LicenseNumber ?? "N/A",
                        ShipInternationalNumber = l.Ship != null ? l.Ship.InternationalNumber ?? "N/A" : "N/A",
                        OwnerName = l.Fisher != null ?
                            $"{l.Fisher.FirstName ?? string.Empty} {l.Fisher.LastName ?? string.Empty}".Trim() :
                            "N/A",
                        ExpiryDate = l.ExpiryDate.Value,
                        DaysRemaining = (l.ExpiryDate.Value - DateTime.Today).Days
                    })
                    .OrderBy(r => r.DaysRemaining)
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при генериране на справка за изтичащи разрешителни");
                throw;
            }
        }

        // 2. Класация на любители (ПОПРАВЕНО: Report 2)
        public async Task<IEnumerable<AmateurRankingDto>> GetAmateurCatchRankingAsync(int lastMonths = 12)
        {
            try
            {
                DateTime cutoffDate = DateTime.Today.AddMonths(-lastMonths);

                // 1. Групираме по новия UserId
                var rawStats = await _context.AmateurCatches
                    .Where(c => c.CatchDate >= cutoffDate)
                    .GroupBy(c => c.UserId)
                    .Select(g => new
                    {
                        UserId = g.Key,
                        TotalCatch = g.Sum(c => c.WeightKgs ?? 0)
                    })
                    .OrderByDescending(x => x.TotalCatch)
                    .Take(20)
                    .ToListAsync();

                // 2. Взимаме имената от таблицата с потребители
                var userIds = rawStats.Select(s => s.UserId).ToList();
                var users = await _context.Users
                    .Where(u => userIds.Contains(u.Id))
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();

                // 3. Сглобяваме резултата
                var result = new List<AmateurRankingDto>();

                foreach (var stat in rawStats)
                {
                    var user = users.FirstOrDefault(u => u.Id == stat.UserId);
                    string displayName = "Неизвестен";

                    if (user != null)
                    {
                        displayName = !string.IsNullOrEmpty(user.Email) ? user.Email : user.UserName;
                    }

                    result.Add(new AmateurRankingDto
                    {
                        FisherId = 0, // Вече не е важно
                        FisherName = displayName,
                        TotalCatchInKgs = (double)stat.TotalCatch
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при генериране на класация на любители");
                throw;
            }
        }

        // 3. Анализ на улова по кораби (Report 3)
        public async Task<IEnumerable<ShipCatchAnalysisDto>> GetShipCatchAnalysisAsync(int year)
        {
            try
            {
                var startDate = new DateTime(year, 1, 1);
                var endDate = new DateTime(year, 12, 31, 23, 59, 59);

                var logbookData = await _context.LogbookEntries
                    .Include(e => e.CatchDetails)
                    .Include(e => e.License)
                    .ThenInclude(l => l.Ship)
                    .Where(e => e.FishingDate >= startDate && e.FishingDate <= endDate)
                    .Where(e => e.License.ShipId != null)
                    .ToListAsync(); // Изтегляме в паметта, за да избегнем сложни SQL грешки

                var groupedData = logbookData
                    .GroupBy(x => x.License.ShipId)
                    .Select(g =>
                    {
                        var ship = g.First().License.Ship;
                        var allCatches = g.SelectMany(x => x.CatchDetails)
                                         .Where(cd => cd.WeightKgs.HasValue)
                                         .Select(cd => cd.WeightKgs.Value)
                                         .ToList();

                        return new ShipCatchAnalysisDto
                        {
                            ShipInternationalNumber = ship != null ? ship.InternationalNumber ?? "N/A" : "N/A",
                            TotalTrips = g.Count(),
                            TotalCatchKgs = (double)allCatches.Sum(),
                            MaxCatchPerTripKgs = allCatches.Any() ? (double)allCatches.Max() : 0,
                            MinCatchPerTripKgs = allCatches.Any() ? (double)allCatches.Min() : 0,
                            AvgCatchPerTripKgs = allCatches.Any() ? (double)allCatches.Average() : 0
                        };
                    })
                    .OrderByDescending(r => r.TotalCatchKgs)
                    .ToList();

                return groupedData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при генериране на анализ на улова по кораби");
                throw;
            }
        }

        // 4. Ефективност на горивото (Report 4)
        public async Task<IEnumerable<ShipFuelEfficiencyDto>> GetShipFuelEfficiencyAsync(int year)
        {
            try
            {
                var startDate = new DateTime(year, 1, 1);
                var endDate = new DateTime(year, 12, 31, 23, 59, 59);

                var logbookData = await _context.LogbookEntries
                    .Include(e => e.CatchDetails)
                    .Include(e => e.License)
                    .ThenInclude(l => l.Ship)
                    .Where(e => e.FishingDate >= startDate && e.FishingDate <= endDate)
                    .Where(e => e.License.Status != "Revoked" && e.License.ShipId != null)
                    .ToListAsync();

                var groupedData = logbookData
                    .GroupBy(x => x.License.ShipId)
                    .Select(g =>
                    {
                        var firstRecord = g.First();
                        var totalCatch = g.Sum(x => x.CatchDetails.Sum(cd => cd.WeightKgs ?? 0));
                        var totalFuel = g.Sum(x => x.FuelConsumptionLiters ?? 0);
                        var totalHours = g.Sum(x => x.StartTime.HasValue && x.EndTime.HasValue ?
                               (x.EndTime.Value - x.StartTime.Value).TotalHours : 0);

                        return new ShipFuelEfficiencyDto
                        {
                            ShipInternationalNumber = firstRecord.License.Ship != null ?
                                firstRecord.License.Ship.InternationalNumber ?? "N/A" : "N/A",
                            TotalCatchKgs = (double)totalCatch,
                            TotalFuelUsed = (double)totalFuel,
                            TotalFishingHours = totalHours,
                            FuelPerKgCatch = totalCatch > 0 ? (double)(totalFuel / totalCatch) : 0,
                            AvgFuelPerHour = totalHours > 0 ? (double)(totalFuel / (decimal)totalHours) : 0
                        };
                    })
                    .Where(x => x.TotalCatchKgs > 0)
                    .OrderBy(x => x.FuelPerKgCatch)
                    .ToList();

                return groupedData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при генериране на анализ на ефективност на горивото");
                throw;
            }
        }

        // 5. Инспекции по период (Report 5)
        public async Task<IEnumerable<InspectionReportDto>> GetInspectionsByPeriodAsync(
            DateTime startDate, DateTime endDate, string? inspectorId = null)
        {
            try
            {
                IQueryable<Inspection> query = _context.Inspections
                    .Include(i => i.Inspector)
                    .Include(i => i.Ship)
                    .Include(i => i.License)
                    .Where(i => i.InspectionDate >= startDate && i.InspectionDate <= endDate);

                if (!string.IsNullOrEmpty(inspectorId) && int.TryParse(inspectorId, out int inspId))
                {
                    query = query.Where(i => i.InspectorId == inspId);
                }

                var inspections = await query
                    .Select(i => new InspectionReportDto
                    {
                        InspectionId = i.Id,
                        InspectionDate = i.InspectionDate,
                        InspectorName = i.Inspector != null ?
                            $"{i.Inspector.FirstName} {i.Inspector.LastName}" : "N/A",
                        ShipName = i.Ship != null ? i.Ship.Name : "N/A",
                        LicenseNumber = i.License != null ? i.License.LicenseNumber : "N/A",
                        InspectionType = i.InspectionType,
                        Status = i.Status,
                        ViolationsFound = !string.IsNullOrEmpty(i.Violations),
                        ActionsTaken = i.ActionsTaken
                    })
                    .OrderByDescending(i => i.InspectionDate)
                    .ToListAsync();

                return inspections;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при генериране на справка за инспекции");
                throw;
            }
        }

        // 6. Статистика за рибари (Report 6)
        public async Task<IEnumerable<FisherStatisticsDto>> GetFisherStatisticsAsync(int year)
        {
            try
            {
                // Зареждаме рибарите, за да избегнем проблем с навигационните пропъртита при сложна заявка
                var fishers = await _context.Fishers
                    .Include(f => f.Licenses).ThenInclude(l => l.LogbookEntries).ThenInclude(le => le.CatchDetails)
                    .Include(f => f.Ships)
                    .Include(f => f.AmateurTickets).ThenInclude(at => at.AmateurCatches)
                    .ToListAsync();

                var statistics = fishers
                    .Select(f => new FisherStatisticsDto
                    {
                        FisherId = f.Id,
                        FisherName = $"{f.FirstName} {f.LastName}",
                        TotalLicenses = f.Licenses.Count(l => l.IssueDate.Year == year),
                        ActiveLicenses = f.Licenses.Count(l => l.Status == "Active" && l.IssueDate.Year == year),
                        OwnedShips = f.Ships.Count(s => s.IsActive),
                        
                        // Безопасно сумиране
                        AmateurCatchesKgs = (double)f.AmateurTickets
                            .Where(t => t.IssueDate.Year == year)
                            .SelectMany(t => t.AmateurCatches)
                            .Sum(c => c.WeightKgs ?? 0),
                            
                        ProfessionalCatchesKgs = (double)f.Licenses
                            .Where(l => l.IssueDate.Year == year)
                            .SelectMany(l => l.LogbookEntries)
                            .Where(e => e.FishingDate.Year == year)
                            .SelectMany(e => e.CatchDetails)
                            .Sum(cd => cd.WeightKgs ?? 0)
                    })
                    .Where(s => s.TotalLicenses > 0 || s.AmateurCatchesKgs > 0 || s.ProfessionalCatchesKgs > 0)
                    .OrderByDescending(s => s.TotalCatchesKgs)
                    .ToList();

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при генериране на статистика за рибари");
                throw;
            }
        }
    }

    // ===================== DTO КЛАСОВЕ =====================
    // (Копирай ги точно както са си били при теб)
    
    public class ExpiringLicenseDto
    {
        public int LicenseId { get; set; }
        public string LicenseNumber { get; set; } = string.Empty;
        public string ShipInternationalNumber { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public int DaysRemaining { get; set; }
    }

    public class AmateurRankingDto
    {
        public int FisherId { get; set; }
        public string FisherName { get; set; } = string.Empty;
        public double TotalCatchInKgs { get; set; }
    }

    public class ShipCatchAnalysisDto
    {
        public string ShipInternationalNumber { get; set; } = string.Empty;
        public int TotalTrips { get; set; }
        public double TotalCatchKgs { get; set; }
        public double MaxCatchPerTripKgs { get; set; }
        public double MinCatchPerTripKgs { get; set; }
        public double AvgCatchPerTripKgs { get; set; }
    }

    public class ShipFuelEfficiencyDto
    {
        public string ShipInternationalNumber { get; set; } = string.Empty;
        public double TotalCatchKgs { get; set; }
        public double TotalFuelUsed { get; set; }
        public double TotalFishingHours { get; set; }
        public double FuelPerKgCatch { get; set; } 
        public double AvgFuelPerHour { get; set; }
    }

    public class InspectionReportDto
    {
        public int InspectionId { get; set; }
        public DateTime InspectionDate { get; set; }
        public string InspectorName { get; set; } = string.Empty;
        public string ShipName { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string InspectionType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool ViolationsFound { get; set; }
        public string? ActionsTaken { get; set; }
    }

    public class FisherStatisticsDto
    {
        public int FisherId { get; set; }
        public string FisherName { get; set; } = string.Empty;
        public int TotalLicenses { get; set; }
        public int ActiveLicenses { get; set; }
        public int OwnedShips { get; set; }
        public double AmateurCatchesKgs { get; set; }
        public double ProfessionalCatchesKgs { get; set; }
        public double TotalCatchesKgs => AmateurCatchesKgs + ProfessionalCatchesKgs;
    }
}