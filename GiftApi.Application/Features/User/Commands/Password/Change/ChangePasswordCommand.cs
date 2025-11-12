using MediatR;

namespace GiftApi.Application.Features.User.Commands.Password.Change
{
    public class ChangePasswordCommand : IRequest<ChangePasswordResponse>
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}