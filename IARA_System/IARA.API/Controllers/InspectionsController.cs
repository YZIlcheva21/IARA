// IARA.API/Controllers/InspectionsController.cs
using IARA.API.Data;
using IARA.Domain.DTOs;
using IARA.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IARA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InspectionsController : ControllerBase
    {
        private readonly IARAContext _context;

        public InspectionsController(IARAContext context)
        {
            _context = context;
        }

        // GET: api/Inspections
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InspectionDto>>> GetInspections(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] string? status = null)
        {
            // Създаваме базов заявка
            IQueryable<Inspection> query = _context.Inspections
                .Include(i => i.Inspector)
                .Include(i => i.Ship)
                .Include(i => i.License);

            // Прилагаме филтри
            if (fromDate.HasValue)
                query = query.Where(i => i.InspectionDate >= fromDate.Value);
            
            if (toDate.HasValue)
                query = query.Where(i => i.InspectionDate <= toDate.Value);
            
            if (!string.IsNullOrEmpty(status))
                query = query.Where(i => i.Status == status);

            // Проектираме в DTO
            var inspections = await query
                .Select(i => new InspectionDto
                {
                    Id = i.Id,
                    InspectorId = i.InspectorId,
                    ShipId = i.ShipId,
                    LicenseId = i.LicenseId,
                    InspectionDate = i.InspectionDate,
                    // CheckDate = i.CheckDate, // ПРЕМАХНЕТЕ ако няма такова свойство
                    InspectionType = i.InspectionType,
                    Location = i.Location,
                    Findings = i.Findings,
                    Violations = i.Violations,
                    ActionsTaken = i.ActionsTaken,
                    Status = i.Status,
                    Notes = i.Notes,
                    InspectorName = i.Inspector != null ? 
                        $"{i.Inspector.FirstName} {i.Inspector.LastName}" : null,
                    ShipName = i.Ship != null ? i.Ship.Name : null,
                    LicenseNumber = i.License != null ? i.License.LicenseNumber : null
                })
                .OrderByDescending(i => i.InspectionDate)
                .ToListAsync();

            return Ok(inspections);
        }

        // GET: api/Inspections/5
        [HttpGet("{id}")]
        public async Task<ActionResult<InspectionDto>> GetInspection(int id)
        {
            var inspection = await _context.Inspections
                .Include(i => i.Inspector)
                .Include(i => i.Ship)
                .Include(i => i.License)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inspection == null)
                return NotFound();

            var inspectionDto = new InspectionDto
            {
                Id = inspection.Id,
                InspectorId = inspection.InspectorId,
                ShipId = inspection.ShipId,
                LicenseId = inspection.LicenseId,
                InspectionDate = inspection.InspectionDate,
                // CheckDate = inspection.CheckDate, // ПРЕМАХНЕТЕ ако няма такова свойство
                InspectionType = inspection.InspectionType,
                Location = inspection.Location,
                Findings = inspection.Findings,
                Violations = inspection.Violations,
                ActionsTaken = inspection.ActionsTaken,
                Status = inspection.Status,
                Notes = inspection.Notes,
                InspectorName = inspection.Inspector != null ? 
                    $"{inspection.Inspector.FirstName} {inspection.Inspector.LastName}" : null,
                ShipName = inspection.Ship != null ? inspection.Ship.Name : null,
                LicenseNumber = inspection.License != null ? inspection.License.LicenseNumber : null
            };

            return Ok(inspectionDto);
        }

        // POST: api/Inspections
        [HttpPost]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<ActionResult<InspectionDto>> PostInspection(CreateInspectionDto createDto)
        {
            // Проверка на модела
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var inspection = new Inspection
            {
                InspectorId = createDto.InspectorId,
                ShipId = createDto.ShipId,
                LicenseId = createDto.LicenseId,
                InspectionDate = createDto.InspectionDate,
                InspectionType = createDto.InspectionType,
                Location = createDto.Location,
                Findings = createDto.Findings,
                Violations = createDto.Violations,
                ActionsTaken = createDto.ActionsTaken,
                Status = createDto.Status,
                Notes = createDto.Notes
            };

            _context.Inspections.Add(inspection);
            await _context.SaveChangesAsync();

            // Връщаме DTO със създадения запис
            var resultDto = new InspectionDto
            {
                Id = inspection.Id,
                InspectorId = inspection.InspectorId,
                ShipId = inspection.ShipId,
                LicenseId = inspection.LicenseId,
                InspectionDate = inspection.InspectionDate,
                InspectionType = inspection.InspectionType,
                Location = inspection.Location,
                Findings = inspection.Findings,
                Violations = inspection.Violations,
                ActionsTaken = inspection.ActionsTaken,
                Status = inspection.Status,
                Notes = inspection.Notes
            };

            return CreatedAtAction(nameof(GetInspection), 
                new { id = inspection.Id }, 
                resultDto);
        }

        // PUT: api/Inspections/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> PutInspection(int id, InspectionDto updateDto)
        {
            if (id != updateDto.Id)
                return BadRequest("ID mismatch");

            var inspection = await _context.Inspections.FindAsync(id);
            if (inspection == null)
                return NotFound();

            // Актуализиране на свойства
            inspection.InspectorId = updateDto.InspectorId;
            inspection.ShipId = updateDto.ShipId;
            inspection.LicenseId = updateDto.LicenseId;
            inspection.InspectionDate = updateDto.InspectionDate;
            inspection.InspectionType = updateDto.InspectionType;
            inspection.Location = updateDto.Location;
            inspection.Findings = updateDto.Findings;
            inspection.Violations = updateDto.Violations;
            inspection.ActionsTaken = updateDto.ActionsTaken;
            inspection.Status = updateDto.Status;
            inspection.Notes = updateDto.Notes;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InspectionExists(id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Inspections/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteInspection(int id)
        {
            var inspection = await _context.Inspections.FindAsync(id);
            if (inspection == null)
                return NotFound();

            _context.Inspections.Remove(inspection);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InspectionExists(int id)
        {
            return _context.Inspections.Any(e => e.Id == id);
        }
    }
}