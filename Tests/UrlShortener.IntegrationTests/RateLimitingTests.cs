using System.Net;
using System.Net.Http.Json;

namespace UrlShortener.IntegrationTests;

public class RateLimitingTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RateLimitingTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task Login_ShouldReturn429_WhenRateLimitExceeded()
    {
        // ARRANGE
        var loginRequest = new { Username = "attacker", Password = "password" };
        var url = "api/user/login";

        // ACT
        // Šaljemo 5 zahtjeva. Nije nas briga ako vrate 500 ili 401, 
        // bitno je samo da "potrošimo" dozvoljene pokušaje radi limietera.
        for (int i = 0; i < 30; i++)
        {
            await _client.PostAsJsonAsync(url, loginRequest);
        }

        // Šesti zahtjev - limiter bi ga trebao presresti prije nego
        // dode do baze ili ostatka koda i vratiti 429.
        var response = await _client.PostAsJsonAsync(url, loginRequest);

        // ASSERT
        // Provjeri status kod - očekujemo 429 (TooManyRequests)
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
    }
}