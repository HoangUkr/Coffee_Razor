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
public class SystemSettingServiceTests
{
    private Mock<ISystemSettingRepository> _repositoryMock = null!;
    private Mock<ICacheService> _cacheServiceMock = null!;
    private Mock<ILogger<SystemSettingService>> _loggerMock = null!;
    private SystemSettingService _systemSettingService = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _repositoryMock = new Mock<ISystemSettingRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<SystemSettingService>>();

        _systemSettingService = new SystemSettingService(
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
            _ = new SystemSettingService(
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
            _ = new SystemSettingService(
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
            _ = new SystemSettingService(
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
    public async Task GetAppSettingsAsync_CachedSettingsExist_ReturnsCachedSettings()
    {
        // Arrange
        var cachedSettings = new AppSettings
        {
            ContactEmail = "cached@test.com",
            ContactPhone = "123456789",
            ContactAddress = "Cached Address",
            ContactFacebook = "facebook.com/cached",
            ContactInstagram = "instagram.com/cached",
            ContactTwitter = "twitter.com/cached",
            EmailConfirmationEnabled = true,
            ShowNotificationCount = false
        };

        _cacheServiceMock
            .Setup(x => x.GetAsync<AppSettings>(It.IsAny<string>()))
            .ReturnsAsync(cachedSettings);

        // Act
        var result = await _systemSettingService.GetAppSettingsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(cachedSettings.ContactEmail, result.ContactEmail);
        Assert.AreEqual(cachedSettings.ContactPhone, result.ContactPhone);
        Assert.AreEqual(cachedSettings.ContactAddress, result.ContactAddress);
        Assert.AreEqual(cachedSettings.ContactFacebook, result.ContactFacebook);
        Assert.AreEqual(cachedSettings.ContactInstagram, result.ContactInstagram);
        Assert.AreEqual(cachedSettings.ContactTwitter, result.ContactTwitter);
        Assert.AreEqual(cachedSettings.EmailConfirmationEnabled, result.EmailConfirmationEnabled);
        Assert.AreEqual(cachedSettings.ShowNotificationCount, result.ShowNotificationCount);

        _repositoryMock.Verify(x => x.GetAllAsync(), Times.Never);
        _cacheServiceMock.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<AppSettings>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    [TestMethod]
    public async Task GetAppSettingsAsync_NoCachedSettings_FetchesFromRepositoryAndCaches()
    {
        // Arrange
        _cacheServiceMock
            .Setup(x => x.GetAsync<AppSettings>(It.IsAny<string>()))
            .ReturnsAsync((AppSettings?)null);

        var systemSettings = new List<SystemSetting>
        {
            new SystemSetting("Contact.Email", "db@test.com"),
            new SystemSetting("Contact.Phone", "987654321"),
            new SystemSetting("Contact.Address", "DB Address"),
            new SystemSetting("Contact.Facebook", "facebook.com/db"),
            new SystemSetting("Contact.Instagram", "instagram.com/db"),
            new SystemSetting("Contact.Twitter", "twitter.com/db"),
            new SystemSetting("Email.ConfirmationEnabled", "false"),
            new SystemSetting("Notification.ShowCount", "true")
        };

        _repositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(systemSettings);

        // Act
        var result = await _systemSettingService.GetAppSettingsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("db@test.com", result.ContactEmail);
        Assert.AreEqual("987654321", result.ContactPhone);
        Assert.AreEqual("DB Address", result.ContactAddress);
        Assert.AreEqual("facebook.com/db", result.ContactFacebook);
        Assert.AreEqual("instagram.com/db", result.ContactInstagram);
        Assert.AreEqual("twitter.com/db", result.ContactTwitter);
        Assert.IsFalse(result.EmailConfirmationEnabled);
        Assert.IsTrue(result.ShowNotificationCount);

        _repositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
        _cacheServiceMock.Verify(x => x.SetAsync("system:settings", It.IsAny<AppSettings>(), It.IsAny<TimeSpan>()), Times.Once);
    }

    [TestMethod]
    public async Task GetAppSettingsAsync_EmptyRepository_ReturnsDefaultSettings()
    {
        // Arrange
        _cacheServiceMock
            .Setup(x => x.GetAsync<AppSettings>(It.IsAny<string>()))
            .ReturnsAsync((AppSettings?)null);

        _repositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<SystemSetting>());

        // Act
        var result = await _systemSettingService.GetAppSettingsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(string.Empty, result.ContactEmail);
        Assert.AreEqual(string.Empty, result.ContactPhone);
        Assert.AreEqual(string.Empty, result.ContactAddress);
        Assert.AreEqual(string.Empty, result.ContactFacebook);
        Assert.AreEqual(string.Empty, result.ContactInstagram);
        Assert.AreEqual(string.Empty, result.ContactTwitter);
        Assert.IsTrue(result.EmailConfirmationEnabled);
        Assert.IsTrue(result.ShowNotificationCount);

        _cacheServiceMock.Verify(x => x.SetAsync("system:settings", It.IsAny<AppSettings>(), It.IsAny<TimeSpan>()), Times.Once);
    }

    [TestMethod]
    public async Task UpdateAsync_ValidRequest_UpdatesRepositoryAndClearsCache()
    {
        // Arrange
        var request = new UpdateSettingsRequest
        {
            ContactEmail = "updated@test.com",
            ContactPhone = "111222333",
            ContactAddress = "Updated Address",
            ContactFacebook = "facebook.com/updated",
            ContactInstagram = "instagram.com/updated",
            ContactTwitter = "twitter.com/updated",
            EmailConfirmationEnabled = true,
            ShowNotificationCount = false
        };

        SystemSetting[]? capturedSettings = null;
        _repositoryMock
            .Setup(x => x.UpsertManyAsync(It.IsAny<IEnumerable<SystemSetting>>()))
            .Callback<IEnumerable<SystemSetting>>(settings => capturedSettings = settings.ToArray())
            .Returns(Task.CompletedTask);

        // Act
        await _systemSettingService.UpdateAsync(request);

        // Assert
        Assert.IsNotNull(capturedSettings);
        Assert.HasCount(8, capturedSettings);
        
        Assert.AreEqual("Contact.Email", capturedSettings[0].Key);
        Assert.AreEqual("updated@test.com", capturedSettings[0].Value);
        
        Assert.AreEqual("Contact.Phone", capturedSettings[1].Key);
        Assert.AreEqual("111222333", capturedSettings[1].Value);
        
        Assert.AreEqual("Contact.Address", capturedSettings[2].Key);
        Assert.AreEqual("Updated Address", capturedSettings[2].Value);
        
        Assert.AreEqual("Contact.Facebook", capturedSettings[3].Key);
        Assert.AreEqual("facebook.com/updated", capturedSettings[3].Value);
        
        Assert.AreEqual("Contact.Instagram", capturedSettings[4].Key);
        Assert.AreEqual("instagram.com/updated", capturedSettings[4].Value);
        
        Assert.AreEqual("Contact.Twitter", capturedSettings[5].Key);
        Assert.AreEqual("twitter.com/updated", capturedSettings[5].Value);
        
        Assert.AreEqual("Email.ConfirmationEnabled", capturedSettings[6].Key);
        Assert.AreEqual("True", capturedSettings[6].Value);
        
        Assert.AreEqual("Notification.ShowCount", capturedSettings[7].Key);
        Assert.AreEqual("False", capturedSettings[7].Value);

        _repositoryMock.Verify(x => x.UpsertManyAsync(It.IsAny<IEnumerable<SystemSetting>>()), Times.Once);
        _cacheServiceMock.Verify(x => x.RemoveAsync("system:settings"), Times.Once);
    }

    [TestMethod]
    public async Task UpdateAsync_RequestWithNullValues_UsesEmptyStrings()
    {
        // Arrange
        var request = new UpdateSettingsRequest
        {
            ContactEmail = null,
            ContactPhone = null,
            ContactAddress = null,
            ContactFacebook = null,
            ContactInstagram = null,
            ContactTwitter = null,
            EmailConfirmationEnabled = false,
            ShowNotificationCount = true
        };

        SystemSetting[]? capturedSettings = null;
        _repositoryMock
            .Setup(x => x.UpsertManyAsync(It.IsAny<IEnumerable<SystemSetting>>()))
            .Callback<IEnumerable<SystemSetting>>(settings => capturedSettings = settings.ToArray())
            .Returns(Task.CompletedTask);

        // Act
        await _systemSettingService.UpdateAsync(request);

        // Assert
        Assert.IsNotNull(capturedSettings);
        Assert.AreEqual(string.Empty, capturedSettings[0].Value);
        Assert.AreEqual(string.Empty, capturedSettings[1].Value);
        Assert.AreEqual(string.Empty, capturedSettings[2].Value);
        Assert.AreEqual(string.Empty, capturedSettings[3].Value);
        Assert.AreEqual(string.Empty, capturedSettings[4].Value);
        Assert.AreEqual(string.Empty, capturedSettings[5].Value);
    }

    [TestMethod]
    public async Task UpdateAsync_NullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        ArgumentNullException? exception = null;

        // Act
        try
        {
            await _systemSettingService.UpdateAsync(null!);
        }
        catch (ArgumentNullException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
    }
}
