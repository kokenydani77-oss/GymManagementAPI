using Microsoft.EntityFrameworkCore;
using GymManagementAPI.Models;

namespace GymManagementAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    //adatbázis táblák
    public DbSet<Member> Members => Set<Member>();
    public DbSet<MembershipPass> Passes => Set<MembershipPass>();
    public DbSet<CheckInLog> CheckInLogs => Set<CheckInLog>();
}