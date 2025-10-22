using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Repositories
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;

        public InMemoryUserRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> EmailExists(string email)
            => await _db.Users.AnyAsync(u => u.Email == email);

        public async Task<bool> PhoneNumberExists(string phoneNumber)
            => await _db.Users.AnyAsync(u => u.PhoneNumber == phoneNumber);

        public async Task<bool> IdentificationNumberExists(string id)
            => await _db.Users.AnyAsync(u => u.IdentificationNumber == id);

        public async Task<bool> UserNameExists(string userName)
            => await _db.Users.AnyAsync(u => u.UserName == userName);

        public async Task<GiftApi.Domain.Entities.User?> Register(GiftApi.Domain.Entities.User user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }
    }

}
