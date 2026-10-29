using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymManagementAPI.Data;
using GymManagementAPI.Models;
using GymManagementAPI.DTOs;

namespace GymManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")] // /api/members
public class MembersController : ControllerBase
{
    private readonly AppDbContext _context;

    public MembersController(AppDbContext context)
    {
        _context = context;
    }

    // 1. READ ALL - Összes tag lekérése (GET /api/members)
    [HttpGet]
    public async Task<ActionResult<List<Member>>> GetAllMembers()
    {
        var members = await _context.Members
            .Include(m => m.Passes)
            .ToListAsync();

        return Ok(members);
    }

    // 2. READ ONE - Egy konkrét tag lekérése ID alapján (GET /api/members/1)
    [HttpGet("{id}")]
    public async Task<ActionResult<Member>> GetMemberById(int id)
    {
        var member = await _context.Members
            .Include(m => m.Passes)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (member == null)
        {
            return NotFound($"A(z) {id} azonosítójú tag nem található.");
        }

        return Ok(member);
    }

    // 3. CREATE - Új tag regisztrálása (POST /api/members)
    [HttpPost]
    public async Task<ActionResult<Member>> CreateMember(CreateMemberDto dto)
    {
        var member = new Member
        {
            FullName = dto.FullName,
            Email = dto.Email,
            CreatedAt = DateTime.UtcNow
        };

        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMemberById), new { id = member.Id }, member);
    }

    // 4. UPDATE - Meglévő tag adatainak frissítése (PUT /api/members/1)
    [HttpPut("{id}")]
    public async Task<ActionResult<Member>> UpdateMember(int id, UpdateMemberDto dto)
    {
        var member = await _context.Members.FindAsync(id);
        if (member == null)
        {
            return NotFound($"A(z) {id} azonosítójú tag nem található.");
        }

        // Adatok felülírása a bejövő DTO alapján
        member.FullName = dto.FullName;
        member.Email = dto.Email;

        await _context.SaveChangesAsync();

        return Ok(member);
    }

    // 5. DELETE - Tag törlése az adatbázisból (DELETE /api/members/1)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMember(int id)
    {
        var member = await _context.Members.FindAsync(id);
        if (member == null)
        {
            return NotFound($"A(z) {id} azonosítójú tag nem található.");
        }

        _context.Members.Remove(member);
        await _context.SaveChangesAsync();

        return NoContent(); // 204 No Content: a törlés sikeres, választest nem szükséges
    }
}