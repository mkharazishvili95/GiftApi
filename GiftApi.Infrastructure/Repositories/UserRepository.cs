using GiftApi.Application.Interfaces;
using GiftApi.Domain.Entities;
using GiftApi.Domain.Enums.User;
using GiftApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

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

        public async Task<User?> GetByEmailAsync(string email) => await _db.Users.FirstOrDefaultAsync(u => u.Email.ToUpper() == email.ToUpper());

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

        public async Task<(List<LoginAudit> Items, int TotalCount)> GetLoginAuditsAsync(
            Guid? userId,
            DateTime? fromUtc,
            DateTime? toUtc,
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _db.LoginAudits.AsNoTracking().AsQueryable();

            if (userId.HasValue)
                query = query.Where(x => x.UserId == userId.Value);

            if (fromUtc.HasValue)
                query = query.Where(x => x.LoginDate >= fromUtc.Value);

            if (toUtc.HasValue)
                query = query.Where(x => x.LoginDate <= toUtc.Value);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(x => x.LoginDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task UpdatePasswordAsync(User user, string newHashedPassword)
        {
            var tracked = await _db.Users.FindAsync(user.Id);
            if (tracked == null) return;
            tracked.Password = newHashedPassword;
            await _db.SaveChangesAsync();
        }

        public async Task RevokeRefreshTokenAsync(User user)
        {
            var tracked = await _db.Users.FindAsync(user.Id);
            if (tracked == null) return;
            tracked.RefreshToken = null;
            tracked.RefreshTokenExpiry = null;
            await _db.SaveChangesAsync();
        }

        public async Task<PasswordResetToken> CreatePasswordResetTokenAsync(Guid userId, string rawToken, TimeSpan lifetime)
        {
            var entity = new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TokenHash = Hash(rawToken),
                CreatedUtc = DateTime.UtcNow,
                ExpiresUtc = DateTime.UtcNow.Add(lifetime),
                Used = false
            };
            _db.PasswordResetTokens.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<PasswordResetToken?> GetPasswordResetTokenAsync(string rawToken)
        {
            var hash = Hash(rawToken);
            return await _db.PasswordResetTokens.FirstOrDefaultAsync(x => x.TokenHash == hash);
        }

        public async Task MarkPasswordResetTokenUsedAsync(PasswordResetToken token)
        {
            var tracked = await _db.PasswordResetTokens.FindAsync(token.Id);
            if (tracked == null) return;
            tracked.Used = true;
            await _db.SaveChangesAsync();
        }

        public async Task<EmailVerificationToken> CreateEmailVerificationTokenAsync(Guid userId, string rawToken, TimeSpan lifetime)
        {
            var entity = new EmailVerificationToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TokenHash = Hash(rawToken),
                CreatedUtc = DateTime.UtcNow,
                ExpiresUtc = DateTime.UtcNow.Add(lifetime),
                Used = false
            };
            _db.EmailVerificationTokens.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<EmailVerificationToken?> GetEmailVerificationTokenAsync(string rawToken)
        {
            var hash = Hash(rawToken);
            return await _db.EmailVerificationTokens.FirstOrDefaultAsync(x => x.TokenHash == hash);
        }

        public async Task MarkEmailVerifiedAsync(User user)
        {
            var tracked = await _db.Users.FindAsync(user.Id);
            if (tracked == null) return;
            tracked.EmailVerified = true;
            await _db.SaveChangesAsync();
        }

        public async Task MarkEmailVerificationTokenUsedAsync(EmailVerificationToken token)
        {
            var tracked = await _db.EmailVerificationTokens.FindAsync(token.Id);
            if (tracked == null) return;
            tracked.Used = true;
            await _db.SaveChangesAsync();
        }

        static string Hash(string value)
        {
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(value)));
        }
    }
}
