using System.Diagnostics;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TrackManagement.Application.Common.Exceptions;
using TrackManagement.Application.Common.Interfaces;
using TrackManagement.Domain.Entities;
using TrackManagement.Domain.Enums;

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

        await RecordAuthEventAsync(user, AuditAction.Registered, cancellationToken);
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

        await RecordAuthEventAsync(user, AuditAction.LoggedIn, cancellationToken);
        return CreateResponse(user);
    }

    /// <summary>
    /// Successful sign-ins and registrations go to the audit trail. Failed attempts are not logged
    /// here (they would flood the table); the login rate limiter covers brute-force attempts.
    /// </summary>
    private async Task RecordAuthEventAsync(User user, AuditAction action, CancellationToken cancellationToken)
    {
        _db.AuditLogs.Add(AuditLog.Create(
            user.Id,
            user.Email,
            action,
            nameof(User),
            user.Id.ToString(),
            changes: null,
            _timeProvider.GetUtcNow().UtcDateTime,
            Activity.Current?.TraceId.ToString()));

        await _db.SaveChangesAsync(cancellationToken);
    }

    private AuthResponse CreateResponse(User user)
    {
        var (token, expiresAt) = _tokenGenerator.Generate(user);
        return new AuthResponse(token, expiresAt, new UserDto(user.Id, user.FullName, user.Email));
    }
}
