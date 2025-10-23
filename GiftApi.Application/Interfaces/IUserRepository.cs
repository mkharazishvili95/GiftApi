using GiftApi.Domain.Entities;

namespace GiftApi.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> Register(User user);
        Task<bool> EmailExists(string email);
        Task<bool> UserNameExists(string userName);
        Task<bool> PhoneNumberExists(string phoneNumber);
        Task<bool> IdentificationNumberExists(string idNumber);
        Task<User?> GetByUserNameAsync(string userName);
        Task UpdateRefreshTokenAsync(User user, string refreshToken, DateTime expiry);
        IQueryable<User> GetAllUsers();

        Task<User?> GetCurrentUserAsync(Guid userId);
    }
}
