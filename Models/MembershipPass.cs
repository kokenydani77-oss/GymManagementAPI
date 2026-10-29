namespace GymManagementAPI.Models;

public enum PassType
{
    Monthly,     // Időalapú (pl. 30 napos korlátlan)
    Occasional   // Alkalom alapú (pl. 10 alkalmas)
}

public class MembershipPass
{
    public int Id { get; set; }
    public PassType Type { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; }
    public int RemainingPasses { get; set; } // Alkalmas bérletnél a maradék alkalmak
    public bool IsActive { get; set; } = true;

    // Relációs kapcsolat (Foreign Key): melyik taghez tartozik a bérlet
    public int MemberId { get; set; }
    public Member? Member { get; set; }
}