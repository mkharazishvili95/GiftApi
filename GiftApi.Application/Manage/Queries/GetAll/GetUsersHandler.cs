using GiftApi.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Application.Manage.Queries.GetUsers
{
    public class GetUsersHandler : IRequestHandler<GetUsersQuery, GetUsersResponse>
    {
        readonly ApplicationDbContext _db;

        public GetUsersHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<GetUsersResponse> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            var query = _db.Users.AsQueryable();

            var totalCount = await query.CountAsync(cancellationToken);

            var users = await query
                .OrderBy(u => u.RegisterDate) 
                .Skip((request.Pagination.Page - 1) * request.Pagination.PageSize)
                .Take(request.Pagination.PageSize)
                .Select(u => new GetUsersItemsResponse
                {
                    Id = u.Id,
                    FirstName = u.FirstName, 
                    LastName = u.LastName,
                    DateOfBirth = u.DateOfBirth,
                    IdentificationNumber = u.IdentificationNumber,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    UserName = u.UserName,
                    Gender = u.Gender,
                    Balance = u.Balance,
                    Type = u.Type,
                    RegisterDate = u.RegisterDate
                })
                .ToListAsync(cancellationToken);

            var response = new GetUsersResponse
            {
                TotalCount = totalCount,
                Items = users
            };

            return response;
        }
    }
}
