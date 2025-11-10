using System.Security.Claims;

namespace GiftApi.Application.Interfaces
{
    public interface ICurrentUserRepository
    {
        Guid? GetUserId();
        ClaimsPrincipal? GetUser();
    }
}
