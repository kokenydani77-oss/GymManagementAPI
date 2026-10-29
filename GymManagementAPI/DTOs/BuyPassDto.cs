using GymManagementAPI.Models;

namespace GymManagementAPI.DTOs;

public class BuyPassDto
{
    public int MemberId { get; set; }
    public PassType Type { get; set; } // 0 = Monthly, 1 = Occasional
    public int DurationInDays { get; set; } = 30; // Alapértelmezetten 30 nap
    public int TotalPasses { get; set; } = 10;     // Alkalmas bérletnél alapból 10 alkalom
}