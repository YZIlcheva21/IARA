using IARA.API.Data;
using IARA.Domain.DTOs;  // This is CRITICAL - references your DTOs
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
    public class ShipRegistryController : ControllerBase
    {
        private readonly IARAContext _context;

        public ShipRegistryController(IARAContext context)
        {
            _context = context;
        }

        // GET: api/ShipRegistry
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShipDto>>> GetShips(
            [FromQuery] string? search = null,
            [FromQuery] bool? activeOnly = true,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            IQueryable<Ship> query = _context.Ships
                .Include(s => s.Owner)
                .Include(s => s.Captain)
                .Include(s => s.Licenses.Where(l => l.Status == "Active"));

            // Apply filters
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s =>
                    s.Name.Contains(search) ||
                    s.InternationalNumber.Contains(search) ||
                    s.CallSign.Contains(search) ||
                    s.RegistrationNumber.Contains(search) ||
                    (s.Owner != null && (
                        s.Owner.FirstName.Contains(search) ||
                        s.Owner.LastName.Contains(search)))
                );
            }

            if (activeOnly == true)
            {
                query = query.Where(s => s.IsActive);
            }

            // Total count
            var totalCount = await query.CountAsync();

            // Pagination
            var ships = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new ShipDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    InternationalNumber = s.InternationalNumber,
                    CallSign = s.CallSign,
                    Marking = s.Marking,
                    RegistrationNumber = s.RegistrationNumber,
                    HomePort = s.HomePort,
                    Length = s.Length,
                    Width = s.Width,
                    GrossTonnage = s.GrossTonnage,
                    Draught = s.Draught,
                    EnginePower = s.EnginePower,
                    EngineType = s.EngineType,
                    FuelType = s.FuelType,
                    AverageFuelConsumptionPerHour = s.AverageFuelConsumptionPerHour,
                    BuiltYear = s.BuiltYear,
                    IsActive = s.IsActive,
                    IsLargeShip = s.IsLargeShip,
                    OwnerId = s.OwnerFisherId,
                    CaptainId = s.CaptainFisherId,
                    OwnerName = s.Owner != null ? $"{s.Owner.FirstName} {s.Owner.LastName}" : null,
                    CaptainName = s.Captain != null ? $"{s.Captain.FirstName} {s.Captain.LastName}" : null,
                    ActiveLicenseNumber = s.Licenses.FirstOrDefault(l => l.Status == "Active") != null ?
                        s.Licenses.First(l => l.Status == "Active").LicenseNumber : null,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            var response = new
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Ships = ships
            };

            return Ok(response);
        }

        // GET: api/ShipRegistry/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ShipDto>> GetShip(int id)
        {
            var ship = await _context.Ships
                .Include(s => s.Owner)
                .Include(s => s.Captain)
                .Include(s => s.Operator)
                .Include(s => s.Licenses)
                .Include(s => s.LogbookEntries)
                .Include(s => s.Inspections)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (ship == null)
            {
                return NotFound(new { message = $"Кораб с ID {id} не е намерен" });
            }

            var shipDto = new ShipDto
            {
                Id = ship.Id,
                Name = ship.Name,
                InternationalNumber = ship.InternationalNumber,
                CallSign = ship.CallSign,
                Marking = ship.Marking,
                RegistrationNumber = ship.RegistrationNumber,
                HomePort = ship.HomePort,
                Length = ship.Length,
                Width = ship.Width,
                GrossTonnage = ship.GrossTonnage,
                Draught = ship.Draught,
                EnginePower = ship.EnginePower,
                EngineType = ship.EngineType,
                FuelType = ship.FuelType,
                AverageFuelConsumptionPerHour = ship.AverageFuelConsumptionPerHour,
                BuiltYear = ship.BuiltYear,
                IsActive = ship.IsActive,
                IsLargeShip = ship.IsLargeShip,
                OwnerId = ship.OwnerFisherId,
                CaptainId = ship.CaptainFisherId,
                OperatorId = ship.OperatorFisherId,
                OwnerName = ship.Owner != null ? $"{ship.Owner.FirstName} {ship.Owner.LastName}" : null,
                CaptainName = ship.Captain != null ? $"{ship.Captain.FirstName} {ship.Captain.LastName}" : null,
                OperatorName = ship.Operator != null ? $"{ship.Operator.FirstName} {ship.Operator.LastName}" : null,
                ActiveLicenseNumber = ship.Licenses.FirstOrDefault(l => l.Status == "Active")?.LicenseNumber,
                LicenseCount = ship.Licenses.Count,
                LogbookEntryCount = ship.LogbookEntries.Count,
                InspectionCount = ship.Inspections.Count,
                CreatedAt = ship.CreatedAt
            };

            return Ok(shipDto);
        }

        // POST: api/ShipRegistry
        [HttpPost]
        [Authorize(Roles = "Admin,RegistryOfficer")]
        public async Task<ActionResult<Ship>> PostShip(CreateShipDto createShipDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ship = new Ship
            {
                Name = createShipDto.Name,
                InternationalNumber = createShipDto.InternationalNumber,
                CallSign = createShipDto.CallSign,
                Marking = createShipDto.Marking,
                RegistrationNumber = createShipDto.RegistrationNumber,
                HomePort = createShipDto.HomePort,
                Length = createShipDto.Length,
                Width = createShipDto.Width,
                GrossTonnage = createShipDto.GrossTonnage,
                Draught = createShipDto.Draught,
                EnginePower = createShipDto.EnginePower,
                EngineType = createShipDto.EngineType,
                FuelType = createShipDto.FuelType,
                AverageFuelConsumptionPerHour = createShipDto.AverageFuelConsumptionPerHour,
                BuiltYear = createShipDto.BuiltYear,
                IsLargeShip = createShipDto.Length > 10,
                OwnerFisherId = createShipDto.OwnerId,
                CaptainFisherId = createShipDto.CaptainId,
                OperatorFisherId = createShipDto.OperatorId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Ships.Add(ship);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetShip", new { id = ship.Id }, new 
            { 
                message = "Корабът е създаден успешно", 
                id = ship.Id 
            });
        }

        // PUT: api/ShipRegistry/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,RegistryOfficer")]
        public async Task<IActionResult> PutShip(int id, UpdateShipDto updateShipDto)
        {
            if (id != updateShipDto.Id)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

            var ship = await _context.Ships.FindAsync(id);
            if (ship == null)
            {
                return NotFound(new { message = $"Кораб с ID {id} не е намерен" });
            }

            // Update properties
            ship.Name = updateShipDto.Name;
            ship.InternationalNumber = updateShipDto.InternationalNumber;
            ship.CallSign = updateShipDto.CallSign;
            ship.Marking = updateShipDto.Marking;
            ship.RegistrationNumber = updateShipDto.RegistrationNumber;
            ship.HomePort = updateShipDto.HomePort;
            ship.Length = updateShipDto.Length;
            ship.Width = updateShipDto.Width;
            ship.GrossTonnage = updateShipDto.GrossTonnage;
            ship.Draught = updateShipDto.Draught;
            ship.EnginePower = updateShipDto.EnginePower;
            ship.EngineType = updateShipDto.EngineType;
            ship.FuelType = updateShipDto.FuelType;
            ship.AverageFuelConsumptionPerHour = updateShipDto.AverageFuelConsumptionPerHour;
            ship.BuiltYear = updateShipDto.BuiltYear;
            ship.IsLargeShip = updateShipDto.Length > 10;
            ship.IsActive = updateShipDto.IsActive;
            ship.OwnerFisherId = updateShipDto.OwnerId;
            ship.CaptainFisherId = updateShipDto.CaptainId;
            ship.OperatorFisherId = updateShipDto.OperatorId;

            _context.Entry(ship).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ShipExists(id))
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

        // DELETE: api/ShipRegistry/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteShip(int id)
        {
            var ship = await _context.Ships.FindAsync(id);
            if (ship == null)
            {
                return NotFound(new { message = $"Кораб с ID {id} не е намерен" });
            }

            // Check if ship has active licenses
            var hasActiveLicenses = await _context.Licenses
                .AnyAsync(l => l.ShipId == id && l.Status == "Active");

            if (hasActiveLicenses)
            {
                return BadRequest(new { message = "Не може да изтриете кораб с активни лицензи" });
            }

            _context.Ships.Remove(ship);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ShipExists(int id)
        {
            return _context.Ships.Any(e => e.Id == id);
        }
    }
}