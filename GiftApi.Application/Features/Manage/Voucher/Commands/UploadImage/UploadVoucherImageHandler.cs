using GiftApi.Application.Features.File.Commands.Upload;
using GiftApi.Application.Interfaces;
using GiftApi.Domain.Enums.File;
using MediatR;

namespace GiftApi.Application.Features.Manage.Voucher.Commands.UploadImage
{
    public class UploadVoucherImageHandler : IRequestHandler<UploadVoucherImageCommand, UploadVoucherImageResponse>
    {
        readonly IMediator _mediator;
        readonly IVoucherRepository _voucherRepository;
        readonly IUnitOfWork _unitOfWork;

        public UploadVoucherImageHandler(
            IMediator mediator,
            IVoucherRepository voucherRepository,
            IUnitOfWork unitOfWork)
        {
            _mediator = mediator;
            _voucherRepository = voucherRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<UploadVoucherImageResponse> Handle(UploadVoucherImageCommand request, CancellationToken cancellationToken)
        {
            if (request.Id == Guid.Empty)
                return new UploadVoucherImageResponse { Success = false, StatusCode = 400, Message = "Voucher Id is required." };

            if (string.IsNullOrWhiteSpace(request.FileName) || string.IsNullOrWhiteSpace(request.FileContent))
                return new UploadVoucherImageResponse { Success = false, StatusCode = 400, Message = "FileName and FileContent are required." };

            var voucher = await _voucherRepository.GetByIdAsync(request.Id);
            if (voucher == null || voucher.IsDeleted)
                return new UploadVoucherImageResponse { Success = false, StatusCode = 404, Message = "Voucher not found." };

            var uploadResult = await _mediator.Send(new UploadFileCommand
            {
                FileName = request.FileName,
                FileContent = request.FileContent,
                FileType = FileType.Image
            }, cancellationToken);

            if (uploadResult == null || uploadResult.Success != true)
            {
                return new UploadVoucherImageResponse
                {
                    Success = false,
                    StatusCode = uploadResult?.StatusCode ?? 500,
                    Message = uploadResult?.Message ?? "Image upload failed."
                };
            }

            voucher.ImageUrl = uploadResult.FileUrl;
            voucher.UpdateDate = DateTime.UtcNow.AddHours(4);
            await _voucherRepository.Edit(voucher);
            await _unitOfWork.SaveChangesAsync();

            return new UploadVoucherImageResponse
            {
                Success = true,
                StatusCode = 200,
                Message = "Voucher image replaced successfully.",
                VoucherId = voucher.Id,
                ImageUrl = voucher.ImageUrl,
                FileId = uploadResult.Id
            };
        }
    }
}