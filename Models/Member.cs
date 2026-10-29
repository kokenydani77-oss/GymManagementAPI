namespace GymManagementAPI.Models;

public class Member
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Egy taghez több bérlet és több belépés is tartozhat
    public List<MembershipPass> Passes { get; set; } = new();
    public List<CheckInLog> CheckIns { get; set; } = new();
}