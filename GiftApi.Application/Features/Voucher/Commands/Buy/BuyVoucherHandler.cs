using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Voucher.Commands.Buy
{
    public class BuyVoucherHandler : IRequestHandler<BuyVoucherCommand, BuyVoucherResponse>
    {
        readonly IVoucherRepository _voucherRepository;
        readonly IUserRepository _userRepository;
        readonly IUnitOfWork _unitOfWork;

        public BuyVoucherHandler(
            IVoucherRepository voucherRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork)
        {
            _voucherRepository = voucherRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<BuyVoucherResponse> Handle(BuyVoucherCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
                return new BuyVoucherResponse
                {
                    Success = false,
                    Message = $"User with Id {request.UserId} not found.",
                    StatusCode = 404
                };

            var voucher = await _voucherRepository.GetByIdAsync(request.VoucherId);
            if (voucher == null || !voucher.IsActive || voucher.IsDeleted)
                return new BuyVoucherResponse
                {
                    Success = false,
                    Message = $"Voucher with Id {request.VoucherId} not found or inactive.",
                    StatusCode = 404
                };

            if (voucher.Quantity < request.Quantity)
                return new BuyVoucherResponse
                {
                    Success = false,
                    Message = "Not enough voucher quantity available.",
                    StatusCode = 400
                };

            var fullPrice = (request.Quantity * voucher.Amount);

            if (user.Balance < fullPrice)
                return new BuyVoucherResponse
                {
                    Success = false,
                    Message = "Insufficient balance.",
                    StatusCode = 400
                };

            var result = await _voucherRepository.Buy(
                request.VoucherId,
                request.UserId,
                request.Quantity,
                request.RecipientName,
                request.RecipientPhone,
                request.RecipientCity,
                request.RecipientAddress,
                request.RecipientEmail,
                request.Message,
                request.SenderName
            );

            if (!result)
                return new BuyVoucherResponse
                {
                    Success = false,
                    Message = "Failed to process voucher purchase.",
                    StatusCode = 500
                };

            user.Balance -= fullPrice;
            await _userRepository.UpdateUserAsync(user); 

            await _unitOfWork.SaveChangesAsync();

            return new BuyVoucherResponse
            {
                Success = true,
                Message = "Voucher purchased successfully.",
                StatusCode = 200
            };
        }
    }
}
