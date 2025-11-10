using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.User.Queries.GetCurrent
{
    public class GetCurrentUserHandler : IRequestHandler<GetCurrentUserQuery, GetCurrentUserResponse>
    {
        readonly IUserRepository _userRepository;
        readonly ICurrentUserRepository _currentUserRepository;

        public GetCurrentUserHandler(IUserRepository userRepository, ICurrentUserRepository currentUserRepository)
        {
            _userRepository = userRepository;
            _currentUserRepository = currentUserRepository;
        }

        public async Task<GetCurrentUserResponse> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserRepository.GetUserId();
            if (userId == null)
                return new GetCurrentUserResponse { Success = false, StatusCode = 401, Message = "Unauthorized." };

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null)
                return new GetCurrentUserResponse { Success = false, StatusCode = 404, Message = "User not found." };

            return new GetCurrentUserResponse
            {
                Success = true,
                StatusCode = 200,
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DateOfBirth = user.DateOfBirth,
                IdentificationNumber = user.IdentificationNumber,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                UserName = user.UserName,
                Gender = user.Gender,
                Balance = user.Balance,
                Type = user.Type,
                RegisterDate = user.RegisterDate,
                Message = "OK"
            };
        }
    }
}