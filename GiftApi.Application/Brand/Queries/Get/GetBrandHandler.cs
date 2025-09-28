using GiftApi.Infrastructure.Data;
using MediatR;

namespace GiftApi.Application.Brand.Queries.Get
{
    public class GetBrandHandler : IRequestHandler<GetBrandQuery, GetBrandResponse>
    {
        readonly ApplicationDbContext _db;
        public GetBrandHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<GetBrandResponse> Handle(GetBrandQuery request, CancellationToken cancellationToken)
        {
            if(request.Id <= 0)
                return new GetBrandResponse { StatusCode = 400, Success = false, UserMessage = "Brand ID is required." };

            var brand = await _db.Brands.FindAsync(request.Id);

            if (brand == null)
                return new GetBrandResponse { StatusCode = 404, Success = false, UserMessage = "Brand not found." };

            var response = new GetBrandResponse
            {
                Id = brand.Id,
                Name = brand.Name,
                Description = brand.Description,
                LogoUrl = brand.LogoUrl,
                Website = brand.Website,
                CreateDate = brand.CreateDate,
                UpdateDate = brand.UpdateDate,
                CategoryId = brand.CategoryId,
                Category = brand.Category,
                StatusCode = 200,
                Success = true
            };  

            return response;
        }
    }
}
