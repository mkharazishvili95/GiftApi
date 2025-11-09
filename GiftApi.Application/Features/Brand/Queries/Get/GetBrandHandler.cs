using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Brand.Queries.Get
{
    public class GetBrandHandler : IRequestHandler<GetBrandQuery, GetBrandResponse>
    {
        readonly IBrandRepository _brandRepository;
        public GetBrandHandler(IBrandRepository brandRepository)
        {
            _brandRepository = brandRepository;
        }

        public async Task<GetBrandResponse> Handle(GetBrandQuery request, CancellationToken cancellationToken)
        {
            if (request.Id <= 0)
                return new GetBrandResponse { StatusCode = 400, Success = false, Message = "Brand ID is required." };

            var brand = await _brandRepository.Get(request.Id);

            if (brand == null)
                return new GetBrandResponse { StatusCode = 404, Success = false, Message = "Brand not found." };

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
                StatusCode = 200,
                Success = true
            };

            return response;
        }
    }
}
