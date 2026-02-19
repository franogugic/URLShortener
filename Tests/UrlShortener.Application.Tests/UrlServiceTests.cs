using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using UrlShortener.Application.URLs.DTO_s;
using UrlShortener.Application.URLs.Exceptions;
using UrlShortener.Application.URLs.Interfaces;
using UrlShortener.Application.URLs.Services;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Tests;

public class UrlServiceTests
{
    private readonly Mock<IUrlRepository> _urlRepositoryMock;
    private readonly Mock<IUrlCache> _urlCacheMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<UrlService>> _loggerMock;

    private readonly UrlService _sut;

    public UrlServiceTests()
    {
        _urlRepositoryMock = new Mock<IUrlRepository>();
        _urlCacheMock = new Mock<IUrlCache>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<UrlService>>();

        _sut = new UrlService(
            _loggerMock.Object,
            userService: null!,
            _urlRepositoryMock.Object,
            _mapperMock.Object,
            _urlCacheMock.Object
        );
    }

    [Fact]
    public async Task CreateAsync_WhenShortCodeDoesNotExist_CreatesUrl()
    {
        var request = new CreateUrlRequestDTO
        {
            Description = "test description",
            LongUrl = "https://www.google.com",
            ShortUrlCode = "ggl"
        };

        var user = User.Create("TestUser", "TestPassword");

        _urlRepositoryMock
            .Setup(r => r.GetUrlByShortCode(request.ShortUrlCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Url?)null);

        _urlRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Url>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mapperMock
            .Setup(m => m.Map<CreateUrlResponseDTO>(It.IsAny<Url>()))
            .Returns(new CreateUrlResponseDTO
            {
                ShortUrlCode = request.ShortUrlCode,
                LongUrl = request.LongUrl
            });

        var result = await _sut.CreateAsync(request, user, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(request.ShortUrlCode, result.ShortUrlCode);
        Assert.Equal(request.LongUrl, result.LongUrl);

        _urlRepositoryMock.Verify(r => r.GetUrlByShortCode(request.ShortUrlCode, It.IsAny<CancellationToken>()), Times.Once);
        _urlRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Url>(), It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<CreateUrlResponseDTO>(It.IsAny<Url>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenShortCodeAlreadyExists_ThrowsException()
    {
        var request = new CreateUrlRequestDTO
        {
            Description = "Test",
            LongUrl = "https://www.google.com",
            ShortUrlCode = "ggl"
        };
        var user = User.Create("TestUser", "TestPassword");

        _urlRepositoryMock
            .Setup(r => r.GetUrlByShortCode(request.ShortUrlCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Url.Create("ggl", "https://www.google.com", "desc", user));

        var exception = await Assert.ThrowsAsync<ShortUrlCodeAlreadyExistsException>(
            () => _sut.CreateAsync(request, user, CancellationToken.None)
        );

        Assert.Equal(request.ShortUrlCode, exception.ShortUrlCode);

        _urlRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Url>(), It.IsAny<CancellationToken>()), Times.Never);
        _mapperMock.Verify(m => m.Map<CreateUrlResponseDTO>(It.IsAny<Url>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenRepositoryThrowsException_PropagatesException()
    {
        var request = new CreateUrlRequestDTO
        {
            Description = "Test",
            LongUrl = "https://www.google.com",
            ShortUrlCode = "ggl"
        };
        var user = User.Create("TestUser", "TestPassword");

        _urlRepositoryMock
            .Setup(r => r.GetUrlByShortCode(request.ShortUrlCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Url?)null);

        _urlRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Url>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB failure"));

        var exception = await Assert.ThrowsAsync<Exception>(
            () => _sut.CreateAsync(request, user, CancellationToken.None)
        );

        Assert.Equal("DB failure", exception.Message);
        _mapperMock.Verify(m => m.Map<CreateUrlResponseDTO>(It.IsAny<Url>()), Times.Never);
    }

    [Fact]
    public async Task GetAllUrlsByUser_ReturnsMappedUrls_WhenRepoReturnsUrls()
    {
        var userId = Guid.NewGuid();
        var urlList = new List<Url>
        {
            Url.Create("code1", "https://site1.com", "desc1", User.Create("User1", "pass")),
            Url.Create("code2", "https://site2.com", "desc2", User.Create("User2", "pass"))
        };

        _urlRepositoryMock
            .Setup(r => r.GetAllUrlsByUser(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(urlList);

        _mapperMock
            .Setup(m => m.Map<List<CreateUrlResponseDTO>>(It.IsAny<List<Url>>()))
            .Returns((List<Url> urls) => urls.Select(u => new CreateUrlResponseDTO
            {
                ShortUrlCode = u.ShortUrlCode,
                LongUrl = u.LongUrl,
                Description = u.Description
            }).ToList());

        var result = await _sut.GetAllUrlsByUser(userId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(urlList.Count, result.Count);

        _urlRepositoryMock.Verify(r => r.GetAllUrlsByUser(userId, It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<List<CreateUrlResponseDTO>>(It.IsAny<List<Url>>()), Times.Once);
    }

    [Fact]
    public async Task GetAllUrlsByUser_ThrowsException_WhenRepoThrows()
    {
        var userId = Guid.NewGuid();
        _urlRepositoryMock
            .Setup(r => r.GetAllUrlsByUser(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _sut.GetAllUrlsByUser(userId, CancellationToken.None));

        Assert.Equal("Database error", exception.Message);
        _urlRepositoryMock.Verify(r => r.GetAllUrlsByUser(userId, It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<List<CreateUrlResponseDTO>>(It.IsAny<List<Url>>()), Times.Never);
    }

    [Fact]
    public async Task GetUrlById_WhenUrlExistsAndUserIsOwner_ReturnsDto()
    {
        var user = User.Create("TestUser", "TestPassword");
        var url = Url.Create("code", "https://example.com", "desc", user);

        _urlRepositoryMock
            .Setup(r => r.GetUrlById(url.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(url);

        _mapperMock
            .Setup(m => m.Map<CreateUrlResponseDTO>(url))
            .Returns(new CreateUrlResponseDTO
            {
                ShortUrlCode = url.ShortUrlCode,
                LongUrl = url.LongUrl,
                Description = url.Description
            });

        var result = await _sut.GetUrlById(url.Id, user.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(url.ShortUrlCode, result.ShortUrlCode);
    }

    [Fact]
    public async Task GetUrlById_WhenUrlDoesNotExist_ThrowsUrlNotFoundException()
    {
        var id = Guid.NewGuid();
        _urlRepositoryMock
            .Setup(r => r.GetUrlById(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Url?)null);

        await Assert.ThrowsAsync<UrlNotFoundException>(() => _sut.GetUrlById(id, Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task GetUrlById_WhenUserIsNotOwner_ThrowsForbiddenException()
    {
        var owner = User.Create("Owner", "pass");
        var url = Url.Create("code", "https://example.com", "desc", owner);

        _urlRepositoryMock
            .Setup(r => r.GetUrlById(url.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(url);

        await Assert.ThrowsAsync<ForbiddenException>(() => _sut.GetUrlById(url.Id, Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task DeleteAsync_WhenUrlExistsAndUserIsOwner_DeletesUrlAndCache()
    {
        var user = User.Create("TestUser", "pass");
        var url = Url.Create("code", "https://example.com", "desc", user);

        _urlRepositoryMock.Setup(r => r.GetUrlById(url.Id, It.IsAny<CancellationToken>())).ReturnsAsync(url);
        _urlRepositoryMock.Setup(r => r.DeleteAsync(url, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _urlCacheMock.Setup(c => c.RemoveAsync(url.ShortUrlCode, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await _sut.DeleteAsync(url.Id, user.Id, CancellationToken.None);

        _urlRepositoryMock.Verify(r => r.DeleteAsync(url, It.IsAny<CancellationToken>()), Times.Once);
        _urlCacheMock.Verify(c => c.RemoveAsync(url.ShortUrlCode, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetLongUrlByCode_WhenFound_UpdatesClicksAndReturnsLongUrl()
    {
        var user = User.Create("User", "pass");
        var url = Url.Create("code", "https://example.com", "desc", user);

        _urlCacheMock.Setup(c => c.GetLongUrlAsync("code", It.IsAny<CancellationToken>())).ReturnsAsync((string?)null);
        _urlRepositoryMock.Setup(r => r.GetUrlByShortCode("code", It.IsAny<CancellationToken>())).ReturnsAsync(url);
        _urlRepositoryMock.Setup(r => r.UpdateAsync(url, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _urlCacheMock.Setup(c => c.SetLongUrlAsync("code", url.LongUrl, It.IsAny<CancellationToken>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.GetLongUrlByCode("code", CancellationToken.None);

        Assert.Equal(url.LongUrl, result);
        Assert.Equal(1, url.Clicks);
        _urlRepositoryMock.Verify(r => r.UpdateAsync(url, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetLongUrlByCode_WhenCacheHit_StillUpdatesClicksAndReturnsCachedLongUrl()
    {
        var user = User.Create("User", "pass");
        var url = Url.Create("code", "https://example.com", "desc", user);

        _urlCacheMock.Setup(c => c.GetLongUrlAsync("code", It.IsAny<CancellationToken>())).ReturnsAsync("https://cached.com");
        _urlRepositoryMock.Setup(r => r.GetUrlByShortCode("code", It.IsAny<CancellationToken>())).ReturnsAsync(url);
        _urlRepositoryMock.Setup(r => r.UpdateAsync(url, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _sut.GetLongUrlByCode("code", CancellationToken.None);

        Assert.Equal("https://cached.com", result);
        Assert.Equal(1, url.Clicks);
        _urlRepositoryMock.Verify(r => r.UpdateAsync(url, It.IsAny<CancellationToken>()), Times.Once);
        _urlCacheMock.Verify(c => c.SetLongUrlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<TimeSpan>()), Times.Never);
    }
}
