using FluentAssertions;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using GiftApi.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Repositories
{
    [TestFixture]
    public class VoucherRepositoryTests
    {
        ApplicationDbContext _db = null!;
        VoucherRepository _repo = null!;
        Guid _voucherId;
        Guid _userId;
        int _brandId;
        int _categoryId;

        [SetUp]
        public async Task SetUp()
        {
            _db = DbContextHelper.GetInMemoryDbContext();
            _repo = new VoucherRepository(_db);

            var category = new GiftApi.Domain.Entities.Category
            {
                Name = "Cat",
                CreateDate = DateTime.UtcNow,
                IsDeleted = false
            };
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();
            _categoryId = category.Id;

            var brand = new GiftApi.Domain.Entities.Brand
            {
                Name = "Brand",
                CategoryId = _categoryId,
                CreateDate = DateTime.UtcNow,
                IsDeleted = false
            };
            _db.Brands.Add(brand);
            await _db.SaveChangesAsync();
            _brandId = brand.Id;

            var voucher = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "Voucher 1",
                Description = "Desc",
                Amount = 50,
                IsPercentage = false,
                BrandId = _brandId,
                ValidMonths = 2,
                Unlimited = false,
                Quantity = 10,
                Redeemed = 0,
                IsActive = true,
                CreateDate = DateTime.UtcNow,
                IsDeleted = false,
                SoldCount = 0
            };
            _db.Vouchers.Add(voucher);
            await _db.SaveChangesAsync();
            _voucherId = voucher.Id;

            var user = new GiftApi.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                FirstName = "U",
                LastName = "L",
                DateOfBirth = DateTime.UtcNow.AddYears(-20),
                IdentificationNumber = "12345678901",
                Email = "u@example.com",
                PhoneNumber = "+995599000001",
                UserName = "user",
                Password = "hash",
                RegisterDate = DateTime.UtcNow,
                Balance = 0,
                Type = GiftApi.Domain.Enums.User.UserType.User,
                EmailVerified = true
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            _userId = user.Id;
        }

        [TearDown]
        public void TearDown()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task GetByIdAsync_Should_Return_Voucher()
        {
            var v = await _repo.GetByIdAsync(_voucherId);
            v.Should().NotBeNull();
            v!.Title.Should().Be("Voucher 1");
        }

        [Test]
        public async Task GetByIdAsync_Should_Return_Null_For_Missing()
        {
            (await _repo.GetByIdAsync(Guid.NewGuid())).Should().BeNull();
        }

        [Test]
        public async Task GetWithCategoryAndBrand_Should_Include_Relations()
        {
            var v = await _repo.GetWithCategoryAndBrand(_voucherId);
            v.Should().NotBeNull();
            v!.Brand.Should().NotBeNull();
            v.Brand.Category.Should().NotBeNull();
        }

        [Test]
        public async Task GetAllWithCategoryAndBrand_Should_Filter_Active_NotDeleted()
        {
            var inactive = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "Voucher 2",
                Description = "Desc",
                Amount = 60,
                IsPercentage = false,
                BrandId = _brandId,
                ValidMonths = 1,
                Unlimited = false,
                Quantity = 5,
                Redeemed = 0,
                IsActive = false,
                CreateDate = DateTime.UtcNow,
                IsDeleted = false
            };
            var deleted = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "Voucher 3",
                Description = "Desc",
                Amount = 70,
                IsPercentage = false,
                BrandId = _brandId,
                ValidMonths = 1,
                Unlimited = false,
                Quantity = 5,
                Redeemed = 0,
                IsActive = true,
                CreateDate = DateTime.UtcNow,
                IsDeleted = true
            };
            _db.Vouchers.AddRange(inactive, deleted);
            await _db.SaveChangesAsync();

            var list = await _repo.GetAllWithCategoryAndBrand();
            list.Should().NotBeNull();
            list!.Count.Should().Be(1);
            list.First().Title.Should().Be("Voucher 1");
        }

        [Test]
        public async Task Buy_Should_Create_DeliveryInfo_And_Update_Voucher()
        {
            var ok = await _repo.Buy(_voucherId, _userId, 3, "Rec", "555", "City", "Addr");
            ok.Should().BeTrue();
            await _db.SaveChangesAsync();

            var v = await _db.Vouchers.FindAsync(_voucherId);
            v!.Quantity.Should().Be(7);
            v.SoldCount.Should().Be(3);

            (await _db.VoucherDeliveryInfos.CountAsync()).Should().Be(1);
            var delivery = await _db.VoucherDeliveryInfos.FirstAsync();
            delivery.Quantity.Should().Be(3);
            delivery.IsUsed.Should().BeFalse();
        }

        [Test]
        public async Task Buy_Should_Return_False_When_User_Not_Found()
        {
            (await _repo.Buy(_voucherId, Guid.NewGuid(), 1, "R", "P", "C", "A")).Should().BeFalse();
        }

        [Test]
        public async Task Buy_Should_Return_False_When_Voucher_Not_Found()
        {
            (await _repo.Buy(Guid.NewGuid(), _userId, 1, "R", "P", "C", "A")).Should().BeFalse();
        }

        [Test]
        public async Task Buy_Should_Return_False_When_Insufficient_Quantity()
        {
            (await _repo.Buy(_voucherId, _userId, 999, "R", "P", "C", "A")).Should().BeFalse();
        }

        [Test]
        public async Task GetDeliveryInfoByIdAsync_Should_Return_Entity()
        {
            await _repo.Buy(_voucherId, _userId, 2, "Rec2", "555", "City", "Addr");
            await _db.SaveChangesAsync();
            var entity = await _db.VoucherDeliveryInfos.FirstAsync();
            var fetched = await _repo.GetDeliveryInfoByIdAsync(entity.Id);
            fetched.Should().NotBeNull();
            fetched!.Quantity.Should().Be(2);
        }

        [Test]
        public async Task GetDeliveryInfosBySenderAsync_Should_Paginate_And_Filter()
        {
            await _repo.Buy(_voucherId, _userId, 1, "R1", "P", "C", "A");
            await _repo.Buy(_voucherId, _userId, 1, "R2", "P", "C", "A");
            await _repo.Buy(_voucherId, _userId, 1, "R3", "P", "C", "A");
            await _db.SaveChangesAsync();

            var page1 = await _repo.GetDeliveryInfosBySenderAsync(_userId, includeVoucher: true, isUsedFilter: null, page:1, pageSize:2);
            var page2 = await _repo.GetDeliveryInfosBySenderAsync(_userId, includeVoucher: true, isUsedFilter: null, page:2, pageSize:2);

            page1.Count.Should().Be(2);
            page2.Count.Should().Be(1);

            page1.All(x => x.Voucher != null).Should().BeTrue();
        }

        [Test]
        public async Task Delete_Should_SoftDelete()
        {
            var ok = await _repo.Delete(_voucherId);
            ok.Should().BeTrue();
            await _db.SaveChangesAsync();
            var v = await _db.Vouchers.FindAsync(_voucherId);
            v!.IsDeleted.Should().BeTrue();
            v.IsActive.Should().BeFalse();
            v.DeleteDate.Should().NotBeNull();
        }

        [Test]
        public async Task Delete_Should_Return_False_For_Missing()
        {
            (await _repo.Delete(Guid.NewGuid())).Should().BeFalse();
        }

        [Test]
        public async Task Delete_Should_Return_False_For_Already_Deleted()
        {
            var v = await _db.Vouchers.FindAsync(_voucherId);
            v!.IsDeleted = true;
            _db.Update(v);
            await _db.SaveChangesAsync();
            (await _repo.Delete(_voucherId)).Should().BeFalse();
        }

        [Test]
        public async Task Restore_Should_Work()
        {
            var v = await _db.Vouchers.FindAsync(_voucherId);
            v!.IsDeleted = true;
            _db.Update(v);
            await _db.SaveChangesAsync();

            var ok = await _repo.Restore(_voucherId);
            ok.Should().BeTrue();
            await _db.SaveChangesAsync();

            var restored = await _db.Vouchers.FindAsync(_voucherId);
            restored!.IsDeleted.Should().BeFalse();
            restored.DeleteDate.Should().BeNull();
            restored.UpdateDate.Should().NotBeNull();
        }

        [Test]
        public async Task Restore_Should_Return_False_When_Not_Deleted()
        {
            (await _repo.Restore(_voucherId)).Should().BeFalse();
        }

        [Test]
        public async Task Restore_Should_Return_False_For_Missing()
        {
            (await _repo.Restore(Guid.NewGuid())).Should().BeFalse();
        }

        [Test]
        public async Task SearchAsync_Should_Filter_By_Parameters()
        {
            var v2 = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "Voucher 2",
                Description = "More",
                Amount = 75,
                IsPercentage = false,
                BrandId = _brandId,
                ValidMonths = 1,
                Unlimited = false,
                Quantity = 5,
                Redeemed = 0,
                IsActive = true,
                CreateDate = DateTime.UtcNow,
                IsDeleted = false
            };
            _db.Vouchers.Add(v2);
            await _db.SaveChangesAsync();

            var results = await _repo.SearchAsync(_brandId, _categoryId, 40, 80, "Voucher");
            results.Should().HaveCountGreaterThan(1);
            results.All(r => r.Amount >= 40 && r.Amount <= 80).Should().BeTrue();
            results.Select(r => r.Title).Should().Contain(new[] { "Voucher 1", "Voucher 2" });
        }

        [Test]
        public async Task SearchAsync_Should_Ignore_Deleted_And_Inactive()
        {
            var inactive = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "Voucher Inactive",
                Description = "Desc",
                Amount = 30,
                IsPercentage = false,
                BrandId = _brandId,
                ValidMonths = 1,
                Unlimited = false,
                Quantity = 5,
                Redeemed = 0,
                IsActive = false,
                CreateDate = DateTime.UtcNow,
                IsDeleted = false
            };
            var deleted = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "Voucher Deleted",
                Description = "Desc",
                Amount = 30,
                IsPercentage = false,
                BrandId = _brandId,
                ValidMonths = 1,
                Unlimited = false,
                Quantity = 5,
                Redeemed = 0,
                IsActive = true,
                CreateDate = DateTime.UtcNow,
                IsDeleted = true
            };
            _db.Vouchers.AddRange(inactive, deleted);
            await _db.SaveChangesAsync();

            var results = await _repo.SearchAsync(null, null, null, null, null);
            results.Any(r => r.Title.Contains("Inactive") || r.Title.Contains("Deleted")).Should().BeFalse();
        }

        [Test]
        public async Task GetByIdsAsync_Should_Return_Selected()
        {
            var ids = new[] { _voucherId };
            var list = await _repo.GetByIdsAsync(ids);
            list.Count.Should().Be(1);
            list.First().Id.Should().Be(_voucherId);
        }

        [Test]
        public async Task AddRangeAsync_Should_Add()
        {
            var vNew = new[]
            {
                new GiftApi.Domain.Entities.Voucher
                {
                    Id = Guid.NewGuid(),
                    Title = "Voucher 10",
                    Description = "Desc",
                    Amount = 10,
                    IsPercentage = false,
                    BrandId = _brandId,
                    ValidMonths = 1,
                    Unlimited = false,
                    Quantity = 3,
                    Redeemed = 0,
                    IsActive = true,
                    CreateDate = DateTime.UtcNow,
                    IsDeleted = false
                }
            };
            await _repo.AddRangeAsync(vNew);
            await _db.SaveChangesAsync();
            (await _db.Vouchers.CountAsync()).Should().BeGreaterThan(1);
        }

        [Test]
        public async Task UpdateRangeAsync_Should_Update()
        {
            var all = await _db.Vouchers.ToListAsync();
            foreach (var v in all)
                v.Description = "Updated";
            await _repo.UpdateRangeAsync(all);
            await _db.SaveChangesAsync();
            var again = await _db.Vouchers.ToListAsync();
            again.All(v => v.Description == "Updated").Should().BeTrue();
        }
    }
}