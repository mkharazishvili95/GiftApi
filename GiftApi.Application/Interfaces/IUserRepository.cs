using GiftApi.Domain.Entities;

namespace GiftApi.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> Register(User user);
        Task<bool> EmailExists(string email);
        Task<bool> UserNameExists(string userName);
        Task<bool> PhoneNumberExists(string phoneNumber);
        Task<bool> IdentificationNumberExists(string idNumber);
    }
}
