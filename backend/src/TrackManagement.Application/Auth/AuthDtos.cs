namespace TrackManagement.Application.Auth;

public record RegisterRequest(string FullName, string Email, string Password);

public record LoginRequest(string Email, string Password);

public record UserDto(int Id, string FullName, string Email);

/// <summary>Returned by register and login. Send the token as "Authorization: Bearer {token}".</summary>
public record AuthResponse(string Token, DateTime ExpiresAt, UserDto User);
