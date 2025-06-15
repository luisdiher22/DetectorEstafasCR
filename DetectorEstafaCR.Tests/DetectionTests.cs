using DetectorEstafaCR.Controllers;
using DetectorEstafaCR.Data;
using DetectorEstafaCR.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DetectorEstafaCR.Tests;

public class DetectionTests
{
    private static ApplicationDbContext GetInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Detect_Flags_Message_With_Keyword()
    {
        using var db = GetInMemoryDb();
        var controller = new HomeController(Microsoft.Extensions.Logging.Abstractions.NullLogger<HomeController>.Instance, db);
        var input = new DetectInputModel { ContactInfo = "test@example.com", Message = "Ganaste un premio" };

        var result = await controller.Detect(input) as ViewResult;
        Assert.NotNull(result);
        var model = Assert.IsType<DetectResultViewModel>(result.Model);
        Assert.True(model.IsPotentialScam);
    }

    [Fact]
    public async Task Detect_NotFlag_When_No_Indicators()
    {
        using var db = GetInMemoryDb();
        var controller = new HomeController(Microsoft.Extensions.Logging.Abstractions.NullLogger<HomeController>.Instance, db);
        var input = new DetectInputModel { ContactInfo = "user@example.com", Message = "Hola, ¿cómo estás?" };

        var result = await controller.Detect(input) as ViewResult;
        Assert.NotNull(result);
        var model = Assert.IsType<DetectResultViewModel>(result.Model);
        Assert.False(model.IsPotentialScam);
    }
}
