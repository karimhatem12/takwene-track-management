using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TrackManagement.Application.Common.Exceptions;
using TrackManagement.Application.Common.Interfaces;
using TrackManagement.Domain.Entities;

namespace TrackManagement.Application.Auth;

public class AuthService : IAuthService
{
    // Same message for "no such user" and "wrong password", so the response
    // does not reveal which emails are registered (user enumeration).
    private const string InvalidCredentialsMessage = "Invalid email or password.";

    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly TimeProvider _timeProvider;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    // A real hash to verify against when the email is unknown, so a failed login takes
    // the same time whether or not the user exists (no timing-based enumeration).
    // Static: computed once per process, not once per request.
    private static string? _dummyHash;

    public AuthService(
        IApplicationDbContext db,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator,
        TimeProvider timeProvider,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _timeProvider = timeProvider;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        await _registerValidator.ValidateAndThrowAsync(request, cancellationToken);

        var email = request.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email, cancellationToken))
            throw new ConflictException("An account with this email already exists.");

        var user = User.Create(
            request.FullName,
            email,
            _passwordHasher.Hash(request.Password),
            _timeProvider.GetUtcNow().UtcDateTime);

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        return CreateResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        await _loginValidator.ValidateAndThrowAsync(request, cancellationToken);

        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is null)
        {
            _dummyHash ??= _passwordHasher.Hash(Guid.NewGuid().ToString());
            _passwordHasher.Verify(_dummyHash, request.Password);
            throw new UnauthorizedException(InvalidCredentialsMessage);
        }

        if (!_passwordHasher.Verify(user.PasswordHash, request.Password))
            throw new UnauthorizedException(InvalidCredentialsMessage);

        return CreateResponse(user);
    }

    private AuthResponse CreateResponse(User user)
    {
        var (token, expiresAt) = _tokenGenerator.Generate(user);
        return new AuthResponse(token, expiresAt, new UserDto(user.Id, user.FullName, user.Email));
    }
}
