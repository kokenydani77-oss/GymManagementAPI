using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymManagementAPI.Data;
using GymManagementAPI.Models;
using GymManagementAPI.DTOs;

namespace GymManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")] // Elérési cím: /api/checkins
public class CheckInsController : ControllerBase
{
    private readonly AppDbContext _context;

    public CheckInsController(AppDbContext context)
    {
        _context = context;
    }

    // 1. Végpont: Beléptetés megkísérlése (POST /api/checkins)
    [HttpPost]
    public async Task<ActionResult<CheckInLog>> CheckIn(CheckInRequestDto dto)
    {
        // 1. Tag ellenőrzése
        var member = await _context.Members
            .Include(m => m.Passes)
            .FirstOrDefaultAsync(m => m.Id == dto.MemberId);

        if (member == null)
        {
            return NotFound("A megadott azonosítójú tag nem található.");
        }

        var now = DateTime.UtcNow;

        // 2. Érvényes bérlet keresése
        // Olyan bérlet kell, ami aktív, nem járt le az ideje, és ha alkalmas, van még belépése
        var validPass = member.Passes.FirstOrDefault(p =>
            p.IsActive &&
            p.EndDate >= now &&
            (p.Type == PassType.Monthly || (p.Type == PassType.Occasional && p.RemainingPasses > 0)));

        // 3. Ha nincs érvényes bérlet: SIKERTELEN belépés naplózása
        if (validPass == null)
        {
            var failedLog = new CheckInLog
            {
                MemberId = member.Id,
                CheckInTime = now,
                IsSuccessful = false,
                RejectionReason = "Nincs érvényes bérlet (lejárt az időtartam vagy elfogytak az alkalmak)."
            };

            _context.CheckInLogs.Add(failedLog);
            await _context.SaveChangesAsync();

            return BadRequest(new { Message = "Belépés megtagadva!", Reason = failedLog.RejectionReason });
        }

        // 4. Ha alkalmas bérlet, levonunk 1 alkalmat
        if (validPass.Type == PassType.Occasional)
        {
            validPass.RemainingPasses--;
            if (validPass.RemainingPasses == 0)
            {
                validPass.IsActive = false; // Ha elfogyott az összes alkalom, inaktiváljuk
            }
        }

        // 5. SIKERES belépés naplózása
        var successLog = new CheckInLog
        {
            MemberId = member.Id,
            CheckInTime = now,
            IsSuccessful = true,
        };

        _context.CheckInLogs.Add(successLog);
        await _context.SaveChangesAsync();

        return Ok(new 
        { 
            Message = $"Sikeres belépés! Jó edzést, {member.FullName}!", 
            PassType = validPass.Type.ToString(),
            RemainingPasses = validPass.RemainingPasses,
            Log = successLog 
        });
    }

    // 2. Végpont: Belépési napló lekérése egy taghoz (GET /api/checkins/member/1)
    [HttpGet("member/{memberId}")]
    public async Task<ActionResult<List<CheckInLog>>> GetLogsForMember(int memberId)
    {
        var logs = await _context.CheckInLogs
            .Where(c => c.MemberId == memberId)
            .OrderByDescending(c => c.CheckInTime) // A legfrissebb belépések legyenek elöl
            .ToListAsync();

        return Ok(logs);
    }
}