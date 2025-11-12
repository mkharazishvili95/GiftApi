using GiftApi.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Application.Features.Manage.User.Queries.GetAllUsers
{
    public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, GetAllUsersResponse>
    {
        readonly IUserRepository _userRepository;

        public GetAllUsersHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<GetAllUsersResponse> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var query = _userRepository.GetAllUsers();

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(u => u.RegisterDate)
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

            return new GetAllUsersResponse
            {
                TotalCount = totalCount,
                Items = items,
                StatusCode = 200,
                Success = true
            };
        }
    }
}
