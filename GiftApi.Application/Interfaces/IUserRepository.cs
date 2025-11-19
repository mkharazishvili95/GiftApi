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
        Task<bool> TopUpBalance(Guid userId, decimal amount);
        Task<User?> GetCurrentUserAsync(Guid userId);
        Task UpdateUserAsync(User user);
        Task<LoginAudit?> SaveLog(Guid userId);
        Task<(List<LoginAudit> Items, int TotalCount)> GetLoginAuditsAsync(Guid? userId, DateTime? fromUtc, DateTime? toUtc, int page, int pageSize, CancellationToken cancellationToken);
        Task UpdatePasswordAsync(User user, string newHashedPassword);

        // ავტორიზაციისთვის:
        Task RevokeRefreshTokenAsync(User user);
        Task<User?> GetByEmailAsync(string email);
        Task<PasswordResetToken> CreatePasswordResetTokenAsync(Guid userId, string rawToken, TimeSpan lifetime);
        Task<PasswordResetToken?> GetPasswordResetTokenAsync(string rawToken);
        Task MarkPasswordResetTokenUsedAsync(PasswordResetToken token);
        Task<EmailVerificationToken> CreateEmailVerificationTokenAsync(Guid userId, string rawToken, TimeSpan lifetime);
        Task<EmailVerificationToken?> GetEmailVerificationTokenAsync(string rawToken);
        Task MarkEmailVerifiedAsync(User user);
        Task MarkEmailVerificationTokenUsedAsync(EmailVerificationToken token);
    }
}
