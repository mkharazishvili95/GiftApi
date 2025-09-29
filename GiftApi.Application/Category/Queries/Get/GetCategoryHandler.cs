using GiftApi.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Application.Category.Queries.Get
{
    public class GetCategoryHandler : IRequestHandler<GetCategoryQuery, GetCategoryResponse>
    {
        readonly ApplicationDbContext _db;
        public GetCategoryHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<GetCategoryResponse> Handle(GetCategoryQuery request, CancellationToken cancellationToken)
        {
            if(request.Id <= 0)
                return new GetCategoryResponse { StatusCode = 400, Success = false, UserMessage = "Id is required." };

            var category = await _db.Categories
                .Include(x => x.Brands)
                .FirstOrDefaultAsync(x => x.Id == request.Id);

            if (category == null)
                return new GetCategoryResponse { StatusCode = 404, Success = false, UserMessage = "Category not found." };

            var response = new GetCategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsDeleted = category.IsDeleted,
                DeleteDate = category.DeleteDate,
                CreateDate = category.CreateDate,
                UpdateDate = category.UpdateDate,
                Logo = category.Logo, 
                Brands = category.Brands.Select(b => new Application.Brand.DTOs.BrandDto
                {
                    BrandId = b.Id,
                    Name = b.Name,
                    Description = b.Description,
                    LogoUrl = b.LogoUrl,
                    Website = b.Website,
                    CreateDate = b.CreateDate,
                    UpdateDate = b.UpdateDate
                }).ToList(),
                StatusCode = 200, 
                Success = true
            };

            return response;
        }
    }
}
