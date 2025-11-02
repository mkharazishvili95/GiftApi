using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Manage.Brand.Commands.Create
{
    public class CreateBrandHandler : IRequestHandler<CreateBrandCommand, CreateBrandResponse>
    {
        readonly IUnitOfWork _unitOfWork;
        readonly IBrandRepository _brandRepository;
        readonly ICategoryRepository _categoryRepository;
        public CreateBrandHandler(IUnitOfWork unitOfWork, IBrandRepository brandRepository, ICategoryRepository categoryRepository)
        {
            _unitOfWork = unitOfWork;
            _brandRepository = brandRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<CreateBrandResponse> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Name))
                return new CreateBrandResponse { StatusCode = 400, Success = false, Message = "Brand name is required." };

            if (request.CategoryId <= 0)
                return new CreateBrandResponse { StatusCode = 400, Success = false, Message = "CategoryId is required" };

            var categoryExists = await _categoryRepository.CategoryExists(request.CategoryId);

            if (!categoryExists)
                return new CreateBrandResponse { StatusCode = 404, Success = false, Message = "Category not found." };

            var brand = new GiftApi.Domain.Entities.Brand
            {
                Name = request.Name,
                Description = request.Description,
                LogoUrl = request.LogoUrl,
                Website = request.Website,
                CreateDate = DateTime.UtcNow.AddHours(4),
                DeleteDate = null,
                IsDeleted = false,
                UpdateDate = null,
                CategoryId = request.CategoryId
            };

            await _brandRepository.Create(brand);
            await _unitOfWork.SaveChangesAsync();

            return new CreateBrandResponse { StatusCode = 200, Success = true, Id = brand.Id, Name = brand.Name, Description = brand.Description, LogoUrl = brand.LogoUrl, Website = brand.Website };
        }
    }
}
