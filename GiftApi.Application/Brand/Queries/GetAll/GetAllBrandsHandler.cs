using GiftApi.Application.Category.DTOs;
using GiftApi.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Application.Brand.Queries.GetAll
{
    public class GetAllBrandsHandler : IRequestHandler<GetAllBrandsQuery, GetAllBrandsResponse>
    {
        private readonly ApplicationDbContext _db;

        public GetAllBrandsHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<GetAllBrandsResponse> Handle(GetAllBrandsQuery request, CancellationToken cancellationToken)
        {
            var query = _db.Brands
                .Include(b => b.Category)
                .Where(b => !b.IsDeleted)
                .AsQueryable();

            var totalCount = await query.CountAsync(cancellationToken);

            var brands = await query
                .OrderBy(b => b.CreateDate)
                .Skip((request.Pagination.Page - 1) * request.Pagination.PageSize)
                .Take(request.Pagination.PageSize)
                .Select(b => new GetAllBrandsItemsResponse
                {
                    Id = b.Id,
                    Name = b.Name,
                    Description = b.Description,
                    LogoUrl = b.LogoUrl,
                    Website = b.Website,
                    IsDeleted = b.IsDeleted,
                    DeleteDate = b.DeleteDate,
                    CreateDate = b.CreateDate,
                    UpdateDate = b.UpdateDate,
                    Category = (b.Category == null || b.Category.IsDeleted)
                        ? new CategoryDto()
                        : new CategoryDto
                        {
                            CategoryId = b.Category.Id,
                            Name = b.Category.Name,
                            Description = b.Category.Description,
                            IsDeleted = b.Category.IsDeleted,
                            DeleteDate = b.Category.DeleteDate,
                            CreateDate = b.Category.CreateDate,
                            UpdateDate = b.Category.UpdateDate,
                            Logo = b.Category.Logo
                        }
                })
                .ToListAsync(cancellationToken);

            return new GetAllBrandsResponse
            {
                TotalCount = totalCount,
                Items = brands
            };
        }
    }
}
