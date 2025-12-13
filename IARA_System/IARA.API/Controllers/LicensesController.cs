using IARA.API.Data;
using IARA.Domain.DTOs;
using IARA.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IARA.Domain.DTOs;

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

            // Филтри
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
                    FisherName = l.Fisher != null ? $"{l.Fisher.FirstName} {l.Fisher.LastName}" : null,
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

        // ... други методи (GetById, Post, Put, Delete)
    }

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