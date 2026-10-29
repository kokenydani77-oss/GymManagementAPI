using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymManagementAPI.Data;
using GymManagementAPI.Models;
using GymManagementAPI.DTOs;

namespace GymManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")] // /api/checkins
public class CheckInsController : ControllerBase
{
    private readonly AppDbContext _context;

    public CheckInsController(AppDbContext context)
    {
        _context = context;
    }

    // 1. BELÉPTETÉS: POST /api/checkins
    [HttpPost]
    public async Task<IActionResult> CheckIn(CheckInRequestDto request)
    {
        // Tag ellenőrzése
        var member = await _context.Members
            .Include(m => m.Passes)
            .FirstOrDefaultAsync(m => m.Id == request.MemberId);

        if (member == null)
        {
            return NotFound($"A(z) {request.MemberId} azonosítójú tag nem található.");
        }

        // VÉDELEM: Ellenőrizzük, hogy nincs-e már bent a teremben
        var alreadyInside = await _context.CheckInLogs
            .AnyAsync(log => log.MemberId == request.MemberId && log.IsSuccessful && log.CheckOutTime == null);

        if (alreadyInside)
        {
            var duplicateRejectedLog = new CheckInLog
            {
                MemberId = request.MemberId,
                CheckInTime = DateTime.UtcNow,
                IsSuccessful = false,
                RejectionReason = "A tag már bent tartózkodik a teremben."
            };
            _context.CheckInLogs.Add(duplicateRejectedLog);
            await _context.SaveChangesAsync();

            return BadRequest(new
            {
                message = "Belépés megtagadva!",
                reason = "Már bent vagy a teremben! Előbb jelentkezz ki a kapunál."
            });
        }

        // Érvényes bérlet keresése
        var now = DateTime.UtcNow;
        var validPass = member.Passes.FirstOrDefault(p =>
            p.IsActive &&
            (
                (p.Type == PassType.Monthly && p.EndDate >= now) ||
                (p.Type == PassType.Occasional && p.RemainingPasses > 0)
            )
        );

        // Ha nincs érvényes bérlet: elutasítás naplózása
        if (validPass == null)
        {
            var rejectedLog = new CheckInLog
            {
                MemberId = request.MemberId,
                CheckInTime = now,
                IsSuccessful = false,
                RejectionReason = "Nincs érvényes bérlet (lejárt vagy elfogyott)."
            };
            _context.CheckInLogs.Add(rejectedLog);
            await _context.SaveChangesAsync();

            return BadRequest(new
            {
                message = "Belépés megtagadva!",
                reason = "Nincs érvényes bérlet (lejárt az időtartam vagy elfogytak az alkalmak)."
            });
        }

        // 10 alkalmas bérlet esetén csökkentjük az alkalmak számát
        if (validPass.Type == PassType.Occasional)
        {
            validPass.RemainingPasses--;
            if (validPass.RemainingPasses == 0)
            {
                validPass.IsActive = false;
            }
        }

        // Sikeres belépés naplózása (CheckOutTime induláskor null)
        var successLog = new CheckInLog
        {
            MemberId = request.MemberId,
            CheckInTime = now,
            CheckOutTime = null,
            IsSuccessful = true
        };
        _context.CheckInLogs.Add(successLog);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = $"Sikeres belépés! Jó edzést, {member.FullName}!",
            passType = validPass.Type.ToString(),
            remainingPasses = validPass.RemainingPasses,
            log = successLog
        });
    }

    // 2. KILÉPTETÉS: POST /api/checkins/checkout
    [HttpPost("checkout")]
    public async Task<IActionResult> CheckOut(CheckOutRequestDto request)
    {
        // Megkeressük a tag legutóbbi bentlétét, ahol még nincs kilépési idő
        var activeStay = await _context.CheckInLogs
            .Where(log => log.MemberId == request.MemberId && log.IsSuccessful && log.CheckOutTime == null)
            .OrderByDescending(log => log.CheckInTime)
            .FirstOrDefaultAsync();

        if (activeStay == null)
        {
            return BadRequest(new
            {
                message = "Kiléptetés sikertelen!",
                reason = "Ez a tag jelenleg nincs bent a teremben, vagy már kijelentkezett."
            });
        }

        // Rögzítjük a távozás idejét
        activeStay.CheckOutTime = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var duration = activeStay.CheckOutTime.Value - activeStay.CheckInTime;

        return Ok(new
        {
            message = "Sikeres kilépés! Viszlát legközelebb!",
            checkInTime = activeStay.CheckInTime,
            checkOutTime = activeStay.CheckOutTime,
            stayDurationMinutes = Math.Round(duration.TotalMinutes, 1)
        });
    }

    // 3. ÉLŐ LÉTSZÁM LEKÉRDEZÉSE: GET /api/checkins/occupancy
    [HttpGet("occupancy")]
    public async Task<IActionResult> GetCurrentOccupancy()
    {
        var currentCount = await _context.CheckInLogs
            .CountAsync(log => log.IsSuccessful && log.CheckOutTime == null);

        var peopleInside = await _context.CheckInLogs
            .Where(log => log.IsSuccessful && log.CheckOutTime == null)
            .Include(log => log.Member)
            .Select(log => new
            {
                memberId = log.MemberId,
                memberName = log.Member != null ? log.Member.FullName : "Ismeretlen",
                enteredAt = log.CheckInTime
            })
            .ToListAsync();

        return Ok(new
        {
            currentPeopleCount = currentCount,
            membersInside = peopleInside
        });
    }

    // 4. EGY TAG BELÉPÉSI ELŐZMÉNYEI: GET /api/checkins/member/1
    [HttpGet("member/{memberId}")]
    public async Task<IActionResult> GetMemberCheckIns(int memberId)
    {
        var logs = await _context.CheckInLogs
            .Where(log => log.MemberId == memberId)
            .OrderByDescending(log => log.CheckInTime)
            .ToListAsync();

        return Ok(logs);
    }
}