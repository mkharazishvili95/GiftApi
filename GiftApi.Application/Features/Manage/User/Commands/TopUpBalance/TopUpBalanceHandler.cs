using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Manage.User.Commands.TopUpBalance
{
    public class TopUpBalanceHandler : IRequestHandler<TopUpBalanceCommand, TopUpBalanceResponse>
    {
        readonly IUserRepository _userRepository;
        public TopUpBalanceHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<TopUpBalanceResponse> Handle(TopUpBalanceCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);

            if (user == null)
                return new TopUpBalanceResponse { Success = false, Message = "User not found", StatusCode = 404 };

            if (request.Amount <= 0)
                return new TopUpBalanceResponse { Success = false, Message = "Amount must be greater than zero", StatusCode = 400 };

            var oldBalance = user.Balance;
            var userName = user.UserName;

            var result = await _userRepository.TopUpBalance(request.UserId, request.Amount);

            if (!result)
                return new TopUpBalanceResponse { Success = false, Message = "Failed to top up balance", StatusCode = 500 };

            var newBalance = oldBalance + request.Amount;
            return new TopUpBalanceResponse { Success = true, Message = "Balance topped up successfully", StatusCode = 200, NewBalance = newBalance, OldBalance = oldBalance, UserName = userName };
        }
    }
}
