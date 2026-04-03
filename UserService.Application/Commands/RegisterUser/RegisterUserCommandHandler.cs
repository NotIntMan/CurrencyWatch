using Common.Database;
using Common.Domain.Entities;
using UserService.Application.Services;
using Microsoft.EntityFrameworkCore;
using UserService.Application.DTOs;
using UserService.Application.Exceptions;

namespace UserService.Application.Commands.RegisterUser;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, UserDto>
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher _passwordHasher;

    public RegisterUserCommandHandler(AppDbContext db, PasswordHasher passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserDto> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        var exists = await _db.Users.AnyAsync(u => u.Name == request.Name, ct);
        if (exists)
        {
            throw new UserAlreadyExistsException(request.Name);
        }

        var user = new User
        {
            Name = request.Name,
            PasswordHash = _passwordHasher.Hash(request.Password),
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        return new UserDto(user.Id, user.Name);
    }
}
