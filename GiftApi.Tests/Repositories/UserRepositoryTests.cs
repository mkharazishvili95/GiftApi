using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GiftApi.Domain.Entities;
using GiftApi.Domain.Enums.User;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using GiftApi.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace GiftApi.Tests.Repositories
{
    [TestFixture]
    public class UserRepositoryTests
    {
        private ApplicationDbContext _db = null!;
        private UserRepository _repo = null!;
        private Guid _userId;
        private GiftApi.Domain.Entities.User _seedUser = null!;

        [SetUp]
        public async Task SetUp()
        {
            _db = DbContextHelper.GetInMemoryDbContext();
            _repo = new UserRepository(_db);

            _userId = Guid.NewGuid();
            _seedUser = new GiftApi.Domain.Entities.User
            {
                Id = _userId,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateTime(1990, 1, 1),
                IdentificationNumber = "12345678901",
                Email = "john@example.com",
                PhoneNumber = "+995599111111",
                UserName = "johndoe",
                Password = BCrypt.Net.BCrypt.HashPassword("Password123"),
                RegisterDate = DateTime.UtcNow,
                Balance = 5m,
                Type = UserType.User,
                EmailVerified = false
            };
            _db.Users.Add(_seedUser);
            await _db.SaveChangesAsync();
        }

        [TearDown]
        public void TearDown()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task Register_Should_Create_New_User()
        {
            var user = new GiftApi.Domain.Entities.User
            {
                FirstName = "Alice",
                LastName = "Smith",
                DateOfBirth = new DateTime(1995, 5, 5),
                IdentificationNumber = "98765432109",
                Email = "alice@example.com",
                PhoneNumber = "+995599222222",
                UserName = "alicesmith",
                Password = BCrypt.Net.BCrypt.HashPassword("SomePass123"),
                RegisterDate = DateTime.UtcNow
            };

            var created = await _repo.Register(user);

            created.Should().NotBeNull();
            created!.Id.Should().NotBe(Guid.Empty);
            created.Balance.Should().Be(0);
            created.Type.Should().Be(UserType.User);

            (await _repo.EmailExists("alice@example.com")).Should().BeTrue();
        }

        [Test]
        public async Task EmailExists_Should_Return_True_For_Existing_Email()
        {
            var exists = await _repo.EmailExists("john@example.com");
            exists.Should().BeTrue();
        }

        [Test]
        public async Task EmailExists_Should_Return_False_For_NonExisting_Email()
        {
            var exists = await _repo.EmailExists("nope@example.com");
            exists.Should().BeFalse();
        }

        [Test]
        public async Task UserNameExists_Should_Work()
        {
            (await _repo.UserNameExists("johndoe")).Should().BeTrue();
            (await _repo.UserNameExists("other")).Should().BeFalse();
        }

        [Test]
        public async Task PhoneNumberExists_Should_Work()
        {
            (await _repo.PhoneNumberExists("+995599111111")).Should().BeTrue();
            (await _repo.PhoneNumberExists("+995599000000")).Should().BeFalse();
        }

        [Test]
        public async Task IdentificationNumberExists_Should_Work()
        {
            (await _repo.IdentificationNumberExists("12345678901")).Should().BeTrue();
            (await _repo.IdentificationNumberExists("00000000000")).Should().BeFalse();
        }

        [Test]
        public async Task GetByIdAsync_Should_Return_User()
        {
            var user = await _repo.GetByIdAsync(_userId);
            user.Should().NotBeNull();
            user!.UserName.Should().Be("johndoe");
        }

        [Test]
        public async Task GetByIdAsync_Should_Return_Null_For_Unknown()
        {
            var user = await _repo.GetByIdAsync(Guid.NewGuid());
            user.Should().BeNull();
        }

        [Test]
        public async Task GetByUserNameAsync_Should_Return_User()
        {
            var user = await _repo.GetByUserNameAsync("johndoe");
            user.Should().NotBeNull();
            user!.Email.Should().Be("john@example.com");
        }

        [Test]
        public async Task GetByEmailAsync_Should_Return_User()
        {
            var user = await _repo.GetByEmailAsync("john@example.com");
            user.Should().NotBeNull();
            user!.UserName.Should().Be("johndoe");
        }

        [Test]
        public async Task RevokeRefreshTokenAsync_Should_Clear_Token()
        {
            var user = await _db.Users.FindAsync(_userId);
            user!.RefreshToken = "initial";
            user.RefreshTokenExpiry = DateTime.UtcNow.AddHours(1);
            await _db.SaveChangesAsync();

            await _repo.RevokeRefreshTokenAsync(user);

            var refreshed = await _db.Users.FindAsync(_userId);
            refreshed!.RefreshToken.Should().BeNull();
            refreshed.RefreshTokenExpiry.Should().BeNull();
        }

        [Test]
        public async Task TopUpBalance_Should_Increase_Balance()
        {
            var before = (await _db.Users.FindAsync(_userId))!.Balance;
            var ok = await _repo.TopUpBalance(_userId, 20m);
            ok.Should().BeTrue();
            var after = (await _db.Users.FindAsync(_userId))!.Balance;
            after.Should().Be(before + 20m);
        }

        [Test]
        public async Task TopUpBalance_Should_Fail_For_Invalid_User()
        {
            var ok = await _repo.TopUpBalance(Guid.NewGuid(), 5m);
            ok.Should().BeFalse();
        }

        [Test]
        public async Task TopUpBalance_Should_Fail_For_NonPositive()
        {
            (await _repo.TopUpBalance(_userId, 0m)).Should().BeFalse();
            (await _repo.TopUpBalance(_userId, -10m)).Should().BeFalse();
        }

        [Test]
        public async Task UpdatePasswordAsync_Should_Change_Password()
        {
            var user = await _repo.GetByIdAsync(_userId);
            var oldHash = user!.Password;

            await _repo.UpdatePasswordAsync(user, BCrypt.Net.BCrypt.HashPassword("NewPass456"));

            var updated = await _repo.GetByIdAsync(_userId);
            updated!.Password.Should().NotBe(oldHash);
            BCrypt.Net.BCrypt.Verify("NewPass456", updated.Password).Should().BeTrue();
        }

        [Test]
        public async Task SaveLog_Should_Create_LoginAudit()
        {
            var log = await _repo.SaveLog(_userId);
            log.Should().NotBeNull();
            var count = await _db.LoginAudits.CountAsync();
            count.Should().Be(1);
        }

        [Test]
        public async Task GetLoginAuditsAsync_Should_Filter_And_Paginate()
        {
            for (int i = 0; i < 15; i++)
                await _repo.SaveLog(_userId);

            var (itemsPage1, total) = await _repo.GetLoginAuditsAsync(_userId, null, null, 1, 10, CancellationToken.None);
            var (itemsPage2, _) = await _repo.GetLoginAuditsAsync(_userId, null, null, 2, 10, CancellationToken.None);

            total.Should().Be(15);
            itemsPage1.Count.Should().Be(10);
            itemsPage2.Count.Should().Be(5);
        }

        [Test]
        public async Task CreatePasswordResetTokenAsync_Should_Persist()
        {
            string raw = Guid.NewGuid().ToString("N");
            var entity = await _repo.CreatePasswordResetTokenAsync(_userId, raw, TimeSpan.FromMinutes(30));

            entity.Should().NotBeNull();
            entity.UserId.Should().Be(_userId);
            entity.Used.Should().BeFalse();
            (await _db.PasswordResetTokens.CountAsync()).Should().Be(1);
        }

        [Test]
        public async Task GetPasswordResetTokenAsync_Should_Return_Token()
        {
            string raw = Guid.NewGuid().ToString("N");
            await _repo.CreatePasswordResetTokenAsync(_userId, raw, TimeSpan.FromMinutes(30));

            var fetched = await _repo.GetPasswordResetTokenAsync(raw);
            fetched.Should().NotBeNull();
            fetched!.UserId.Should().Be(_userId);
        }

        [Test]
        public async Task MarkPasswordResetTokenUsedAsync_Should_Set_Used()
        {
            string raw = Guid.NewGuid().ToString("N");
            var token = await _repo.CreatePasswordResetTokenAsync(_userId, raw, TimeSpan.FromMinutes(30));

            await _repo.MarkPasswordResetTokenUsedAsync(token);

            var tracked = await _db.PasswordResetTokens.FindAsync(token.Id);
            tracked!.Used.Should().BeTrue();
        }

        [Test]
        public async Task CreateEmailVerificationTokenAsync_Should_Persist()
        {
            string raw = Guid.NewGuid().ToString("N");
            var token = await _repo.CreateEmailVerificationTokenAsync(_userId, raw, TimeSpan.FromHours(1));

            token.Should().NotBeNull();
            token.UserId.Should().Be(_userId);
            token.Used.Should().BeFalse();
            (await _db.EmailVerificationTokens.CountAsync()).Should().Be(1);
        }

        [Test]
        public async Task GetEmailVerificationTokenAsync_Should_Return_Token()
        {
            string raw = Guid.NewGuid().ToString("N");
            await _repo.CreateEmailVerificationTokenAsync(_userId, raw, TimeSpan.FromHours(1));

            var fetched = await _repo.GetEmailVerificationTokenAsync(raw);
            fetched.Should().NotBeNull();
            fetched!.UserId.Should().Be(_userId);
        }

        [Test]
        public async Task MarkEmailVerificationTokenUsedAsync_Should_Set_Used()
        {
            string raw = Guid.NewGuid().ToString("N");
            var token = await _repo.CreateEmailVerificationTokenAsync(_userId, raw, TimeSpan.FromHours(1));

            await _repo.MarkEmailVerificationTokenUsedAsync(token);

            var tracked = await _db.EmailVerificationTokens.FindAsync(token.Id);
            tracked!.Used.Should().BeTrue();
        }

        [Test]
        public async Task MarkEmailVerifiedAsync_Should_Set_Flag()
        {
            var user = await _repo.GetByIdAsync(_userId);
            user!.EmailVerified.Should().BeFalse();

            await _repo.MarkEmailVerifiedAsync(user);

            var tracked = await _db.Users.FindAsync(_userId);
            tracked!.EmailVerified.Should().BeTrue();
        }

        [Test]
        public async Task GetAllUsers_Should_Return_Queryable()
        {
            var q = _repo.GetAllUsers();
            q.Should().NotBeNull();
            q.Count().Should().Be(1);
        }

        [Test]
        public async Task UpdateUserAsync_Should_Modify_Fields()
        {
            var snapshot = await _repo.GetByIdAsync(_userId);
            snapshot!.FirstName.Should().Be("John");

            snapshot.FirstName = "Changed";
            snapshot.LastName = "Updated";
            snapshot.Balance = 123.45m;
            await _repo.UpdateUserAsync(snapshot);

            var tracked = await _db.Users.FindAsync(_userId);
            tracked!.FirstName.Should().Be("Changed");
            tracked.LastName.Should().Be("Updated");
            tracked.Balance.Should().Be(123.45m);
        }

        [Test]
        public async Task UpdatePasswordAsync_Should_NoOp_When_User_Not_Found()
        {
            var ghost = new GiftApi.Domain.Entities.User { Id = Guid.NewGuid(), Password = "X" };
            await _repo.UpdatePasswordAsync(ghost, "NewHash");
            (await _db.Users.CountAsync()).Should().Be(1);
        }

        [Test]
        public async Task RevokeRefreshTokenAsync_Should_NoOp_When_User_Not_Found()
        {
            var ghost = new GiftApi.Domain.Entities.User { Id = Guid.NewGuid() };
            await _repo.RevokeRefreshTokenAsync(ghost);
            var original = await _db.Users.FindAsync(_userId);
            original!.Id.Should().Be(_userId);
        }
    }
}