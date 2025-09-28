using GiftApi.Infrastructure.Data;
using MediatR;

namespace GiftApi.Application.Manage.Queries.Get
{
    public class GetUserHandler : IRequestHandler<GetUserQuery, GetUserResponse>
    {
        readonly ApplicationDbContext _db;
        public GetUserHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<GetUserResponse> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            if (request.Id == Guid.Empty)
            {
                return new GetUserResponse { Success = false, UserMessage = "UserId is required.", StatusCode = 400 };
            }

            var user = await _db.Users.FindAsync(request.Id);

            if (user == null)
                return new GetUserResponse { Success = false, UserMessage = "User not found.", StatusCode = 404 };

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
