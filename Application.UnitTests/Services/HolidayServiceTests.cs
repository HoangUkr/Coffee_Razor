using Application.DTOs.Settings;
using Application.Interfaces;
using Application.Repositories;
using Application.Services;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Application.UnitTests.Services;

[TestClass]
public class HolidayServiceTests
{
    private Mock<IHolidayRepository> _holidayRepositoryMock = null!;
    private Mock<ICacheService> _cacheServiceMock = null!;
    private Mock<ILogger<HolidayService>> _loggerMock = null!;
    private HolidayService _holidayService = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _holidayRepositoryMock = new Mock<IHolidayRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<HolidayService>>();

        _holidayService = new HolidayService(
            _holidayRepositoryMock.Object,
            _cacheServiceMock.Object,
            _loggerMock.Object);
    }

    [TestMethod]
    public void Constructor_WithValidDependencies_InitializesService()
    {
        // Arrange & Act
        var service = new HolidayService(
            _holidayRepositoryMock.Object,
            _cacheServiceMock.Object,
            _loggerMock.Object);

        // Assert
        Assert.IsNotNull(service);
    }

    [TestMethod]
    public async Task GetAllActiveAsync_WhenCacheHit_ReturnsOrderedHolidaysFromCache()
    {
        // Arrange
        var cachedHolidays = new List<HolidayResponse>
        {
            new() { Id = 1, Date = new DateOnly(2024, 12, 25), Name = "Christmas", IsActive = true },
            new() { Id = 2, Date = new DateOnly(2024, 1, 1), Name = "New Year", IsActive = true },
            new() { Id = 3, Date = new DateOnly(2024, 7, 4), Name = "Independence Day", IsActive = true }
        };

        _cacheServiceMock.Setup(x => x.GetAsync<List<HolidayResponse>>("system:holidays"))
            .ReturnsAsync(cachedHolidays);

        // Act
        var result = await _holidayService.GetAllActiveAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.HasCount(3, result);
        Assert.AreEqual(new DateOnly(2024, 1, 1), result[0].Date);
        Assert.AreEqual(new DateOnly(2024, 7, 4), result[1].Date);
        Assert.AreEqual(new DateOnly(2024, 12, 25), result[2].Date);
    }

    [TestMethod]
    public async Task GetAllActiveAsync_WhenCacheMiss_ReturnsOrderedHolidaysFromRepository()
    {
        // Arrange
        var holidays = new List<Holiday>
        {
            new(new DateOnly(2024, 12, 25), "Christmas", false),
            new(new DateOnly(2024, 1, 1), "New Year", true)
        };

        _cacheServiceMock.Setup(x => x.GetAsync<List<HolidayResponse>>("system:holidays"))
            .ReturnsAsync((List<HolidayResponse>?)null);

        _holidayRepositoryMock.Setup(x => x.GetAllActiveAsync())
            .ReturnsAsync(holidays);

        _cacheServiceMock.Setup(x => x.SetAsync(
            "system:holidays",
            It.IsAny<List<HolidayResponse>>(),
            It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _holidayService.GetAllActiveAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.HasCount(2, result);
        Assert.AreEqual(new DateOnly(2024, 1, 1), result[0].Date);
        Assert.AreEqual(new DateOnly(2024, 12, 25), result[1].Date);
        _cacheServiceMock.Verify(x => x.SetAsync(
            "system:holidays",
            It.IsAny<List<HolidayResponse>>(),
            It.IsAny<TimeSpan>()), Times.Once);
    }

    [TestMethod]
    public async Task GetAllActiveAsync_WhenEmptyList_ReturnsEmptyList()
    {
        // Arrange
        _cacheServiceMock.Setup(x => x.GetAsync<List<HolidayResponse>>("system:holidays"))
            .ReturnsAsync(new List<HolidayResponse>());

        // Act
        var result = await _holidayService.GetAllActiveAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public async Task GetByIdAsync_WhenHolidayExists_ReturnsHoliday()
    {
        // Arrange
        var cachedHolidays = new List<HolidayResponse>
        {
            new() { Id = 1, Date = new DateOnly(2024, 1, 1), Name = "New Year", IsActive = true },
            new() { Id = 2, Date = new DateOnly(2024, 12, 25), Name = "Christmas", IsActive = true }
        };

        _cacheServiceMock.Setup(x => x.GetAsync<List<HolidayResponse>>("system:holidays"))
            .ReturnsAsync(cachedHolidays);

        // Act
        var result = await _holidayService.GetByIdAsync(2);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Id);
        Assert.AreEqual("Christmas", result.Name);
    }

    [TestMethod]
    public async Task GetByIdAsync_WhenHolidayDoesNotExist_ReturnsNull()
    {
        // Arrange
        var cachedHolidays = new List<HolidayResponse>
        {
            new() { Id = 1, Date = new DateOnly(2024, 1, 1), Name = "New Year", IsActive = true }
        };

        _cacheServiceMock.Setup(x => x.GetAsync<List<HolidayResponse>>("system:holidays"))
            .ReturnsAsync(cachedHolidays);

        // Act
        var result = await _holidayService.GetByIdAsync(999);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetByIdAsync_WhenEmptyList_ReturnsNull()
    {
        // Arrange
        _cacheServiceMock.Setup(x => x.GetAsync<List<HolidayResponse>>("system:holidays"))
            .ReturnsAsync(new List<HolidayResponse>());

        // Act
        var result = await _holidayService.GetByIdAsync(1);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetHolidayForDateAsync_WhenExactDateMatch_ReturnsHoliday()
    {
        // Arrange
        var targetDate = new DateOnly(2024, 12, 25);
        var cachedHolidays = new List<HolidayResponse>
        {
            new() { Id = 1, Date = targetDate, Name = "Christmas", IsActive = true, IsRecurring = false }
        };

        _cacheServiceMock.Setup(x => x.GetAsync<List<HolidayResponse>>("system:holidays"))
            .ReturnsAsync(cachedHolidays);

        // Act
        var result = await _holidayService.GetHolidayForDateAsync(targetDate);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Christmas", result.Name);
    }

    [TestMethod]
    public async Task GetHolidayForDateAsync_WhenRecurringHolidayMatchesMonthAndDay_ReturnsHoliday()
    {
        // Arrange
        var cachedHolidays = new List<HolidayResponse>
        {
            new() { Id = 1, Date = new DateOnly(2023, 12, 25), Name = "Christmas", IsActive = true, IsRecurring = true }
        };

        _cacheServiceMock.Setup(x => x.GetAsync<List<HolidayResponse>>("system:holidays"))
            .ReturnsAsync(cachedHolidays);

        // Act
        var result = await _holidayService.GetHolidayForDateAsync(new DateOnly(2024, 12, 25));

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Christmas", result.Name);
    }

    [TestMethod]
    public async Task GetHolidayForDateAsync_WhenNoMatch_ReturnsNull()
    {
        // Arrange
        var cachedHolidays = new List<HolidayResponse>
        {
            new() { Id = 1, Date = new DateOnly(2024, 12, 25), Name = "Christmas", IsActive = true, IsRecurring = false }
        };

        _cacheServiceMock.Setup(x => x.GetAsync<List<HolidayResponse>>("system:holidays"))
            .ReturnsAsync(cachedHolidays);

        // Act
        var result = await _holidayService.GetHolidayForDateAsync(new DateOnly(2024, 1, 1));

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetHolidayForDateAsync_WhenHolidayIsNotActive_ReturnsNull()
    {
        // Arrange
        var targetDate = new DateOnly(2024, 12, 25);
        var cachedHolidays = new List<HolidayResponse>
        {
            new() { Id = 1, Date = targetDate, Name = "Christmas", IsActive = false, IsRecurring = false }
        };

        _cacheServiceMock.Setup(x => x.GetAsync<List<HolidayResponse>>("system:holidays"))
            .ReturnsAsync(cachedHolidays);

        // Act
        var result = await _holidayService.GetHolidayForDateAsync(targetDate);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task CreateAsync_WithValidRequest_CreatesHolidayAndInvalidatesCache()
    {
        // Arrange
        var request = new CreateHolidayRequest
        {
            Date = new DateOnly(2024, 12, 25),
            Name = "Christmas",
            IsRecurring = true
        };

        Holiday? capturedHoliday = null;
        _holidayRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Holiday>()))
            .Callback<Holiday>(h => capturedHoliday = h)
            .Returns(Task.CompletedTask);

        _cacheServiceMock.Setup(x => x.RemoveAsync("system:holidays"))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _holidayService.CreateAsync(request);

        // Assert
        Assert.IsNotNull(capturedHoliday);
        Assert.AreEqual(request.Date, capturedHoliday.Date);
        Assert.AreEqual(request.Name, capturedHoliday.Name);
        Assert.AreEqual(request.IsRecurring, capturedHoliday.IsRecurring);
        _holidayRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Holiday>()), Times.Once);
        _cacheServiceMock.Verify(x => x.RemoveAsync("system:holidays"), Times.Once);
    }

    [TestMethod]
    public async Task CreateAsync_WhenCacheInvalidationFails_StillCompletesSuccessfully()
    {
        // Arrange
        var request = new CreateHolidayRequest
        {
            Date = new DateOnly(2024, 1, 1),
            Name = "New Year",
            IsRecurring = false
        };

        _holidayRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Holiday>()))
            .Returns(Task.CompletedTask);

        _cacheServiceMock.Setup(x => x.RemoveAsync("system:holidays"))
            .ThrowsAsync(new Exception("Cache failure"));

        // Act
        var result = await _holidayService.CreateAsync(request);

        // Assert
        _holidayRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Holiday>()), Times.Once);
    }

    [TestMethod]
    public async Task UpdateAsync_WithValidRequest_UpdatesHolidayAndInvalidatesCache()
    {
        // Arrange
        var holidayId = 1;
        var request = new UpdateHolidayRequest
        {
            Date = new DateOnly(2024, 12, 25),
            Name = "Christmas Day",
            IsRecurring = true
        };

        var existingHoliday = new Holiday(new DateOnly(2024, 12, 24), "Christmas Eve", false);

        _holidayRepositoryMock.Setup(x => x.GetByIdAsync(holidayId))
            .ReturnsAsync(existingHoliday);

        _holidayRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Holiday>()))
            .Returns(Task.CompletedTask);

        _cacheServiceMock.Setup(x => x.RemoveAsync("system:holidays"))
            .Returns(Task.CompletedTask);

        // Act
        await _holidayService.UpdateAsync(holidayId, request);

        // Assert
        _holidayRepositoryMock.Verify(x => x.GetByIdAsync(holidayId), Times.Once);
        _holidayRepositoryMock.Verify(x => x.UpdateAsync(existingHoliday), Times.Once);
        _cacheServiceMock.Verify(x => x.RemoveAsync("system:holidays"), Times.Once);
    }

    [TestMethod]
    public async Task UpdateAsync_WhenHolidayNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var holidayId = 999;
        var request = new UpdateHolidayRequest
        {
            Date = new DateOnly(2024, 12, 25),
            Name = "Christmas",
            IsRecurring = true
        };

        _holidayRepositoryMock.Setup(x => x.GetByIdAsync(holidayId))
            .ReturnsAsync((Holiday?)null);

        KeyNotFoundException? exception = null;

        // Act
        try
        {
            await _holidayService.UpdateAsync(holidayId, request);
        }
        catch (KeyNotFoundException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual($"Holiday {holidayId} not found.", exception.Message);
        _holidayRepositoryMock.Verify(x => x.GetByIdAsync(holidayId), Times.Once);
        _holidayRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Holiday>()), Times.Never);
        _cacheServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task UpdateAsync_WhenCacheInvalidationFails_StillCompletesSuccessfully()
    {
        // Arrange
        var holidayId = 1;
        var request = new UpdateHolidayRequest
        {
            Date = new DateOnly(2024, 1, 1),
            Name = "New Year",
            IsRecurring = false
        };

        var existingHoliday = new Holiday(new DateOnly(2023, 12, 31), "Old Year", false);

        _holidayRepositoryMock.Setup(x => x.GetByIdAsync(holidayId))
            .ReturnsAsync(existingHoliday);

        _holidayRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Holiday>()))
            .Returns(Task.CompletedTask);

        _cacheServiceMock.Setup(x => x.RemoveAsync("system:holidays"))
            .ThrowsAsync(new Exception("Cache failure"));

        // Act
        await _holidayService.UpdateAsync(holidayId, request);

        // Assert
        _holidayRepositoryMock.Verify(x => x.GetByIdAsync(holidayId), Times.Once);
        _holidayRepositoryMock.Verify(x => x.UpdateAsync(existingHoliday), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAsync_WithValidId_DeletesHolidayAndInvalidatesCache()
    {
        // Arrange
        var holidayId = 1;

        _holidayRepositoryMock.Setup(x => x.DeleteAsync(holidayId))
            .Returns(Task.CompletedTask);

        _cacheServiceMock.Setup(x => x.RemoveAsync("system:holidays"))
            .Returns(Task.CompletedTask);

        // Act
        await _holidayService.DeleteAsync(holidayId);

        // Assert
        _holidayRepositoryMock.Verify(x => x.DeleteAsync(holidayId), Times.Once);
        _cacheServiceMock.Verify(x => x.RemoveAsync("system:holidays"), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAsync_WhenCacheInvalidationFails_StillCompletesSuccessfully()
    {
        // Arrange
        var holidayId = 1;

        _holidayRepositoryMock.Setup(x => x.DeleteAsync(holidayId))
            .Returns(Task.CompletedTask);

        _cacheServiceMock.Setup(x => x.RemoveAsync("system:holidays"))
            .ThrowsAsync(new Exception("Cache failure"));

        // Act
        await _holidayService.DeleteAsync(holidayId);

        // Assert
        _holidayRepositoryMock.Verify(x => x.DeleteAsync(holidayId), Times.Once);
    }
}
