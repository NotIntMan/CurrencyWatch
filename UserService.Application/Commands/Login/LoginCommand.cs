using UserService.Application.DTOs;

namespace UserService.Application.Commands.Login;

public record LoginCommand(string Name, string Password) : IRequest<LoginResultDto>;
