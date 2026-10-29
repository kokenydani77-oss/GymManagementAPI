using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using GymManagementAPI.Controllers;
using GymManagementAPI.Data;
using GymManagementAPI.Models;
using GymManagementAPI.DTOs;

namespace GymManagementAPI.Tests;

public class CheckInTests
{
    private AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task CheckIn_ShouldReject_WhenMemberAlreadyInside()
    {
        // 1. ARRANGE
        using var context = GetInMemoryDbContext();
        var member = new Member 
        { 
            Id = 1, 
            FullName = "John Doe", 
            Email = "john.doe@example.com" 
        };
        var pass = new MembershipPass
        {
            Id = 1,
            MemberId = 1,
            Type = PassType.Monthly,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(20),
            IsActive = true
        };
        var activeLog = new CheckInLog
        {
            MemberId = 1,
            CheckInTime = DateTime.UtcNow.AddMinutes(-30),
            CheckOutTime = null,
            IsSuccessful = true
        };

        context.Members.Add(member);
        context.Passes.Add(pass);
        context.CheckInLogs.Add(activeLog);
        await context.SaveChangesAsync();

        var controller = new CheckInsController(context);

        // 2. ACT
        var result = await controller.CheckIn(new CheckInRequestDto { MemberId = 1 });

        // 3. ASSERT
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task CheckIn_ShouldSucceed_WhenPassIsValid()
    {
        // 1. ARRANGE
        using var context = GetInMemoryDbContext();
        var member = new Member 
        { 
            Id = 2, 
            FullName = "Jane Doe", 
            Email = "jane.doe@example.com" 
        };
        var pass = new MembershipPass
        {
            Id = 2,
            MemberId = 2,
            Type = PassType.Monthly,
            StartDate = DateTime.UtcNow.AddDays(-5),
            EndDate = DateTime.UtcNow.AddDays(25),
            IsActive = true
        };

        context.Members.Add(member);
        context.Passes.Add(pass);
        await context.SaveChangesAsync();

        var controller = new CheckInsController(context);

        // 2. ACT
        var result = await controller.CheckIn(new CheckInRequestDto { MemberId = 2 });

        // 3. ASSERT
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        var savedLog = await context.CheckInLogs.FirstOrDefaultAsync(l => l.MemberId == 2 && l.IsSuccessful);
        Assert.NotNull(savedLog);
        Assert.Null(savedLog.CheckOutTime);
    }
    [Fact]
    public async Task CheckIn_ShouldReject_WhenPassIsExpired()
    {
        // 1. ARRANGE: Tag lejárt bérlettel
        using var context = GetInMemoryDbContext();
        var member = new Member 
        { 
            Id = 3, 
            FullName = "Expired User", 
            Email = "expired.user@example.com" 
        };
        var expiredPass = new MembershipPass
        {
            Id = 3,
            MemberId = 3,
            Type = PassType.Monthly,
            StartDate = DateTime.UtcNow.AddDays(-60),
            EndDate = DateTime.UtcNow.AddDays(-30), // 30 napja lejárt
            IsActive = true
        };

        context.Members.Add(member);
        context.Passes.Add(expiredPass);
        await context.SaveChangesAsync();

        var controller = new CheckInsController(context);

        // 2. ACT
        var result = await controller.CheckIn(new CheckInRequestDto { MemberId = 3 });

        // 3. ASSERT: Elutasítás elvárása (400 Bad Request)
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);

        // Ellenőrizzük, hogy elutasított státuszú log mentődött-e
        var savedLog = await context.CheckInLogs.FirstOrDefaultAsync(l => l.MemberId == 3);
        Assert.NotNull(savedLog);
        Assert.False(savedLog.IsSuccessful);
    }

    [Fact]
    public async Task CheckIn_ShouldDecrementRemainingPasses_WhenPassIsOccasional()
    {
        // 1. ARRANGE: Tag 5 alkalmas bérlettel
        using var context = GetInMemoryDbContext();
        var member = new Member 
        { 
            Id = 4, 
            FullName = "Punch Card User", 
            Email = "punchcard.user@example.com" 
        };
        var occasionalPass = new MembershipPass
        {
            Id = 4,
            MemberId = 4,
            Type = PassType.Occasional,
            RemainingPasses = 5,
            IsActive = true
        };

        context.Members.Add(member);
        context.Passes.Add(occasionalPass);
        await context.SaveChangesAsync();

        var controller = new CheckInsController(context);

        // 2. ACT
        var result = await controller.CheckIn(new CheckInRequestDto { MemberId = 4 });

        // 3. ASSERT: Sikeres belépés, és a bérlet 4 alkalomra csökkent
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        var updatedPass = await context.Passes.FindAsync(4);
        Assert.NotNull(updatedPass);
        Assert.Equal(4, updatedPass.RemainingPasses);
    }
}