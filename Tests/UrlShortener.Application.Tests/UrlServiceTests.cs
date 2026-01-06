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

    // CreateAsync TESTOVIIIIII
    
    [Fact] //oznacava da se radi o jednom testu, bit ce pokrenut automatski
    public async Task CreateAsync_WhenShortCodeDoesNotExist_CreatesUrl()
    {
        // ******************************ARRANGE**********************************************************
        // pripema podatak
        // pripremimo sve sto nam treba za services metodu Create async a to je req, user, canc token
        // odradimo vanjski metode iz repoa getUrlByShortId i CreateAsync
        var request = new CreateUrlRequestDTO
        {
            Description = "test description",
            LongUrl = "https://www.google.com",
            ShortUrlCode = "ggl"
        };
        
        var user = User.Create("TestUser", "TestPassword");

        // situacija da status code ne postoji... tj. da se moze kreirat url
        _urlRepositoryMock
            .Setup(r => r.GetUrlByShortCode(request.ShortUrlCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Url?)null);

        //kreiranje urla
        _urlRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Url>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        //kreirati mapper
        _mapperMock
            .Setup(m => m.Map<CreateUrlResponseDTO>(It.IsAny<Url>()))
            .Returns(new CreateUrlResponseDTO
            {
                ShortUrlCode = request.ShortUrlCode,
                LongUrl = request.LongUrl
            });

        
        //*********************************ACT************************************************************
        var result = await _sut.CreateAsync(request, user, CancellationToken.None);
        //sve metode vanjske su mockane, zovemo metodu cekamo rezultat i spemni smo z aprovjeru

        //***************************************ASSERT****************************************************
        //Sto je metoda vratila
        //koje vanjske metode su pozvane
        //jesu li povezane tocno koliko treba
        
        //metoda je nesto morala vratiti
        Assert.NotNull(result);
        //provjerava da metoda nije promjenila nesto sto nije smijela
        Assert.Equal(request.ShortUrlCode, result.ShortUrlCode);
        Assert.Equal(request.LongUrl, result.LongUrl);
        
        //provjerava da se izvrislo
        _urlRepositoryMock.Verify(r => r.GetUrlByShortCode(request.ShortUrlCode, It.IsAny<CancellationToken>()), Times.Once);
        _urlRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Url>(), It.IsAny<CancellationToken>()), Times.Once);
        
        _mapperMock.Verify(m => m.Map<CreateUrlResponseDTO>(It.IsAny<Url>()), Times.Once);
        
        //da nije doslo do mappera stavili bi Times.Never
        //i ovde je sve proslo pozitivno ali ako nesto ima exception onda umjesto returnAsync bi radili ThrowsASYNC i vracali exceptione
        //iiiii jos bi hvatali exceptione u ACTu
    }

    [Fact]
    public async Task CreateAsync_WhenShortCodeAlreadyExists_ThrowsException()
    {
        //ARRANGE
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

        //ACT 
        //ovo hvata nas exception
        var exception = await Assert.ThrowsAsync<ShortUrlCodeAlreadyExistsException>(
            () => _sut.CreateAsync(request, user, CancellationToken.None)
        );

        //ASSERT
        Assert.Equal(request.ShortUrlCode, exception.ShortUrlCode);

        _urlRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Url>(), It.IsAny<CancellationToken>()), Times.Never);
        _mapperMock.Verify(m => m.Map<CreateUrlResponseDTO>(It.IsAny<Url>()), Times.Never);
    }
    
    [Fact]
    public async Task CreateAsync_WhenRepositoryThrowsException_PropagatesException()
    {
        //ARRAGE
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

        // CreateAsync repozitorija baca exception
        _urlRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Url>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB failure"));

        //ACT &
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _sut.CreateAsync(request, user, CancellationToken.None)
        );
        
        //ASSERT
        Assert.Equal("DB failure", exception.Message);
        //Times never jer se ne pzovia mapper jer nije doslo do njega jos
        _mapperMock.Verify(m => m.Map<CreateUrlResponseDTO>(It.IsAny<Url>()), Times.Never);
    }

    
    //GetAllUrlsByUser TESTOVI
    
    [Fact]
    public async Task GetAllUrlsByUser_ReturnsMappedUrls_WhenRepoReturnsUrls()
    {
        //ARRANGE
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

        //ACT
        var result = await _sut.GetAllUrlsByUser(userId, CancellationToken.None);

        //ASSERT
        Assert.NotNull(result);
        Assert.Equal(urlList.Count, result.Count);
        for (int i = 0; i < urlList.Count; i++)
        {
            Assert.Equal(urlList[i].ShortUrlCode, result[i].ShortUrlCode);
            Assert.Equal(urlList[i].LongUrl, result[i].LongUrl);
            Assert.Equal(urlList[i].Description, result[i].Description);
        }
    
        _urlRepositoryMock.Verify(r => r.GetAllUrlsByUser(userId, It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<List<CreateUrlResponseDTO>>(It.IsAny<List<Url>>()), Times.Once);
    }

    [Fact]
    public async Task GetAllUrlsByUser_ThrowsException_WhenRepoThrows()
    {
        //ARRANGE
        var userId = Guid.NewGuid();
        _urlRepositoryMock
            .Setup(r => r.GetAllUrlsByUser(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        //ACT
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _sut.GetAllUrlsByUser(userId, CancellationToken.None));

        //ASSERT
        Assert.Equal("Database error", exception.Message);
        _urlRepositoryMock.Verify(r => r.GetAllUrlsByUser(userId, It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<List<CreateUrlResponseDTO>>(It.IsAny<List<Url>>()), Times.Never);
    }

    //GetUrlById testovi
    
    [Fact]
    public async Task GetUrlById_WhenUrlExistsAndUserIsOwner_ReturnsDto()
    {
        //ARRANGE
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

        //ACT
        var result = await _sut.GetUrlById(url.Id, user.Id, CancellationToken.None);

        //ASSERT
        Assert.NotNull(result);
        Assert.Equal(url.ShortUrlCode, result.ShortUrlCode);
        _urlRepositoryMock.Verify(r => r.GetUrlById(url.Id, It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<CreateUrlResponseDTO>(url), Times.Once);
    }

    [Fact]
    public async Task GetUrlById_WhenUrlDoesNotExist_ThrowsUrlNotFoundException()
    {
        //ARRANGE
        var id = Guid.NewGuid();
        _urlRepositoryMock
            .Setup(r => r.GetUrlById(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Url?)null);

        //ACT & ASSERT
        await Assert.ThrowsAsync<UrlNotFoundException>(() => _sut.GetUrlById(id, Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task GetUrlById_WhenUserIsNotOwner_ThrowsUnauthorizedAccessException()
    {
        //ARRANGE
        var owner = User.Create("Owner", "pass");
        var url = Url.Create("code", "https://example.com", "desc", owner);

        _urlRepositoryMock.Setup(r => r.GetUrlById(url.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(url);

        //ACT & ASSERT
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.GetUrlById(url.Id, Guid.NewGuid(), CancellationToken.None));
    }

    //DeleteAsync TESTOVI
    
    [Fact]
    public async Task DeleteAsync_WhenUrlExistsAndUserIsOwner_DeletesUrlAndCache()
    {
    //ARRANGE
        var user = User.Create("TestUser", "pass");
        var url = Url.Create("code", "https://example.com", "desc", user);

        _urlRepositoryMock.Setup(r => r.GetUrlById(url.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(url);

        _urlRepositoryMock.Setup(r => r.DeleteAsync(url, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _urlCacheMock.Setup(c => c.RemoveAsync(url.ShortUrlCode, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        //ACT
        await _sut.DeleteAsync(url.Id, user.Id, CancellationToken.None);

        //ASSERT
        _urlRepositoryMock.Verify(r => r.DeleteAsync(url, It.IsAny<CancellationToken>()), Times.Once);
        _urlCacheMock.Verify(c => c.RemoveAsync(url.ShortUrlCode, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenUrlDoesNotExist_ThrowsUrlNotFoundException()
    {
        //ARRANGE
        var id = Guid.NewGuid();
        _urlRepositoryMock.Setup(r => r.GetUrlById(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Url?)null);

        //ACT & ASSERT
        await Assert.ThrowsAsync<UrlNotFoundException>(() => _sut.DeleteAsync(id, Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task DeleteAsync_WhenUserIsNotOwner_ThrowsUnauthorizedAccessException()
    {
        //ARRANGE
        var owner = User.Create("Owner", "pass");
        var url = Url.Create("code", "https://example.com", "desc", owner);

        _urlRepositoryMock.Setup(r => r.GetUrlById(url.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(url);

        //ACT & ASSERT
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.DeleteAsync(url.Id, Guid.NewGuid(), CancellationToken.None));
    }

    //GetLongUrlByCode TESTOVI
    
    [Fact]
    public async Task GetLongUrlByCode_WhenCacheHit_ReturnsCachedValue()
    {
        //ARRANGE
        _urlCacheMock.Setup(c => c.GetLongUrlAsync("code", It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://example.com");

        //ACT
        var result = await _sut.GetLongUrlByCode("code", CancellationToken.None);

        //ASSERT
        Assert.Equal("https://example.com", result);
        _urlCacheMock.Verify(c => c.GetLongUrlAsync("code", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetLongUrlByCode_WhenNotInCacheAndUrlExists_ReturnsUrlAndSetsCache()
    {
        //ARRANGE
        var user = User.Create("TestUser", "pass");
        var url = Url.Create("code", "https://example.com", "desc", user);

        _urlCacheMock.Setup(c => c.GetLongUrlAsync(url.ShortUrlCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        _urlRepositoryMock.Setup(r => r.GetUrlByShortCode(url.ShortUrlCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(url);

        _urlCacheMock.Setup(c => c.SetLongUrlAsync(url.ShortUrlCode, url.LongUrl, It.IsAny<CancellationToken>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);

        //ACT
        var result = await _sut.GetLongUrlByCode(url.ShortUrlCode, CancellationToken.None);

        //ASSERT
        Assert.Equal(url.LongUrl, result);
        _urlCacheMock.Verify(c => c.SetLongUrlAsync(url.ShortUrlCode, url.LongUrl, It.IsAny<CancellationToken>(), It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    public async Task GetLongUrlByCode_WhenUrlDoesNotExist_ReturnsNull()
    {
       //ARRANGE
       _urlCacheMock.Setup(c => c.GetLongUrlAsync("code", It.IsAny<CancellationToken>()))
           .ReturnsAsync((string?)null);

        _urlRepositoryMock.Setup(r => r.GetUrlByShortCode("code", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Url?)null);
    
        //ACT
        var result = await _sut.GetLongUrlByCode("code", CancellationToken.None);

        //ASSERT
        Assert.Null(result);
    }

    
}
