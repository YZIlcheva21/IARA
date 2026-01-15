using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using IARA.Domain.Models; 
using IARA.API.Data; // Тук се намира твоят IARAContext

namespace IARA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AmateurCatchesController : ControllerBase
    {
        // 👇 ПРОМЯНА 1: Използваме IARAContext, а не ApplicationDbContext
        private readonly IARAContext _context;

        // 👇 ПРОМЯНА 2: Инжектираме IARAContext в конструктора
        public AmateurCatchesController(IARAContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AmateurCatch>>> GetMyCatches()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return await _context.AmateurCatches
                .Include(c => c.AmateurTicket) // Зареждаме и билета, ако има такъв
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CatchDate)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<AmateurCatch>> PostCatch(AmateurCatch amateurCatch)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Свързваме улова с текущия потребител
            amateurCatch.UserId = userId ?? "unknown";
            
            // Уверяваме се, че датите са верни
            if (amateurCatch.CatchDate == default) 
                amateurCatch.CatchDate = DateTime.UtcNow;
            
            amateurCatch.CreatedAt = DateTime.UtcNow;

            _context.AmateurCatches.Add(amateurCatch);
            await _context.SaveChangesAsync();

            return Ok(amateurCatch);
        }
    }
}