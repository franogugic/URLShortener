using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Application.URLs.DTO_s;
using UrlShortener.Application.URLs.Interfaces;
using UrlShortener.Domain.Entities;
using UrlShortener.Infrastructure.Db;

namespace UrlShortener.IntegrationTests;

//IClassFixure omogucuje da se nasa aplikacije pokrene samo jednom sto stedi vrijeme i resurse
public class UrlControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public UrlControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        //Iz factorya izvlacimo klijenta koji glumi postman ili browser
        //on udara direktno u nas api u memoriji
        //simulacija POST requesta
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions()
        {
	        AllowAutoRedirect = false // KLJUČNO: Ne želimo da klijent sam ode na Google, želimo uhvatiti 302
        });

        _factory = factory;
    }
    
    //CRAETE TESTOVI

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreated()
    {
        // 1. ARRANGE

        // Kreiramo korisnika, radi provjere middlewarea
        var testUser = User.Create("TestUser", "SigurnaLozinka123!");
        var testUserId = testUser.Id;

        // Ubacujemo korisnika direktno u bazu da ga servis može pronaći
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Osiguravamo da je baza čista i dodajemo usera
            db.Users.Add(testUser);
            await db.SaveChangesAsync();
        }

        // Pripremamo DTO koji kontroler prima
        var request = new CreateUrlRequestDTO()
        {
            LongUrl = "https://www.google.com",
            ShortUrlCode = "google-test",
            Description = "Testni opis"
        };

        // KLJUČNI DIO: Šaljemo ID korisnika kroz header. 
        // Naš TestAuthHandler u Factoryju će pročitati ovaj ID i ulogirati nas kao tog usera.
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("TestScheme", testUserId.ToString());

        // 2. ACT
        var response = await _client.PostAsJsonAsync("url/create", request);

        // 3. ASSERT
        // Provjeravamo status kod 201 Created
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        // Opcionalno: Provjeri sadržaj odgovora
        var responseData = await response.Content.ReadFromJsonAsync<CreateUrlResponseDTO>();
        Assert.NotNull(responseData);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_InvalidRequest_ReturnsBadRequest()
    {
        //ARRANGE
        //Ostaje isti zato sto su su i isti ulazni podaci jer je ista metoda, samo ovde cemo npr izbrisat longUrl
        //jer simuliramo 400 error da nisu req dobro poslani
        var user = User.Create("TestUser", "SigurnaLozinka123!");
        var userId = user.Id;
        
        var request = new CreateUrlRequestDTO()
        {
            LongUrl = "",
            ShortUrlCode = "google-test",
            Description = "Ovo je testni opis"
        };
        
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("TestScheme", userId.ToString());

        //ACT
        //takodjer ostaje isti zato sto testiramo istu metodu controllera
        var response = await _client.PostAsJsonAsync("/url/create", request);

        //ASSERT
        //Provjeravamo da je vratio bar nesto response
        Assert.NotNull(response);
        //usporedjujemo vrijednosti tj da nas response.Status kod mora vratit badReq
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_NoAuth_ReturnsUnauthorized()
    {
        //ARRANGE
        //dodajemo req jer to user prima, necemo kreirat usera jer ne treba proc auth
        var request = new CreateUrlRequestDTO()
        {
            Description = "TEST",
            LongUrl = "https://www.google.com",
            ShortUrlCode = "google-test",
        };

        //ACT
        var response = await _client.PostAsJsonAsync("url/create", request);

        //ASSERT
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    //GetAllUrlsByUser TESTOVI

    [Fact]
    public async Task GetAllUrlsByUser_Valid_ReturnsOk()
    {
        //ARRANGE
        //prvo kreiramo usera jer mora proc auth
        var testUser = User.Create("TestUser", "SigurnaLozinka123!");
        var testUserId = testUser.Id;

        //kreiramo listu usera koju cemo dodat u nasu laznu bazu
        var url1 = Url.Create("shortCode1", "https://www.google.com", "description1", testUser);
        var url2 = Url.Create("shortCode2", "https://www.google.com", "description2", testUser);
        var url3 = Url.Create("shortCode3", "https://www.google.com", "description3", testUser);

        //znaci ovde kreiramos scope koji ce bit vazec dok se zadnja linija ovog ne izvrsi
        using (var scope = _factory.Services.CreateScope())
        {
            //kreiramo lazni AppDbContext
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            //dodati usera i url
            db.Users.Add(testUser);
            db.Urls.AddRange(url1, url2, url3);
            await db.SaveChangesAsync();
        }
        
        //dodamo u header auth
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("TestScheme", testUserId.ToString());
        
        //ACT
        var response = await _client.GetAsync($"url/getAllUrlsByUserId");
        
        //ASSERT
        var urls = await response.Content.ReadFromJsonAsync<List<CreateUrlResponseDTO>>();
        
        Assert.NotNull(urls);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, urls?.Count);
        Assert.Contains(urls, u => u.ShortUrlCode == "shortCode1"); 
    }

    [Fact]
    public async Task GetAllUrlsByUser_NoAuth_ReturnsUnauthorized()
    {
        //ARRANGE
        // ostavljamo prazno jer se nece proc uopce auth
        
        //ACT
        var response = await _client.GetAsync($"url/getAllUrlsByUserId");
        
        //ASSERT
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAllUrlsByUser_ValidEmptyList_ReturnsOk()
    {
        //kreirali smo usera
        var testUser = User.Create("TestUser", "SigurnaLozinka123!");
        var testUserId = testUser.Id;

        //popunimo bazu
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            db.Users.Add(testUser);
            db.SaveChanges();
        }
        
        //DODAMO U HEADER Auth
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("TestScheme", testUserId.ToString());
        
        //ACT
        var response = await _client.GetAsync($"url/getAllUrlsByUserId");
        var urls = await response.Content.ReadFromJsonAsync<List<CreateUrlResponseDTO>>();
        
        //ASSERT
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        //provjeravamo da je vracelo 0 urlova tj prazna lista, ali da je svejedno vracen statusni kod 200, linija iznad
        Assert.Equal(0, urls?.Count);
    }
    
    
    //GetUrlById TESTOVI
    [Fact]
    public async Task GetUrlById_ValidRequest_ReturnsUrl()
    {
        //ARRANGE
        var testUser = User.Create("TestUser", "SigurnaLozinka123!"); 
        var testUserId = testUser.Id;
        
        var url1 = Url.Create("shortCode1", "https://www.google.com", "description1", testUser);
       
        using (var scope = _factory.Services.CreateScope()) {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Users.Add(testUser);
            db.Urls.Add(url1);
            db.SaveChanges();
        }
        
        var urlId = url1.Id;
        
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("TestScheme", testUserId.ToString());
       
        //ACT
        var response = await _client.GetAsync($"url/getUrlById/{urlId}");
        var url = await response.Content.ReadFromJsonAsync<CreateUrlResponseDTO>();
        
        //ASSERT
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(url);
        Assert.Equal("shortCode1", url.ShortUrlCode); 
    }
    
    [Fact]
    public async Task GetUrlById_NoAuthHeader_ReturnsUnauthorized()
    {
        // ARRANGE
        //stavljamo bilo koji urlId jer je nebitno svakako nece proci zato sto nemamo usera id
        var urlId = Guid.NewGuid(); 

        // ACT
        var response = await _client.GetAsync($"url/getUrlById/{urlId}");

        // ASSERT
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task GetUrlById_TokenWithoutValidClaim_ReturnsUnauthorized()
    {
        // ARRANGE
        var urlId = Guid.NewGuid();
    
        // Postavljamo “token” ali s krivim/bez claimova
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("TestScheme", "InvalidOrMissingClaim");

        // ACT
        var response = await _client.GetAsync($"url/getUrlById/{urlId}");

        // ASSERT
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUrlById_AuthenticatedUser_AttemptingToAccessOtherUsersUrl_ReturnsForbidden()
    {
        //ARRANGE
        
        //kreiramo usera 2, jedan ce bit vlasnik urlova a s drugim cemo se probat priajvit
        //i on da dohvatit tudje urlove
        var testUser = User.Create("TestUser", "SigurnaLozinka123!");
        var testUser2 = User.Create("TestUser2", "SigurnaLozinka123!");
        var testUserId2 = testUser2.Id;
        
        //kreiramo url pripadaju useru 2
        var url1 = Url.Create("shortCode1", "https://www.google.com", "description1", testUser);
        var url1Id = url1.Id;
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Users.AddRange(testUser, testUser2);
            db.Urls.Add(url1);
            db.SaveChanges();
        }
        
        //user 2 stavljen u header
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("TestScheme", testUserId2.ToString());
        
        //ACT
        var response = await _client.GetAsync($"url/getUrlById/{url1Id}");
        
        //ASSERT
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotNull(response);
    }

    [Fact]
    public async Task GetUrlById_UrlDoesNotExist_ReturnsNotFound()
    {
        //ARRANGE
        var testUser = User.Create("TestUser", "SigurnaLozinka123!");
        var testUserId = testUser.Id;
        
        var url1 = Url.Create("shortCode1", "https://www.google.com", "description1", testUser);
        var urlId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Users.Add(testUser);
            db.Urls.Add(url1);
            db.SaveChanges();
        }
        
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("TestScheme", testUserId.ToString());
        
        //ACT
        var response = await _client.GetAsync($"url/getUrlById/{urlId}");
        
        //ASSERT
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUrlById_InvalidGuid_ReturnsBadRequest()
    {
        // ARRANGE
        var testUser = User.Create("TestUser", "Password123!");
    
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Users.Add(testUser);
        db.SaveChanges();

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("TestScheme", testUser.Id.ToString());

        // ACT
        var response = await _client.GetAsync("url/getUrlById/invalid-guid");

        // ASSERT
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    //Delete TESTOVI
    [Fact]
    public async Task DeleteUrl_AuthenticatedUser_DeletesOwnUrl_ReturnsNoContent()
    {
	    //ARRANGE
	    var user = User.Create("TestUser", "Password123!");
	    var url = Url.Create("shortCode1", "https://www.google.com", "description1", user);

	    using var scope = _factory.Services.CreateScope();
	    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
	    var service = scope.ServiceProvider.GetRequiredService<IUrlService>();

	    db.Users.Add(user);
	    db.Urls.Add(url);
	    db.SaveChanges();

	    _client.DefaultRequestHeaders.Authorization =
		    new System.Net.Http.Headers.AuthenticationHeaderValue("TestScheme", user.Id.ToString());

	    //ACT
	    var response = await _client.DeleteAsync($"url/delete/{url.Id}");

	    //ASSERT
	    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
	
	[Fact]
	public async Task DeleteUrl_NoAuthHeader_ReturnsUnauthorized()
	{
		//ARRANGE
		var urlId = Guid.NewGuid();

		//ACT
		var response = await _client.DeleteAsync($"url/delete/{urlId}");

		//ASSERT
		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
	}

	[Fact]
	public async Task DeleteUrl_InvalidClaim_ReturnsUnauthorized()
	{
		//ARRANGE
		var user = User.Create("TestUser", "Password123!");
		_client.DefaultRequestHeaders.Authorization =
			new System.Net.Http.Headers.AuthenticationHeaderValue("TestScheme", "InvalidClaim");
		var urlId = Guid.NewGuid();

		//ACT
		var response = await _client.DeleteAsync($"url/delete/{urlId}");

		//ASSERT
		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
	}

	[Fact]
	public async Task DeleteUrl_AuthenticatedUser_AttemptingToDeleteOtherUsersUrl_ReturnsForbidden()
	{
		//ARRANGE
		var owner = User.Create("OwnerUser", "Password123!");
		var attacker = User.Create("AttackerUser", "Password123!");
		var url = Url.Create("shortCode1", "https://www.google.com", "description1", owner);

		using var scope = _factory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
		db.Users.AddRange(owner, attacker);
		db.Urls.Add(url);
		db.SaveChanges();

		_client.DefaultRequestHeaders.Authorization =
			new System.Net.Http.Headers.AuthenticationHeaderValue("TestScheme", attacker.Id.ToString());

		//ACT
		var response = await _client.DeleteAsync($"url/delete/{url.Id}");

		//ASSERT
		Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
	}

	[Fact]
	public async Task DeleteUrl_AuthenticatedUser_UrlDoesNotExist_ReturnsNotFound()
	{
		//ARRANGE
		var user = User.Create("TestUser", "Password123!");
		var nonExistingUrlId = Guid.NewGuid();

		using var scope = _factory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
		db.Users.Add(user);
		db.SaveChanges();

		_client.DefaultRequestHeaders.Authorization =
			new System.Net.Http.Headers.AuthenticationHeaderValue("TestScheme", user.Id.ToString());

		//ACT
		var response = await _client.DeleteAsync($"url/delete/{nonExistingUrlId}");

		//ASSERT
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task DeleteUrl_InvalidGuid_ReturnsBadRequest()
	{
		//ARRANGE
		var user = User.Create("TestUser", "Password123!");
		using var scope = _factory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
		db.Users.Add(user);
		db.SaveChanges();

		_client.DefaultRequestHeaders.Authorization =
			new System.Net.Http.Headers.AuthenticationHeaderValue("TestScheme", user.Id.ToString());

		//ACT
		var response = await _client.DeleteAsync("url/delete/invalid-guid");

		//ASSERT
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}
	
	//ShortCode GetUrl TESTOVI
	
	[Fact]
	public async Task RedirectToLongUrl_ExistingCode_ReturnsRedirect()
	{
		// ARRANGE
		using var scope = _factory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
		var cache = scope.ServiceProvider.GetRequiredService<IUrlCache>();

		var user = User.Create("TestUser", "Password123!");
		var url = Url.Create("short123", "https://www.google.com", "description", user);

		db.Users.Add(user);
		db.Urls.Add(url);
		await db.SaveChangesAsync();

		// Punimo i cache da budemo sigurni
		await cache.SetLongUrlAsync(url.ShortUrlCode, url.LongUrl, CancellationToken.None, null);

		// ACT
		var response = await _client.GetAsync($"url/{url.ShortUrlCode}");

		// ASSERT
		Assert.Equal(HttpStatusCode.Found, response.StatusCode);
    
		// Čistimo oba URL-a od kosih crta na kraju kako bi test bio otporan na te sitnice
		var expectedUrl = url.LongUrl.TrimEnd('/');
		var actualUrl = response.Headers.Location?.ToString().TrimEnd('/');
    
		Assert.Equal(expectedUrl, actualUrl);
	}
	[Fact]
	public async Task RedirectToLongUrl_NonExistingCode_ReturnsNotFound()
	{
		//ARRANGE
		var nonExistingCode = "doesnotexist";

		//ACT
		var response = await _client.GetAsync($"url/{nonExistingCode}");

		//ASSERT
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task RedirectToLongUrl_EmptyCode_ReturnsNotFound()
	{
		//ARRANGE
		var emptyCode = "";

		//ACT
		var response = await _client.GetAsync($"url/{emptyCode}");

		//ASSERT
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}
}
