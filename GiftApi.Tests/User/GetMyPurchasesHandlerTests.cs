using GiftApi.Application.Common.Models;
using GiftApi.Application.Features.User.Queries.GetMyPurchases;
using GiftApi.Application.Interfaces;
using GiftApi.Domain.Entities;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GiftApi.Tests.User
{
    public class GetMyPurchasesHandlerTests
    {
        private ApplicationDbContext _db;
        private IVoucherRepository _voucherRepository;
        private TestCurrentUserRepository _currentUserRepository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);
            _voucherRepository = new VoucherRepository(_db);
            _currentUserRepository = new TestCurrentUserRepository();
        }

        [TearDown]
        public void TearDown()
        {
            _db.Dispose();
        }

        [Test]
        public async Task Handle_ReturnsUnauthorized_WhenUserNotLoggedIn()
        {
            var handler = new GetMyPurchasesHandler(_currentUserRepository, _voucherRepository);
            var query = new GetMyPurchasesQuery();

            var result = await handler.Handle(query, default);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(401, result.StatusCode);
        }

        [Test]
        public async Task Handle_ReturnsPurchases_WhenUserHasPurchases()
        {
            var userId = Guid.NewGuid();
            _currentUserRepository.SetUserId(userId);

            var category = new GiftApi.Domain.Entities.Category
            {
                Name = "Test Category",
                Description = "Desc",
                CreateDate = DateTime.UtcNow,
                IsDeleted = false
            };
            await _db.Categories.AddAsync(category);

            var brand = new GiftApi.Domain.Entities.Brand
            {
                Name = "Test Brand",
                Description = "Brand Desc",
                CreateDate = DateTime.UtcNow,
                CategoryId = category.Id,
                Category = category,
                IsDeleted = false
            };
            await _db.Brands.AddAsync(brand);

            var voucher = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "Test Voucher",
                Description = "Test Description",
                Amount = 50,
                IsPercentage = false,
                BrandId = brand.Id,
                Brand = brand,
                ValidMonths = 12,
                Unlimited = false,
                Quantity = 10,
                Redeemed = 0,
                SoldCount = 0,
                IsActive = true,
                CreateDate = DateTime.UtcNow,
                IsDeleted = false
            };
            await _db.Vouchers.AddAsync(voucher);

            var deliveryInfo = new GiftApi.Domain.Entities.VoucherDeliveryInfo
            {
                Id = Guid.NewGuid(),
                VoucherId = voucher.Id,
                Voucher = voucher,
                SenderId = userId,
                SenderName = "John Doe",
                RecipientName = "Jane Doe",
                RecipientEmail = "jane@example.com",
                RecipientPhone = "598123456",
                RecipientCity = "Tbilisi",
                RecipientAddress = "Rustaveli 1",
                Message = "Happy Birthday",
                Quantity = 1,
                IsUsed = false
            };
            await _db.VoucherDeliveryInfos.AddAsync(deliveryInfo);

            await _db.SaveChangesAsync();

            var handler = new GetMyPurchasesHandler(_currentUserRepository, _voucherRepository);
            var query = new GetMyPurchasesQuery
            {
                IncludeVoucher = true,
                IsUsed = false,
                Pagination = new PaginationModel { Page = 1, PageSize = 10 }
            };

            var result = await handler.Handle(query, default);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(1, result.Items.Count);
            Assert.AreEqual(deliveryInfo.SenderName, result.Items.First().SenderName);
            Assert.AreEqual(deliveryInfo.RecipientName, result.Items.First().RecipientName);
            Assert.IsNotNull(result.Items.First().Brand);
            Assert.IsNotNull(result.Items.First().Category);
        }
    }

    public class TestCurrentUserRepository : ICurrentUserRepository
    {
        private Guid? _userId;
        public void SetUserId(Guid id) => _userId = id;
        public Guid? GetUserId() => _userId;
        public ClaimsPrincipal? GetUser() => null;
    }
}