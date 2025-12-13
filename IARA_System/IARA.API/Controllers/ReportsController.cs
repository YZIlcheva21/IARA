using IARA.API.Services; // Това вече е достатъчно
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace IARA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService; // Променено: няма пълен namespace
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        [HttpGet("expiring-licenses")]
        public async Task<ActionResult> GetExpiringLicenses([FromQuery] int daysAhead = 30)
        {
            try
            {
                if (daysAhead <= 0 || daysAhead > 365)
                    return BadRequest(new { message = "Параметърът daysAhead трябва да бъде между 1 и 365" });
                
                var result = await _reportService.GetExpiringLicensesAsync(daysAhead);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при генериране на справка за изтичащи разрешителни");
                return StatusCode(500, new { message = "Грешка при генериране на справката", error = ex.Message });
            }
        }

        [HttpGet("amateur-ranking")]
        public async Task<ActionResult> GetAmateurRanking([FromQuery] int lastMonths = 12)
        {
            try
            {
                if (lastMonths <= 0 || lastMonths > 60)
                    return BadRequest(new { message = "Параметърът lastMonths трябва да бъде между 1 и 60" });
                
                var result = await _reportService.GetAmateurCatchRankingAsync(lastMonths);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при генериране на класация на любители");
                return StatusCode(500, new { message = "Грешка при генериране на класацията", error = ex.Message });
            }
        }

        [HttpGet("ship-catch-analysis/{year}")]
        public async Task<ActionResult> GetShipCatchAnalysis(int year)
        {
            try
            {
                if (year < 2000 || year > DateTime.Now.Year)
                    return BadRequest(new { message = $"Невалидна година. Моля изберете година между 2000 и {DateTime.Now.Year}" });
                
                var result = await _reportService.GetShipCatchAnalysisAsync(year);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при генериране на анализ на улова по кораби");
                return StatusCode(500, new { message = "Грешка при генериране на анализа", error = ex.Message });
            }
        }

        [HttpGet("ship-fuel-efficiency/{year}")]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<ActionResult> GetShipFuelEfficiency(int year)
        {
            try
            {
                if (year < 2000 || year > DateTime.Now.Year)
                    return BadRequest(new { message = $"Невалидна година. Моля изберете година между 2000 и {DateTime.Now.Year}" });
                
                var result = await _reportService.GetShipFuelEfficiencyAsync(year);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при генериране на анализ на ефективност на горивото");
                return StatusCode(500, new { message = "Грешка при генериране на анализа", error = ex.Message });
            }
        }

        [HttpGet("inspections")]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<ActionResult> GetInspectionsByPeriod(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string? inspectorId = null)
        {
            try
            {
                if (startDate > endDate)
                    return BadRequest(new { message = "Началната дата не може да бъде след крайната дата" });
                
                if ((endDate - startDate).TotalDays > 365)
                    return BadRequest(new { message = "Периодът за справка не може да надвишава 365 дни" });
                
                var result = await _reportService.GetInspectionsByPeriodAsync(startDate, endDate, inspectorId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при генериране на справка за инспекции");
                return StatusCode(500, new { message = "Грешка при генериране на справката", error = ex.Message });
            }
        }

        [HttpGet("fisher-statistics/{year}")]
        [Authorize(Roles = "Admin,LicenseOfficer")]
        public async Task<ActionResult> GetFisherStatistics(int year)
        {
            try
            {
                if (year < 2000 || year > DateTime.Now.Year)
                    return BadRequest(new { message = $"Невалидна година. Моля изберете година между 2000 и {DateTime.Now.Year}" });
                
                var result = await _reportService.GetFisherStatisticsAsync(year);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при генериране на статистика за рибари");
                return StatusCode(500, new { message = "Грешка при генериране на статистиката", error = ex.Message });
            }
        }
    }
}