using FluentValidation;
using GiftApi.Common.Enums.User;
using GiftApi.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Application.User.Commands.Register
{
    public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
    {
        readonly ApplicationDbContext _db;
        readonly IValidator<RegisterUserCommand> _validator;

        public RegisterUserHandler(ApplicationDbContext db, IValidator<RegisterUserCommand> validator)
        {
            _db = db;
            _validator = validator;
        }

        public async Task<RegisterUserResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return new RegisterUserResponse
                {
                    Success = false,
                    UserMessage = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                    StatusCode = 400
                };
            }

            string userName = request.LoginType switch
            {
                LoginIdentifierType.Email => request.Email,
                LoginIdentifierType.PhoneNumber => request.PhoneNumber,
                _ => request.PhoneNumber
            };

            bool exists = await _db.Users
                .AnyAsync(u => u.UserName == userName, cancellationToken);

            if (exists)
            {
                return new RegisterUserResponse
                {
                    Success = false,
                    UserMessage = $"{request.LoginType} already exists.",
                    StatusCode = 409
                };
            }

            var user = new GiftApi.Core.Entities.User
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                IdentificationNumber = request.IdentificationNumber,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                UserName = userName,
                RegisterDate = DateTime.UtcNow.AddHours(4),
                Balance = 0,
                Type = UserType.User,
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync(cancellationToken);

            return new RegisterUserResponse
            {
                Success = true,
                UserMessage = "User registered successfully",
                StatusCode = 200,
                UserName = user.UserName
            };
        }
    }
}
