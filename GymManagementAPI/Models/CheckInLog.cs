namespace GymManagementAPI.Models;

public class CheckInLog
{
    public int Id { get; set; }

    public int MemberId { get; set; }
    public Member? Member { get; set; }

    public DateTime CheckInTime { get; set; } = DateTime.UtcNow;

    // amikor belép, ez még null, mert nem tudjuk, mikor megy haza
    // amikor kilép, ide írjuk be a távozás pontos idejét
    public DateTime? CheckOutTime { get; set; }

    public bool IsSuccessful { get; set; }
    public string? RejectionReason { get; set; }
}