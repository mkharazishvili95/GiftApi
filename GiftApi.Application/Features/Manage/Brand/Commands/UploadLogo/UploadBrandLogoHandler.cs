using GiftApi.Application.Features.File.Commands.Upload;
using GiftApi.Application.Interfaces;
using GiftApi.Domain.Enums.File;
using MediatR;

namespace GiftApi.Application.Features.Manage.Brand.Commands.UploadLogo
{
    public class UploadBrandLogoHandler : IRequestHandler<UploadBrandLogoCommand, UploadBrandLogoResponse>
    {
        readonly IMediator _mediator;
        readonly IBrandRepository _brandRepository;
        readonly IUnitOfWork _unitOfWork;

        public UploadBrandLogoHandler(
            IMediator mediator,
            IBrandRepository brandRepository,
            IUnitOfWork unitOfWork)
        {
            _mediator = mediator;
            _brandRepository = brandRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<UploadBrandLogoResponse> Handle(UploadBrandLogoCommand request, CancellationToken cancellationToken)
        {
            if (request.BrandId <= 0)
                return new UploadBrandLogoResponse { Success = false, StatusCode = 400, Message = "BrandId is required." };

            if (string.IsNullOrWhiteSpace(request.FileName) || string.IsNullOrWhiteSpace(request.FileContent))
                return new UploadBrandLogoResponse { Success = false, StatusCode = 400, Message = "FileName and FileContent are required." };

            var brand = await _brandRepository.Get(request.BrandId);
            if (brand == null || brand.IsDeleted)
                return new UploadBrandLogoResponse { Success = false, StatusCode = 404, Message = "Brand not found." };

            var uploadResult = await _mediator.Send(new UploadFileCommand
            {
                FileName = request.FileName,
                FileContent = request.FileContent,
                FileType = FileType.Image
            }, cancellationToken);

            if (uploadResult == null || uploadResult.Success != true)
            {
                return new UploadBrandLogoResponse
                {
                    Success = false,
                    StatusCode = uploadResult?.StatusCode ?? 500,
                    Message = uploadResult?.Message ?? "File upload failed."
                };
            }

            brand.LogoUrl = uploadResult.FileUrl;
            brand.UpdateDate = DateTime.UtcNow.AddHours(4);
            await _brandRepository.Update(brand);
            await _unitOfWork.SaveChangesAsync();

            return new UploadBrandLogoResponse
            {
                Success = true,
                StatusCode = 200,
                Message = "Brand logo uploaded successfully.",
                BrandId = brand.Id,
                LogoUrl = brand.LogoUrl,
                FileId = uploadResult.Id
            };
        }
    }
}