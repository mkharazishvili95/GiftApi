using GiftApi.Application.Interfaces;
using GiftApi.Domain.Entities;
using GiftApi.Domain.Enums.User;
using GiftApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        readonly ApplicationDbContext _db;
        public UserRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetCurrentUserAsync(Guid userId)
        {
            return await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<bool> EmailExists(string email)
        {
            return await _db.Users.AnyAsync(u => u.Email.ToUpper() == email.ToUpper());
        }
        public async Task<bool> UserNameExists(string userName)
        {
            return await _db.Users.AnyAsync(u => u.UserName.ToUpper() == userName.ToUpper());
        }
        public async Task<bool> PhoneNumberExists(string phoneNumber)
        {
            return await _db.Users.AnyAsync(u => u.PhoneNumber == phoneNumber);
        }
        public async Task<bool> IdentificationNumberExists(string identificationNumber)
        {
            return await _db.Users.AnyAsync(u => u.IdentificationNumber == identificationNumber);
        }

        public async Task<User?> Register(User user)
        {
            try
            {
                var newUser = new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IdentificationNumber = user.IdentificationNumber,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Password = user.Password,
                    DateOfBirth = user.DateOfBirth,
                    Gender = user.Gender,
                    UserName = user.UserName,
                    RegisterDate = DateTime.UtcNow.AddHours(4),
                    Balance = 0,
                    Type = UserType.User
                };

                _db.Users.Add(newUser);
                await _db.SaveChangesAsync();

                return newUser;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
        public async Task<User?> GetByUserNameAsync(string userName)
        {
            return await _db.Users
                .FirstOrDefaultAsync(u => u.UserName.ToUpper() == userName.ToUpper());
        }

        public async Task UpdateRefreshTokenAsync(User user, string refreshToken, DateTime expiry)
        {
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = expiry;
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }

        public IQueryable<User> GetAllUsers()
        {
            return _db.Users.AsNoTracking();
        }

        public async Task<bool> TopUpBalance(Guid userId, decimal amount)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return false;

            if(amount <= 0) return false;

            user.Balance += amount;
            _db.Users.Update(user);
            await _db.SaveChangesAsync();

            return true;
        }

        public async Task UpdateUserAsync(User user)
        {
            var trackedUser = await _db.Users.FindAsync(user.Id);
            if (trackedUser != null)
            {
                trackedUser.Balance = user.Balance;
                trackedUser.FirstName = user.FirstName;
                trackedUser.LastName = user.LastName;
            }
            else
            {
                _db.Users.Attach(user);
                _db.Entry(user).State = EntityState.Modified;
            }
        }

        public async Task<LoginAudit?> SaveLog(Guid userId)
        {
            var log = new LoginAudit
            {
                UserId = userId,
                LoginDate = DateTime.UtcNow.AddHours(4)
            };
            _db.LoginAudits.Add(log);
            await _db.SaveChangesAsync();
            return log;
        }
    }
}
