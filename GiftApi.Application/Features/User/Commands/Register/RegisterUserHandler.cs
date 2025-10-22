using GiftApi.Application.Interfaces;
using GiftApi.Domain.Enums.User;
using MediatR;

namespace GiftApi.Application.Features.User.Commands.Register
{
    public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly RegisterUserValidator _validator;

        public RegisterUserHandler(IUserRepository userRepository, RegisterUserValidator validator)
        {
            _userRepository = userRepository;
            _validator = validator;
        }

        public async Task<RegisterUserResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
                return new RegisterUserResponse { Success = false, Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)), StatusCode = 400 };

            string userName = request.LoginType switch
            {
                LoginIdentifierType.Email => request.Email,
                LoginIdentifierType.PhoneNumber => request.PhoneNumber,
                _ => request.PhoneNumber
            };

            bool alreadyExists = await _userRepository.UserNameExists(userName);

            if (alreadyExists)
                return new RegisterUserResponse { Success = false, Message = $"{request.LoginType} already exists.", StatusCode = 409 };


            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var newUser = new GiftApi.Domain.Entities.User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                IdentificationNumber = request.IdentificationNumber,
                DateOfBirth = request.DateOfBirth,
                PhoneNumber = request.PhoneNumber,
                Password = hashedPassword,
                UserName = userName,
                Gender = request.Gender,
                RegisterDate = DateTime.UtcNow.AddHours(4),
                Balance = 0,
                Type = UserType.User
            };

            var createdUser = await _userRepository.Register(newUser);

            return new RegisterUserResponse
            {
                Success = true,
                Message = "User registered successfully",
                StatusCode = 200,
                UserName = createdUser.UserName,
                PhoneNumber = createdUser.PhoneNumber,
                Email = createdUser.Email,
                FirstName = createdUser.FirstName,
                LastName = createdUser.LastName
            };
        }
    }
}
