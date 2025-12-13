using IARA.API.Data;
using IARA.Domain.DTOs;
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

        // 1. Изтичащи разрешителни (Report 1 от заданието - 3 точки)
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

                _logger.LogInformation("Справка за изтичащи разрешителни: намерени {Count} записа", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при генериране на справка за изтичащи разрешителни");
                throw;
            }
        }

        // 2. Класация на любители (Report 2 от заданието - 4 точки)
        public async Task<IEnumerable<AmateurRankingDto>> GetAmateurCatchRankingAsync(int lastMonths = 12)
        {
            try
            {
                DateTime cutoffDate = DateTime.Today.AddMonths(-lastMonths);

                var ranking = await _context.AmateurCatches
                    .Include(c => c.AmateurTicket)
                    .ThenInclude(t => t.Fisher)
                    .Where(c => c.CatchDate >= cutoffDate)
                    .GroupBy(c => c.AmateurTicket.FisherId)
                    .Select(g => new AmateurRankingDto
                    {
                        FisherId = g.Key,
                        FisherName = g.First().AmateurTicket.Fisher != null ?
                            $"{g.First().AmateurTicket.Fisher.FirstName ?? string.Empty} {g.First().AmateurTicket.Fisher.LastName ?? string.Empty}".Trim() :
                            "Unknown",
                        TotalCatchInKgs = (double)g.Sum(c => c.WeightKgs ?? 0)
                    })
                    .OrderByDescending(r => r.TotalCatchInKgs)
                    .ToListAsync();

                _logger.LogInformation("Класация на любители: намерени {Count} участници", ranking.Count);
                return ranking;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при генериране на класация на любители");
                throw;
            }
        }

        // 3. Анализ на улова по кораби (Report 3 от заданието - 4 точки)
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
                    .Select(e => new
                    {
                        ShipId = e.License.ShipId,
                        Ship = e.License.Ship,
                        CatchDetails = e.CatchDetails
                    })
                    .ToListAsync();

                var groupedData = logbookData
                    .GroupBy(x => x.ShipId)
                    .Select(g =>
                    {
                        var ship = g.First().Ship;
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

                _logger.LogInformation("Анализ на улова по кораби за {Year}: {Count} кораба", year, groupedData.Count);
                return groupedData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при генериране на анализ на улова по кораби за година {Year}", year);
                throw;
            }
        }

        // 4. Ефективност на горивото (Report 4 от заданието - 6 точки)
        // Въглероден отпечатък = общо гориво / общ улов
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
                    .Select(e => new
                    {
                        ShipId = e.License.ShipId,
                        Ship = e.License.Ship,
                        CatchWeight = e.CatchDetails.Sum(cd => cd.WeightKgs ?? 0),
                        FuelUsed = e.FuelConsumptionLiters ?? 0,
                        Hours = e.StartTime.HasValue && e.EndTime.HasValue ?
                               (e.EndTime.Value - e.StartTime.Value).TotalHours : 0
                    })
                    .ToListAsync();

                var groupedData = logbookData
                    .GroupBy(x => x.ShipId)
                    .Select(g =>
                    {
                        var firstRecord = g.First();
                        var totalCatch = g.Sum(x => x.CatchWeight);
                        var totalFuel = g.Sum(x => x.FuelUsed);
                        var totalHours = g.Sum(x => x.Hours);

                        return new ShipFuelEfficiencyDto
                        {
                            ShipInternationalNumber = firstRecord.Ship != null ?
                                firstRecord.Ship.InternationalNumber ?? "N/A" : "N/A",
                            TotalCatchKgs = (double)totalCatch,
                            TotalFuelUsed = (double)totalFuel,
                            TotalFishingHours = totalHours,
                            FuelPerKgCatch = totalCatch > 0 ? (double)(totalFuel / totalCatch) : 0,
                            AvgFuelPerHour = totalHours > 0 ? (double)(totalFuel / (decimal)totalHours) : 0
                        };
                    })
                    .Where(x => x.TotalCatchKgs > 0)
                    .OrderBy(x => x.FuelPerKgCatch) // Подредени по най-ефективни (най-малко гориво за кг риба)
                    .ToList();

                _logger.LogInformation("Ефективност на горивото за {Year}: {Count} кораба", year, groupedData.Count);
                return groupedData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при генериране на анализ на ефективност на горивото за година {Year}", year);
                throw;
            }
        }

        // 5. Инспекции по период
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

                if (!string.IsNullOrEmpty(inspectorId))
                {
                    if (int.TryParse(inspectorId, out int inspectorIdInt))
                    {
                        query = query.Where(i => i.InspectorId == inspectorIdInt);
                    }
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

                _logger.LogInformation("Справка за инспекции от {StartDate} до {EndDate}: {Count} записа", 
                    startDate, endDate, inspections.Count);
                return inspections;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при генериране на справка за инспекции");
                throw;
            }
        }

        // 6. Статистика за рибари
        public async Task<IEnumerable<FisherStatisticsDto>> GetFisherStatisticsAsync(int year)
        {
            try
            {
                var startDate = new DateTime(year, 1, 1);
                var endDate = new DateTime(year, 12, 31, 23, 59, 59);

                var statistics = await _context.Fishers
                    .Select(f => new FisherStatisticsDto
                    {
                        FisherId = f.Id,
                        FisherName = $"{f.FirstName} {f.LastName}",
                        TotalLicenses = f.Licenses.Count(l => l.IssueDate.Year == year),
                        ActiveLicenses = f.Licenses.Count(l => l.Status == "Active" && l.IssueDate.Year == year),
                        OwnedShips = f.Ships.Count(s => s.IsActive),
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
                    .OrderByDescending(s => s.ProfessionalCatchesKgs + s.AmateurCatchesKgs)
                    .ToListAsync();

                _logger.LogInformation("Статистика за рибари за {Year}: {Count} рибаря", year, statistics.Count);
                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при генериране на статистика за рибари за година {Year}", year);
                throw;
            }
        }
    }

    // ===================== ВСИЧКИ DTO КЛАСОВЕ (добавете ги в края на файла) =====================
    
    // Report 1 DTO
    public class ExpiringLicenseDto
    {
        public int LicenseId { get; set; }
        public string LicenseNumber { get; set; } = string.Empty;
        public string ShipInternationalNumber { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public int DaysRemaining { get; set; }
    }

    // Report 2 DTO
    public class AmateurRankingDto
    {
        public int FisherId { get; set; }
        public string FisherName { get; set; } = string.Empty;
        public double TotalCatchInKgs { get; set; }
    }

    // Report 3 DTO
    public class ShipCatchAnalysisDto
    {
        public string ShipInternationalNumber { get; set; } = string.Empty;
        public int TotalTrips { get; set; }
        public double TotalCatchKgs { get; set; }
        public double MaxCatchPerTripKgs { get; set; }
        public double MinCatchPerTripKgs { get; set; }
        public double AvgCatchPerTripKgs { get; set; }
    }

    // Report 4 DTO
    public class ShipFuelEfficiencyDto
    {
        public string ShipInternationalNumber { get; set; } = string.Empty;
        public double TotalCatchKgs { get; set; }
        public double TotalFuelUsed { get; set; }
        public double TotalFishingHours { get; set; }
        public double FuelPerKgCatch { get; set; } // Въглероден отпечатък: гориво за 1кг риба
        public double AvgFuelPerHour { get; set; }
    }

    // Допълнителни DTO-та
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