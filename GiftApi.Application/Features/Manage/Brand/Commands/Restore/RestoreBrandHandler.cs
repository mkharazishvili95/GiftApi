using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Manage.Brand.Commands.Restore
{
    public class RestoreBrandHandler : IRequestHandler<RestoreBrandCommand, RestoreBrandResponse>
    {
        readonly IUnitOfWork _unitOfWork;
        readonly IBrandRepository _brandRepository;
        public RestoreBrandHandler(IUnitOfWork unitOfWork, IBrandRepository brandRepository)
        {
            _unitOfWork = unitOfWork;
            _brandRepository = brandRepository;
        }

        public async Task<RestoreBrandResponse> Handle(RestoreBrandCommand request, CancellationToken cancellationToken)
        {
            if(request.Id <= 0)
                return new RestoreBrandResponse { Success = false, StatusCode = 400, Message = "Brand Id is required." };

            var brand = await _brandRepository.Get(request.Id);

            if (brand == null)
                return new RestoreBrandResponse { Success = false, StatusCode = 404, Message = "Brand not found." };

            if(!brand.IsDeleted)
                return new RestoreBrandResponse { Success = false, StatusCode = 400, Message = "Brand is not deleted." };

            var result = await _brandRepository.Restore(request.Id);

            if(!result)
                return new RestoreBrandResponse { Success = false, StatusCode = 500, Message = "Failed to restore brand." };

            await _unitOfWork.SaveChangesAsync();
            return new RestoreBrandResponse { Success = true, StatusCode = 200, Message = "Brand restored successfully." };
        }
    }
}
