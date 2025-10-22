using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Manage.Queries.GetUser
{
    public class GetUserHandler : IRequestHandler<GetUserQuery, GetUserResponse>
    {
        readonly IUserRepository _userRepository;
        public GetUserHandler(IUserRepository userRepositry)
        {
            _userRepository = userRepositry;
        }

        public async Task<GetUserResponse> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            if (request.Id == Guid.Empty)
            {
                return new GetUserResponse { Success = false, Message = "UserId is required.", StatusCode = 400 };
            }

            var user = await _userRepository.GetByIdAsync(request.Id);

            if (user == null)
                return new GetUserResponse { Success = false, Message = "User not found.", StatusCode = 404 };

            var response = new GetUserResponse
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
                Balance = user.Balance,
                Gender = user.Gender,
                Type = user.Type,
                RegisterDate = user.RegisterDate,
                UserName = user.UserName
            };

            return response;
        }
    }
}
