using GiftApi.Application.Common.Models;

namespace GiftApi.Application.Features.User.Commands.Auth.VerifyEmail
{
    public class VerifyEmailResponse : BaseResponse
    {
        public bool AlreadyVerified { get; set; }
    }
}