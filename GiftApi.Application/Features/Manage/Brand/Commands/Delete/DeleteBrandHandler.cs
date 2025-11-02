using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Manage.Brand.Commands.Delete
{
    public class DeleteBrandHandler : IRequestHandler<DeleteBrandCommand, DeleteBrandResponse>
    {
        readonly IUnitOfWork _unitOfWork;
        readonly IBrandRepository _brandRepository;
        public DeleteBrandHandler(IUnitOfWork unitOfWork, IBrandRepository brandRepository)
        {
            _unitOfWork = unitOfWork;
            _brandRepository = brandRepository;
        }

        public async Task<DeleteBrandResponse> Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
        {
            if (request.Id <= 0)
                return new DeleteBrandResponse { Success = false, StatusCode = 400, Message = "BrandId is required." };

            var brand = await _brandRepository.Get(request.Id);

            if(brand == null)
                return new DeleteBrandResponse { Success = false, StatusCode = 404, Message = "Brand not found" };

            if(brand.IsDeleted)
                return new DeleteBrandResponse { Success = false, StatusCode = 400, Message = "Brand is already deleted" };

            var result = await _brandRepository.Delete(request.Id);
            if(!result)
                return new DeleteBrandResponse { Success = false, StatusCode = 500, Message = "Failed to delete brand" };

            await _unitOfWork.SaveChangesAsync();
            return new DeleteBrandResponse { Success = true, StatusCode = 200, Message = "Brand deleted successfully" };
        }
    }
}
