using GiftApi.Infrastructure.Data;
using MediatR;

namespace GiftApi.Application.Payment.Commands.TopUp
{
    public class TopUpBalanceHandler : IRequestHandler<TopUpBalanceCommand, TopUpBalanceResponse>
    {
        readonly ApplicationDbContext _db;
        public TopUpBalanceHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<TopUpBalanceResponse> Handle(TopUpBalanceCommand request, CancellationToken cancellationToken)
        {
            if(request.UserId == Guid.Empty)
                return new TopUpBalanceResponse { Success = false, UserMessage = "UserId is required.", StatusCode = 400 };

            if(request.Amount <= 0)
                return new TopUpBalanceResponse { Success = false, UserMessage = "Amount must be greater than zero.", StatusCode = 400 };

            var user = await _db.Users.FindAsync(request.UserId);

            if (user == null)
                return new TopUpBalanceResponse { Success = false, UserMessage = $"User with Id {request.UserId} not found.", StatusCode = 404 };

            user.Balance += request.Amount;
            await _db.SaveChangesAsync(cancellationToken);

            return new TopUpBalanceResponse { Success = true, UserMessage = "Balance topped up successfully.", StatusCode = 200, NewBalance = user.Balance };
        }
    }
}
