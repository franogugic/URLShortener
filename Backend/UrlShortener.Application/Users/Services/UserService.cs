using AutoMapper;
using Microsoft.Extensions.Logging;
using UrlShortener.Application.DTO_s;
using UrlShortener.Application.Interfaces;
using UrlShortener.Application.Users.Exceptions;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<UserService> _logger;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher, ILogger<UserService> logger,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<RegisterUserResponseDTO> RegisterUser(RegisterUserRequestDTO request, CancellationToken cancellationToken)
    {
        var username = request.Username;
        var password = request.Password;

        _logger.LogInformation("RegisterUser called for {Username}", request.Username);
        
        var passwordHash = _passwordHasher.Hash(password);
        
        var existingUser = await _userRepository.GetUserByUsername(username, cancellationToken);
        if (existingUser  != null)
        {
            _logger.LogWarning("Attempt to register existing username: {Username}", request.Username);
            throw new UsernameAlreadyExistsException(username);
        }

        var user = User.Create(username, passwordHash);
        
        await _userRepository.CreateAsync(user, cancellationToken);
        _logger.LogInformation("User {Username} successfully created with Id {UserId}", user.Username, user.Id);

        
        return _mapper.Map<RegisterUserResponseDTO>(user);
    }

    public async Task<LoginUserResponseDTO> LoginUser(LoginUserRequestDTO request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByUsername(request.Username, cancellationToken);
        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for username {Username}", request.Username);
            throw new InvalidCredentialsException();
        }
        
        _logger.LogInformation("User {Username} logged in successfully", request.Username);

        return _mapper.Map<LoginUserResponseDTO>(user);
    }

    public async Task<User> GetUserById(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserById(userId, cancellationToken);
        return _mapper.Map<User>(user);
    }
}