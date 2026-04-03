namespace UserService.Application.Commands.Logout;

public record LogoutCommand(string Jti, DateTime AccessTokenExpiresAt, string RefreshToken) : IRequest;
