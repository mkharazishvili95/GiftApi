using GiftApi.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Application.User.Queries.Get
{
    public class GetCurrentUserHandler : IRequestHandler<GetCurrentUserQuery, GetCurrentUserResponse>
    {
        private readonly ApplicationDbContext _db;

        public GetCurrentUserHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<GetCurrentUserResponse> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var user = await _db.Users
                .Select(u => new GetCurrentUserResponse
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    IdentificationNumber = u.IdentificationNumber
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null)
                return new GetCurrentUserResponse
                {
                    Success = false,
                    StatusCode = 404,
                    UserMessage = "User not found"
                };

            return user;
        }
    }
}
