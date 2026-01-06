using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using UrlShortener.Application.Users.Exceptions;
using UrlShortener.Application.DTO_s;
using UrlShortener.Application.Interfaces;
using UrlShortener.Application.Services;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Tests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ILogger<UserService>> _loggerMock;
    private readonly Mock<IMapper> _mapperMock;

    private readonly UserService _sut;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _loggerMock = new Mock<ILogger<UserService>>();
        _mapperMock = new Mock<IMapper>();

        _sut = new UserService(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _loggerMock.Object,
            _mapperMock.Object
        );
    }

    //  RegisterUser TESTOVI
    
    [Fact]
    public async Task RegisterUser_WhenUsernameDoesNotExist_CreatesUser()
    {
        //ARRANGE
        var request = new RegisterUserRequestDTO { Username = "test", Password = "pass" };
        _userRepositoryMock.Setup(r => r.GetUserByUsername(request.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHasherMock.Setup(h => h.Hash(request.Password)).Returns("hashedPass");

        _userRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mapperMock.Setup(m => m.Map<RegisterUserResponseDTO>(It.IsAny<User>()))
            .Returns((User u) => new RegisterUserResponseDTO { Id = u.Id, Username = u.Username });

        //ACT
        var result = await _sut.RegisterUser(request, CancellationToken.None);

        //ASSERT
        Assert.NotNull(result);
        Assert.Equal(request.Username, result.Username);
        _userRepositoryMock.Verify(r => r.GetUserByUsername(request.Username, It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<RegisterUserResponseDTO>(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task RegisterUser_WhenUsernameExists_ThrowsUsernameAlreadyExistsException()
    {
        //ARRANGE
        var request = new RegisterUserRequestDTO { Username = "test", Password = "pass" };
        var existingUser = User.Create("test", "hashedPass");

        _userRepositoryMock.Setup(r => r.GetUserByUsername(request.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        //ACT
        var exception = await Assert.ThrowsAsync<UsernameAlreadyExistsException>(() =>
            _sut.RegisterUser(request, CancellationToken.None));

        //ASSERT
        Assert.Equal(request.Username, exception.Username);
        _userRepositoryMock.Verify(r => r.GetUserByUsername(request.Username, It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    //Login TESTOVI
    
    [Fact]
    public async Task LoginUser_WhenCredentialsAreValid_ReturnsUser()
    {
        //ARRANGE
        var request = new LoginUserRequestDTO { Username = "test", Password = "pass" };
        var user = User.Create(request.Username, "hashedPass");

        _userRepositoryMock.Setup(r => r.GetUserByUsername(request.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(h => h.Verify(request.Password, user.PasswordHash)).Returns(true);
        _mapperMock.Setup(m => m.Map<LoginUserResponseDTO>(It.IsAny<User>()))
            .Returns((User u) => new LoginUserResponseDTO { Id = u.Id, Username = u.Username });

        //ACT
        var result = await _sut.LoginUser(request, CancellationToken.None);

        //ASSERT
        Assert.NotNull(result);
        Assert.Equal(request.Username, result.Username);
    }

    [Fact]
    public async Task LoginUser_WhenUserDoesNotExist_ThrowsInvalidCredentialsException()
    {
        //ARRANGE
        var request = new LoginUserRequestDTO { Username = "test", Password = "pass" };
        _userRepositoryMock.Setup(r => r.GetUserByUsername(request.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        //ACT
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            _sut.LoginUser(request, CancellationToken.None));

        //ASSERT
        _userRepositoryMock.Verify(r => r.GetUserByUsername(request.Username, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginUser_WhenPasswordIsInvalid_ThrowsInvalidCredentialsException()
    {
        //ARRANGE
        var request = new LoginUserRequestDTO { Username = "test", Password = "pass" };
        var user = User.Create(request.Username, "hashedPass");

        _userRepositoryMock.Setup(r => r.GetUserByUsername(request.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(h => h.Verify(request.Password, user.PasswordHash)).Returns(false);

        //ACT
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            _sut.LoginUser(request, CancellationToken.None));
    }

    //GetUserById TESTOVI
    
    [Fact]
    public async Task GetUserById_WhenUserExists_ReturnsUser()
    {
        //ARRANGE
        var userId = Guid.NewGuid();
        var user = User.Create("test", "hashedPass");

        _userRepositoryMock.Setup(r => r.GetUserById(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mapperMock.Setup(m => m.Map<User>(It.IsAny<User>())).Returns((User u) => u);

        //ACT
        var result = await _sut.GetUserById(userId, CancellationToken.None);

        //ASSERT
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
    }
    
}
