using GiftApi.Application.Brand.DTOs;
using GiftApi.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Application.Category.Queries.GetAll
{
    public class GetAllCategoriesHandler : IRequestHandler<GetAllCategoriesQuery, GetAllCategoriesResponse>
    {
        private readonly ApplicationDbContext _db;

        public GetAllCategoriesHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<GetAllCategoriesResponse> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            var query = _db.Categories
                .Include(c => c.Brands)
                .Where(c => !c.IsDeleted)
                .AsQueryable();

            var totalCount = await query.CountAsync(cancellationToken);

            var categories = await query
                .OrderBy(c => c.CreateDate)
                .Skip((request.Pagination.Page - 1) * request.Pagination.PageSize)
                .Take(request.Pagination.PageSize)
                .Select(c => new GetAllCategoriesItemsResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IsDeleted = c.IsDeleted,
                    DeleteDate = c.DeleteDate,
                    CreateDate = c.CreateDate,
                    UpdateDate = c.UpdateDate,
                    Logo = c.Logo,
                    Brands = c.Brands
                        .Where(b => !b.IsDeleted)
                        .Select(b => new BrandDto
                        {
                            BrandId = b.Id,
                            Name = b.Name,
                            Description = b.Description,
                            LogoUrl = b.LogoUrl,
                            Website = b.Website,
                            IsDeleted = b.IsDeleted,
                            DeleteDate = b.DeleteDate,
                            CreateDate = b.CreateDate,
                            UpdateDate = b.UpdateDate
                        })
                        .ToList()
                })
                .ToListAsync(cancellationToken);

            return new GetAllCategoriesResponse
            {
                TotalCount = totalCount,
                Items = categories
            };
        }
    }
}
