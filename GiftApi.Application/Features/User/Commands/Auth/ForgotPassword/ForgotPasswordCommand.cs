using MediatR;

namespace GiftApi.Application.Features.User.Commands.Auth.ForgotPassword
{
    public class ForgotPasswordCommand : IRequest<ForgotPasswordResponse>
    {
        public string Email { get; set; } = null!;
    }
}