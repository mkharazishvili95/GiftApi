using GiftApi.Application.Common.Models;

namespace GiftApi.Application.Features.User.Commands.Auth.ForgotPassword
{
    public class ForgotPasswordResponse : BaseResponse
    {
        public string? PublicToken { get; set; }
    }
}