using MediatR;
using UserService.Application.DTOs;

namespace UserService.Application.Commands.Refresh;

public record RefreshCommand(string RefreshToken) : IRequest<LoginResultDto>;
