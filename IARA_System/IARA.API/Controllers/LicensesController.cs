using IARA.API.Data;
using IARA.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IARA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LicensesController : ControllerBase
    {
        private readonly IARAContext _context;

        public LicensesController(IARAContext context)
        {
            _context = context;
        }

        // 1. GET: Взимане на списъка (Използва DTO за по-чисти данни)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LicenseDto>>> GetLicenses(
            [FromQuery] string? status = null,
            [FromQuery] int? fisherId = null,
            [FromQuery] int? shipId = null,
            [FromQuery] bool? expiringSoon = null)
        {
            IQueryable<License> query = _context.Licenses
                .Include(l => l.Fisher)
                .Include(l => l.Ship);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(l => l.Status == status);
            
            if (fisherId.HasValue)
                query = query.Where(l => l.FisherId == fisherId.Value);
            
            if (shipId.HasValue)
                query = query.Where(l => l.ShipId == shipId.Value);
            
            if (expiringSoon == true)
            {
                var warningDate = DateTime.Today.AddDays(30);
                query = query.Where(l => l.ExpiryDate.HasValue && 
                                        l.ExpiryDate.Value <= warningDate && 
                                        l.ExpiryDate.Value >= DateTime.Today);
            }

            var licenses = await query
                .Select(l => new LicenseDto
                {
                    Id = l.Id,
                    LicenseNumber = l.LicenseNumber,
                    FisherId = l.FisherId,
                    FisherName = l.Fisher != null ? $"{l.Fisher.FirstName} {l.Fisher.LastName}" : "Неизвестен",
                    ShipId = l.ShipId,
                    ShipName = l.Ship != null ? l.Ship.Name : null,
                    IssueDate = l.IssueDate,
                    ExpiryDate = l.ExpiryDate,
                    Status = l.Status,
                    LicenseType = l.LicenseType
                })
                .OrderByDescending(l => l.IssueDate)
                .ToListAsync();

            return Ok(licenses);
        }

        // 2. GET BY ID: Взимане на едно разрешително (За страницата Edit)
        [HttpGet("{id}")]
        public async Task<ActionResult<License>> GetLicense(int id)
        {
            // Тук връщаме целия обект. Благодарение на настройката в Program.cs (IgnoreCycles),
            // това няма да гръмне, дори да има връзки.
            var license = await _context.Licenses
                .Include(l => l.Fisher)
                .Include(l => l.Ship)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (license == null)
            {
                return NotFound();
            }

            return license;
        }

        // 3. POST: Създаване на ново разрешително
        [HttpPost]
        public async Task<ActionResult<License>> PostLicense(License license)
        {
            // 👇 КЛЮЧОВ МОМЕНТ ЗА СПРАВЯНЕ С ГРЕШКА 400 👇
            // Премахваме валидацията за навигационните обекти, защото клиентът изпраща само ID-та
            ModelState.Remove("Fisher");
            ModelState.Remove("Ship");
            ModelState.Remove("Inspections");
            ModelState.Remove("LogbookEntries");

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Проверка дали рибарят съществува
            if (license.FisherId > 0) 
            {
                var fisherExists = await _context.Fishers.AnyAsync(f => f.Id == license.FisherId);
                if (!fisherExists)
                {
                    return BadRequest($"Рибар с ID {license.FisherId} не съществува в базата.");
                }
            }

            // Проверка дали корабът съществува (ако е подаден)
            if (license.ShipId.HasValue && license.ShipId.Value > 0)
            {
                var shipExists = await _context.Ships.AnyAsync(s => s.Id == license.ShipId);
                if (!shipExists)
                {
                    return BadRequest($"Кораб с ID {license.ShipId} не съществува в базата.");
                }
            }
            else
            {
                // Уверяваме се, че е null, ако е <= 0
                license.ShipId = null;
            }

            _context.Licenses.Add(license);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLicense", new { id = license.Id }, license);
        }

        // 4. PUT: Редакция на съществуващо разрешително
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLicense(int id, License license)
        {
            if (id != license.Id)
            {
                return BadRequest("ID в URL не съвпада с ID в тялото на заявката.");
            }

            // 👇 КЛЮЧОВ МОМЕНТ И ТУК 👇
            ModelState.Remove("Fisher");
            ModelState.Remove("Ship");
            ModelState.Remove("Inspections");
            ModelState.Remove("LogbookEntries");

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Entry(license).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Licenses.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // 5. DELETE: Изтриване
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLicense(int id)
        {
            var license = await _context.Licenses.FindAsync(id);
            if (license == null)
            {
                return NotFound();
            }

            _context.Licenses.Remove(license);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    // DTO класът за списъка
    public class LicenseDto
    {
        public int Id { get; set; }
        public string LicenseNumber { get; set; } = string.Empty;
        public int FisherId { get; set; }
        public string? FisherName { get; set; }
        public int? ShipId { get; set; }
        public string? ShipName { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? LicenseType { get; set; }
    }
}