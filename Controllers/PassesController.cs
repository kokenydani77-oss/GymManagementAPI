using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymManagementAPI.Data;
using GymManagementAPI.Models;
using GymManagementAPI.DTOs;

namespace GymManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")] // Elérési út: /api/passes
public class PassesController : ControllerBase
{
    private readonly AppDbContext _context;

    public PassesController(AppDbContext context)
    {
        _context = context;
    }

    // 1. Végpont: Egy tag összes bérletének lekérése (GET /api/passes/member/1)
    [HttpGet("member/{memberId}")]
    public async Task<ActionResult<List<MembershipPass>>> GetPassesForMember(int memberId)
    {
        var passes = await _context.Passes
            .Where(p => p.MemberId == memberId)
            .ToListAsync();

        return Ok(passes);
    }

    // 2. Végpont: Új bérlet vásárlása (POST /api/passes/buy)
    [HttpPost("buy")]
    public async Task<ActionResult<MembershipPass>> BuyPass(BuyPassDto dto)
    {
        // 1. Lépés: Létezik-e egyáltalán ez a tag?
        var member = await _context.Members.FindAsync(dto.MemberId);
        if (member == null)
        {
            return NotFound("A megadott azonosítójú tag nem található a rendszerben.");
        }

        // 2. Lépés: Bérlet összeállítása az üzleti szabályok alapján
        var pass = new MembershipPass
        {
            MemberId = dto.MemberId,
            Type = dto.Type,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(dto.DurationInDays),
            RemainingPasses = dto.Type == PassType.Occasional ? dto.TotalPasses : 0,
            IsActive = true
        };

        // 3. Lépés: Mentés az adatbázisba
        _context.Passes.Add(pass);
        await _context.SaveChangesAsync();

        return Ok(pass);
    }
}