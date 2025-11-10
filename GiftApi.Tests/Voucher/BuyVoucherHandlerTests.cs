using GiftApi.Application.Features.Voucher.Commands.Buy;
using GiftApi.Domain.Enums.User;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GiftApi.Tests.Voucher
{
    [TestFixture]
    public class BuyVoucherHandlerTests
    {
        private ApplicationDbContext _db;
        private BuyVoucherHandler _handler;
        private VoucherRepository _voucherRepository;
        private UserRepository _userRepository;
        private UnitOfWork _unitOfWork;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning)) 
            .Options;

            _db = new ApplicationDbContext(options);
            _voucherRepository = new VoucherRepository(_db);
            _userRepository = new UserRepository(_db);
            _unitOfWork = new UnitOfWork(_db);

            _handler = new BuyVoucherHandler(_voucherRepository, _userRepository, _unitOfWork);
        }

        [TearDown]
        public void TearDown() => _db.Dispose();

        private GiftApi.Domain.Entities.User CreateUser(decimal balance = 1000)
        {
            return new GiftApi.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Password = "hashedpassword",
                PhoneNumber = "598336060",
                UserName = "johndoe",
                IdentificationNumber = "123456789",
                Balance = balance,
                DateOfBirth = DateTime.UtcNow.AddYears(-30),
                Type = UserType.User,
                RegisterDate = DateTime.UtcNow
            };
        }

        private GiftApi.Domain.Entities.Voucher CreateVoucher(int quantity = 5)
        {
            return new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "Test Voucher",
                Description = "Test Description",
                Amount = 50,
                IsPercentage = false,
                Quantity = quantity,
                SoldCount = 0,
                ValidMonths = 12,
                IsActive = true,
                IsDeleted = false
            };
        }

        [Test]
        public async Task Should_ReturnNotFound_WhenUserDoesNotExist()
        {
            var command = new BuyVoucherCommand
            {
                UserId = Guid.NewGuid(),
                VoucherId = Guid.NewGuid(),
                Quantity = 1,
                RecipientName = "John",
                RecipientPhone = "598123456",
                RecipientCity = "Tbilisi",
                RecipientAddress = "Street 1"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(404, result.StatusCode);
            Assert.That(result.Message, Does.Contain("User with Id"));
        }

        [Test]
        public async Task Should_ReturnNotFound_WhenVoucherDoesNotExistOrInactive()
        {
            var user = CreateUser();
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var command = new BuyVoucherCommand
            {
                UserId = user.Id,
                VoucherId = Guid.NewGuid(),
                Quantity = 1,
                RecipientName = "John",
                RecipientPhone = "598123456",
                RecipientCity = "Tbilisi",
                RecipientAddress = "Street 1"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(404, result.StatusCode);
            Assert.That(result.Message, Does.Contain("Voucher with Id"));
        }

        [Test]
        public async Task Should_ReturnBadRequest_WhenQuantityNotAvailable()
        {
            var user = CreateUser();
            var voucher = CreateVoucher(quantity: 1);

            _db.Users.Add(user);
            _db.Vouchers.Add(voucher);
            await _db.SaveChangesAsync();

            var command = new BuyVoucherCommand
            {
                UserId = user.Id,
                VoucherId = voucher.Id,
                Quantity = 2,
                RecipientName = "John",
                RecipientPhone = "598123456",
                RecipientCity = "Tbilisi",
                RecipientAddress = "Street 1"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(400, result.StatusCode);
            Assert.That(result.Message, Does.Contain("Not enough voucher quantity"));
        }

        [Test]
        public async Task Should_ReturnBadRequest_WhenUserBalanceIsInsufficient()
        {
            var user = CreateUser(balance: 30);
            var voucher = CreateVoucher(quantity: 5);

            _db.Users.Add(user);
            _db.Vouchers.Add(voucher);
            await _db.SaveChangesAsync();

            var command = new BuyVoucherCommand
            {
                UserId = user.Id,
                VoucherId = voucher.Id,
                Quantity = 1,
                RecipientName = "John",
                RecipientPhone = "598123456",
                RecipientCity = "Tbilisi",
                RecipientAddress = "Street 1"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(400, result.StatusCode);
            Assert.That(result.Message, Does.Contain("Insufficient balance"));
        }

        [Test]
        public async Task Should_ReturnSuccess_WhenVoucherIsBoughtSuccessfully()
        {
            var user = CreateUser(balance: 100);
            var voucher = CreateVoucher(quantity: 5);

            await _db.Users.AddAsync(user);
            await _db.Vouchers.AddAsync(voucher);
            await _db.SaveChangesAsync();

            var command = new BuyVoucherCommand
            {
                UserId = user.Id,
                VoucherId = voucher.Id,
                Quantity = 1,
                RecipientName = "John",
                RecipientPhone = "598123456",
                RecipientCity = "Tbilisi",
                RecipientAddress = "Street 1"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(200, result.StatusCode);
            Assert.That(result.Message, Does.Contain("Voucher purchased successfully"));

            var updatedUser = await _db.Users.FindAsync(user.Id);
            var updatedVoucher = await _db.Vouchers.FindAsync(voucher.Id);

            Assert.AreEqual(50, updatedUser.Balance);
            Assert.AreEqual(4, updatedVoucher.Quantity);
        }
    }
}