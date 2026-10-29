using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymManagementAPI.Data;
using GymManagementAPI.Models;
using GymManagementAPI.DTOs;

namespace GymManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")] // Az elérés címe a böngészőben: /api/members
public class MembersController : ControllerBase
{
    private readonly AppDbContext _context;

    // A .NET automatikusan ideadja az AppDbContext-et (Dependency Injection)
    public MembersController(AppDbContext context)
    {
        _context = context;
    }

    // 1. Végpont: Összes tag lekérése (GET /api/members)
    [HttpGet]
    public async Task<ActionResult<List<Member>>> GetAllMembers()
    {
        var members = await _context.Members.ToListAsync();
        return Ok(members);
    }

    // 2. Végpont: Egy konkrét tag lekérése Id alapján (GET /api/members/5)
    [HttpGet("{id}")]
    public async Task<ActionResult<Member>> GetMemberById(int id)
    {
        var member = await _context.Members.FindAsync(id);

        if (member == null)
        {
            return NotFound("A megadott azonosítójú tag nem található.");
        }

        return Ok(member);
    }

    // 3. Végpont: Új tag létrehozása (POST /api/members)
    [HttpPost]
    public async Task<ActionResult<Member>> CreateMember(CreateMemberDto dto)
    {
        var newMember = new Member
        {
            FullName = dto.FullName,
            Email = dto.Email,
            CreatedAt = DateTime.UtcNow
        };

        _context.Members.Add(newMember); // Hozzáadjuk az Entity Framework listájához
        await _context.SaveChangesAsync(); // Lefuttatja az SQL INSERT parancsot

        // Visszaküldjük a 201 Created státuszt és az újonnan létrejött tagot az Id-jával együtt
        return CreatedAtAction(nameof(GetMemberById), new { id = newMember.Id }, newMember);
    }
}