using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Manage.Brand.Commands.Edit
{
    public class EditBrandHandler : IRequestHandler<EditBrandCommand, EditBrandResponse>
    {
        readonly IBrandRepository _brandRepository;
        readonly ICategoryRepository _categoryRepository;
        readonly IUnitOfWork _unitOfWork;
        public EditBrandHandler(IBrandRepository brandRepository, ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
        {
            _brandRepository = brandRepository;
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<EditBrandResponse> Handle(EditBrandCommand request, CancellationToken cancellationToken)
        {
            if (request.Id <= 0)
                return new EditBrandResponse { StatusCode = 400, Success = false, Message = "Brand Id is required." };

            var brand = _brandRepository.Get(request.Id).Result;

            if (brand == null || brand.IsDeleted)
                return new EditBrandResponse { StatusCode = 404, Success = false, Message = "Brand not found." };

            if (string.IsNullOrEmpty(request.Name))
                return new EditBrandResponse { StatusCode = 400, Success = false, Message = "Brand name is required." };

            if (request.CategoryId.HasValue)
            {
                if (request.CategoryId <= 0)
                    return new EditBrandResponse { StatusCode = 400, Success = false, Message = "CategoryId must be greater than zero." };

                var categoryExists = await _categoryRepository.CategoryExists(request.CategoryId.Value);

                if (!categoryExists)
                    return new EditBrandResponse { StatusCode = 404, Success = false, Message = "Category not found." };

                brand.CategoryId = request.CategoryId.Value;
            }

            brand.Name = request.Name;
            brand.Description = request.Description;
            brand.LogoUrl = request.LogoUrl;
            brand.Website = request.Website;
            brand.UpdateDate = DateTime.UtcNow.AddHours(4);

            await _brandRepository.Update(brand);
            await _unitOfWork.SaveChangesAsync();

            return new EditBrandResponse
            {
                StatusCode = 200,
                Success = true,
                Message = "Brand edited successfully."
            };
        }
    }
}
