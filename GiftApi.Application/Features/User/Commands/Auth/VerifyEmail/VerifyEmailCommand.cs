using MediatR;

namespace GiftApi.Application.Features.User.Commands.Auth.VerifyEmail
{
    public class VerifyEmailCommand : IRequest<VerifyEmailResponse>
    {
        public string Token { get; set; } = null!;
    }
}