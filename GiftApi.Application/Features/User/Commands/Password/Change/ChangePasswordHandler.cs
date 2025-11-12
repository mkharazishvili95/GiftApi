using MediatR;
using FluentValidation;
using GiftApi.Application.Interfaces;

namespace GiftApi.Application.Features.User.Commands.Password.Change
{
    public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, ChangePasswordResponse>
    {
        readonly IUserRepository _userRepository;
        readonly ICurrentUserRepository _currentUserRepository;
        readonly IValidator<ChangePasswordCommand> _validator;

        public ChangePasswordHandler(
            IUserRepository userRepository,
            ICurrentUserRepository currentUserRepository,
            IValidator<ChangePasswordCommand> validator)
        {
            _userRepository = userRepository;
            _currentUserRepository = currentUserRepository;
            _validator = validator;
        }

        public async Task<ChangePasswordResponse> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
                return new ChangePasswordResponse  { Success = false, StatusCode = 400, Message = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)) };

            var userId = _currentUserRepository.GetUserId();
            if (userId == null)
                return new ChangePasswordResponse { Success = false, StatusCode = 401, Message = "Unauthorized" };

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null)
                return new ChangePasswordResponse { Success = false, StatusCode = 404, Message = "User not found" };

            if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.Password))
                return new ChangePasswordResponse { Success = false, StatusCode = 400,  Message = "Old password is incorrect" };

            string hashed = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _userRepository.UpdatePasswordAsync(user, hashed);

            return new ChangePasswordResponse { Success = true,  StatusCode = 200, Message = "Password changed successfully"  };
        }
    }
}