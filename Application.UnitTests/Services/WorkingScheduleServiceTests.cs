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
public class WorkingScheduleServiceTests
{
    private Mock<IWorkingScheduleRepository> _repositoryMock = null!;
    private Mock<ICacheService> _cacheServiceMock = null!;
    private Mock<ILogger<WorkingScheduleService>> _loggerMock = null!;
    private WorkingScheduleService _service = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _repositoryMock = new Mock<IWorkingScheduleRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<WorkingScheduleService>>();

        _service = new WorkingScheduleService(
            _repositoryMock.Object,
            _cacheServiceMock.Object,
            _loggerMock.Object
        );
    }

    [TestMethod]
    public void Constructor_NullRepository_ThrowsArgumentNullException()
    {
        // Arrange
        ArgumentNullException? exception = null;

        // Act
        try
        {
            _ = new WorkingScheduleService(
                null!,
                _cacheServiceMock.Object,
                _loggerMock.Object
            );
        }
        catch (ArgumentNullException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
    }

    [TestMethod]
    public void Constructor_NullCacheService_ThrowsArgumentNullException()
    {
        // Arrange
        ArgumentNullException? exception = null;

        // Act
        try
        {
            _ = new WorkingScheduleService(
                _repositoryMock.Object,
                null!,
                _loggerMock.Object
            );
        }
        catch (ArgumentNullException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
    }

    [TestMethod]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        ArgumentNullException? exception = null;

        // Act
        try
        {
            _ = new WorkingScheduleService(
                _repositoryMock.Object,
                _cacheServiceMock.Object,
                null!
            );
        }
        catch (ArgumentNullException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
    }

    [TestMethod]
    public async Task GetScheduleAsync_CachedDataExists_ReturnsCachedData()
    {
        // Arrange
        var cachedSchedule = new List<WorkingScheduleEntry>
        {
            new() { Day = DayOfWeek.Monday, DayName = "Monday", OpenTime = new TimeOnly(9, 0), CloseTime = new TimeOnly(17, 0), IsClosed = false }
        };

        _cacheServiceMock.Setup(x => x.GetAsync<List<WorkingScheduleEntry>>("system:schedule"))
            .ReturnsAsync(cachedSchedule);

        // Act
        var result = await _service.GetScheduleAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
        Assert.AreEqual(DayOfWeek.Monday, result[0].Day);
        _cacheServiceMock.Verify(x => x.GetAsync<List<WorkingScheduleEntry>>("system:schedule"), Times.Once);
        _repositoryMock.Verify(x => x.GetAllAsync(), Times.Never);
    }

    [TestMethod]
    public async Task GetScheduleAsync_NoCacheAllDaysInDb_ReturnsDbDataAndCaches()
    {
        // Arrange
        _cacheServiceMock.Setup(x => x.GetAsync<List<WorkingScheduleEntry>>("system:schedule"))
            .ReturnsAsync((List<WorkingScheduleEntry>?)null);

        var dbSchedules = new List<WorkingSchedule>
        {
            new(DayOfWeek.Monday, new TimeOnly(8, 0), new TimeOnly(20, 0), false),
            new(DayOfWeek.Tuesday, new TimeOnly(8, 0), new TimeOnly(20, 0), false),
            new(DayOfWeek.Wednesday, new TimeOnly(8, 0), new TimeOnly(20, 0), false),
            new(DayOfWeek.Thursday, new TimeOnly(8, 0), new TimeOnly(20, 0), false),
            new(DayOfWeek.Friday, new TimeOnly(8, 0), new TimeOnly(20, 0), false),
            new(DayOfWeek.Saturday, new TimeOnly(9, 0), new TimeOnly(18, 0), false),
            new(DayOfWeek.Sunday, new TimeOnly(10, 0), new TimeOnly(16, 0), true)
        };

        _repositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(dbSchedules);

        // Act
        var result = await _service.GetScheduleAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.HasCount(7, result);
        Assert.AreEqual(DayOfWeek.Monday, result[0].Day);
        Assert.AreEqual(new TimeOnly(8, 0), result[0].OpenTime);
        Assert.AreEqual(new TimeOnly(20, 0), result[0].CloseTime);
        Assert.IsFalse(result[0].IsClosed);
        Assert.AreEqual(DayOfWeek.Sunday, result[6].Day);
        Assert.IsTrue(result[6].IsClosed);
        _repositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
        _cacheServiceMock.Verify(x => x.SetAsync("system:schedule", It.IsAny<List<WorkingScheduleEntry>>(), TimeSpan.FromMinutes(60)), Times.Once);
    }

    [TestMethod]
    public async Task GetScheduleAsync_NoCacheMissingDaysInDb_ReturnsMergedDataWithDefaults()
    {
        // Arrange
        _cacheServiceMock.Setup(x => x.GetAsync<List<WorkingScheduleEntry>>("system:schedule"))
            .ReturnsAsync((List<WorkingScheduleEntry>?)null);

        var dbSchedules = new List<WorkingSchedule>
        {
            new(DayOfWeek.Monday, new TimeOnly(8, 0), new TimeOnly(20, 0), false),
            new(DayOfWeek.Wednesday, new TimeOnly(8, 0), new TimeOnly(20, 0), false)
        };

        _repositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(dbSchedules);

        // Act
        var result = await _service.GetScheduleAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.HasCount(7, result);
        
        // Monday from DB
        Assert.AreEqual(DayOfWeek.Monday, result[0].Day);
        Assert.AreEqual(new TimeOnly(8, 0), result[0].OpenTime);
        Assert.AreEqual(new TimeOnly(20, 0), result[0].CloseTime);
        
        // Tuesday default fallback
        Assert.AreEqual(DayOfWeek.Tuesday, result[1].Day);
        Assert.AreEqual(new TimeOnly(8, 0), result[1].OpenTime);
        Assert.AreEqual(new TimeOnly(22, 0), result[1].CloseTime);
        Assert.IsFalse(result[1].IsClosed);
        
        // Wednesday from DB
        Assert.AreEqual(DayOfWeek.Wednesday, result[2].Day);
        Assert.AreEqual(new TimeOnly(8, 0), result[2].OpenTime);
        Assert.AreEqual(new TimeOnly(20, 0), result[2].CloseTime);
        
        _repositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
        _cacheServiceMock.Verify(x => x.SetAsync("system:schedule", It.IsAny<List<WorkingScheduleEntry>>(), TimeSpan.FromMinutes(60)), Times.Once);
    }

    [TestMethod]
    public async Task UpdateScheduleAsync_NullRequests_ThrowsArgumentNullException()
    {
        // Arrange
        ArgumentNullException? exception = null;

        // Act
        try
        {
            await _service.UpdateScheduleAsync(null!);
        }
        catch (ArgumentNullException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
    }

    [TestMethod]
    public async Task UpdateScheduleAsync_ValidRequests_UpdatesAndInvalidatesCache()
    {
        // Arrange
        var requests = new List<UpdateScheduleDayRequest>
        {
            new() { Day = DayOfWeek.Monday, OpenTime = "09:00", CloseTime = "18:00", IsClosed = false },
            new() { Day = DayOfWeek.Sunday, OpenTime = "10:00", CloseTime = "16:00", IsClosed = true }
        };

        // Act
        await _service.UpdateScheduleAsync(requests);

        // Assert
        _repositoryMock.Verify(x => x.UpsertManyAsync(It.Is<IEnumerable<WorkingSchedule>>(schedules =>
            schedules.Count() == 2 &&
            schedules.ElementAt(0).Day == DayOfWeek.Monday &&
            schedules.ElementAt(0).OpenTime == new TimeOnly(9, 0) &&
            schedules.ElementAt(0).CloseTime == new TimeOnly(18, 0) &&
            schedules.ElementAt(0).IsClosed == false &&
            schedules.ElementAt(1).Day == DayOfWeek.Sunday &&
            schedules.ElementAt(1).OpenTime == new TimeOnly(10, 0) &&
            schedules.ElementAt(1).CloseTime == new TimeOnly(16, 0) &&
            schedules.ElementAt(1).IsClosed == true
        )), Times.Once);
        _cacheServiceMock.Verify(x => x.RemoveAsync("system:schedule"), Times.Once);
    }
}
