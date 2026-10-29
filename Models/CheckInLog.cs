namespace GymManagementAPI.Models;

public class CheckInLog
{
    public int Id { get; set; }
    public DateTime CheckInTime { get; set; } = DateTime.UtcNow;
    public bool IsSuccessful { get; set; } // Sikeres belépés vagy elutasított
    public string RejectionReason { get; set; } = string.Empty; // Pl.: "Lejárt bérlet"

    // Relációs kapcsolat: ki próbált belépni
    public int MemberId { get; set; }
    public Member? Member { get; set; }
}