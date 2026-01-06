using Moq;
using StackExchange.Redis;
using UrlShortener.Application.URLs.Services;

namespace UrlShortener.Application.Tests;

public class RedisUrlCacheTests
{
    private readonly Mock<IDatabase> _dbMock;
    private readonly RedisUrlCache _redisUrlCache;

    public RedisUrlCacheTests()
    {
        var redisMock = new Mock<IConnectionMultiplexer>();
        _dbMock = new Mock<IDatabase>();

        redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                 .Returns(_dbMock.Object);

        _redisUrlCache = new RedisUrlCache(redisMock.Object);
    }

    [Fact]
    public async Task GetLongUrlAsync_ReturnsValue_WhenKeyExists()
    {
        // Arrange
        var shortUrlCode = "abc123";
        var longUrl = "https://example.com";

        _dbMock.Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
               .ReturnsAsync(longUrl);

        // Act
        var result = await _redisUrlCache.GetLongUrlAsync(shortUrlCode, CancellationToken.None);

        // Assert
        Assert.Equal(longUrl, result);
    }

    [Fact]
    public async Task GetLongUrlAsync_ReturnsNull_WhenKeyDoesNotExist()
    {
        // Arrange
        var shortUrlCode = "missing";

        _dbMock.Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
               .ReturnsAsync(RedisValue.Null);

        // Act
        var result = await _redisUrlCache.GetLongUrlAsync(shortUrlCode, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SetLongUrlAsync_DoesNotThrow()
    {
        // Arrange
        var shortUrlCode = "abc123";
        var longUrl = "https://example.com";
        var ttl = TimeSpan.FromHours(1);

        _dbMock.Setup(db => db.StringSetAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<RedisValue>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act & Assert
        await _redisUrlCache.SetLongUrlAsync(shortUrlCode, longUrl, CancellationToken.None, ttl);
    }

    [Fact]
    public async Task RemoveAsync_CallsKeyDeleteAsync()
    {
        // Arrange
        var shortUrlCode = "abc123";
        _dbMock.Setup(db => db.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
               .ReturnsAsync(true);

        // Act
        await _redisUrlCache.RemoveAsync(shortUrlCode, CancellationToken.None);

        // Assert
        _dbMock.Verify(db => db.KeyDeleteAsync(It.Is<RedisKey>(k => k.ToString() == $"url:{shortUrlCode}"),
                                                It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task Methods_Throw_WhenCancellationRequested()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _redisUrlCache.GetLongUrlAsync("abc", cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _redisUrlCache.SetLongUrlAsync("abc", "url", cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _redisUrlCache.RemoveAsync("abc", cts.Token));
    }
}
