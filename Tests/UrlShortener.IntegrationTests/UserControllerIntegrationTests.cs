using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Application.DTO_s;
using UrlShortener.Infrastructure.Db;
using Xunit;

namespace UrlShortener.IntegrationTests;

public class UserControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public UserControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    //RegisterUser TESTOVI
    [Fact]
    public async Task RegisterUser_ValidRequest_ReturnsCreated()
    {
        // ARRANGE
        var request = new RegisterUserRequestDTO { Username = "SeniorUser", Password = "StrongPassword123!" };

        // ACT
        var response = await _client.PostAsJsonAsync("api/user/register", request);

        // ASSERT
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<RegisterUserResponseDTO>();
        Assert.Equal(request.Username, result?.Username);
        Assert.NotEqual(Guid.Empty, result?.Id);
    }

    [Fact]
    public async Task RegisterUser_DuplicateUsername_ReturnsConflict()
    {
        // ARRANGE
        var request = new RegisterUserRequestDTO { Username = "Duplicate", Password = "Password1" };
        await _client.PostAsJsonAsync("api/user/register", request);

        // ACT
        var response = await _client.PostAsJsonAsync("api/user/register", request);

        // ASSERT
        // Promijenjeno u Conflict (409) jer tvoj API tako vraća
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
    
    [Theory]
    [InlineData("", "Password123!")]
    [InlineData("User", "")]
    [InlineData(null, null)]
    public async Task RegisterUser_InvalidData_ReturnsBadRequest(string username, string password)
    {
        // ARRANGE
        var request = new RegisterUserRequestDTO { Username = username, Password = password };

        // ACT
        var response = await _client.PostAsJsonAsync("api/user/register", request);

        // ASSERT
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    //Login TESTOVI
    
    [Fact]
    public async Task Login_ValidCredentials_ReturnsOk()
    {
        // ARRANGE
        var reg = new RegisterUserRequestDTO { Username = "LoginTester", Password = "Password123!" };
        await _client.PostAsJsonAsync("api/user/register", reg);
        var loginRequest = new LoginUserRequestDTO { Username = reg.Username, Password = reg.Password };

        // ACT
        var response = await _client.PostAsJsonAsync("api/user/login", loginRequest);

        // ASSERT
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // ARRANGE
        var loginRequest = new LoginUserRequestDTO { Username = "NonExistent", Password = "WrongPassword" };

        // ACT
        var response = await _client.PostAsJsonAsync("api/user/login", loginRequest);

        // ASSERT
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    //GetMe testovi

    [Fact]
    public async Task GetMe_AuthenticatedUser_ReturnsCorrectIdentity()
    {
        // ARRANGE
        var userId = Guid.NewGuid().ToString();
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("TestScheme", userId);

        // ACT
        var response = await _client.GetAsync("api/user/me");

        // ASSERT
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains(userId, content);
    }

    [Fact]
    public async Task GetMe_Unauthenticated_ReturnsUnauthorized()
    {
        // ARRANGE
        _client.DefaultRequestHeaders.Authorization = null;

        // ACT
        var response = await _client.GetAsync("api/user/me");

        // ASSERT
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    //Logout TESTOVI
    
    [Fact]
    public async Task Logout_Authenticated_ReturnsOk()
    {
        // ARRANGE
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("TestScheme", Guid.NewGuid().ToString());

        // ACT
        var response = await _client.PostAsync("api/user/logout", null);

        // ASSERT
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Logout_Unauthenticated_ReturnsUnauthorized()
    {
        // ARRANGE
        _client.DefaultRequestHeaders.Authorization = null;

        // ACT
        var response = await _client.PostAsync("api/user/logout", null);

        // ASSERT
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
}