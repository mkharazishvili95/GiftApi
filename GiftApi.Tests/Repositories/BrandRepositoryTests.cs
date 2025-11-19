using FluentAssertions;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using GiftApi.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Repositories
{
    [TestFixture]
    public class BrandRepositoryTests
    {
        ApplicationDbContext _db = null!;
        BrandRepository _repo = null!;
        int _brandId;
        int _categoryId;

        [SetUp]
        public async Task SetUp()
        {
            _db = DbContextHelper.GetInMemoryDbContext();
            _repo = new BrandRepository(_db);

            var category = new GiftApi.Domain.Entities.Category
            {
                Name = "Category A",
                CreateDate = DateTime.UtcNow,
                IsDeleted = false
            };
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();
            _categoryId = category.Id;

            var brand = new GiftApi.Domain.Entities.Brand
            {
                Name = "Brand One",
                Description = "Desc",
                Website = "https://example.com",
                LogoUrl = "logo.png",
                CategoryId = _categoryId,
                CreateDate = DateTime.UtcNow,
                IsDeleted = false
            };
            _db.Brands.Add(brand);
            await _db.SaveChangesAsync();
            _brandId = brand.Id;
        }

        [TearDown]
        public void TearDown()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task BrandExists_Should_Return_True_For_Existing()
        {
            (await _repo.BrandExists(_brandId)).Should().BeTrue();
        }

        [Test]
        public async Task BrandExists_Should_Return_False_For_Deleted()
        {
            var brand = await _db.Brands.FindAsync(_brandId);
            brand!.IsDeleted = true;
            _db.Brands.Update(brand);
            await _db.SaveChangesAsync();
            (await _repo.BrandExists(_brandId)).Should().BeFalse();
        }

        [Test]
        public async Task BrandExists_Should_Return_False_For_Missing()
        {
            (await _repo.BrandExists(9999)).Should().BeFalse();
        }

        [Test]
        public async Task Get_Should_Return_Brand()
        {
            var b = await _repo.Get(_brandId);
            b.Should().NotBeNull();
            b!.Name.Should().Be("Brand One");
        }

        [Test]
        public async Task Get_Should_Return_Null_For_Missing()
        {
            (await _repo.Get(8888)).Should().BeNull();
        }

        [Test]
        public void GetQueryable_Should_Return_All()
        {
            var q = _repo.GetQueryable();
            q.Count().Should().Be(1);
        }

        [Test]
        public async Task Delete_Should_Mark_Deleted()
        {
            var tracked = await _db.Brands.FindAsync(_brandId);
            _db.Entry(tracked!).State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            var ok = await _repo.Delete(_brandId);
            ok.Should().BeTrue();
            await _db.SaveChangesAsync();

            var b = await _db.Brands.FindAsync(_brandId);
            b!.IsDeleted.Should().BeTrue();
            b.DeleteDate.Should().NotBeNull();
        }

        [Test]
        public async Task Delete_Should_Return_False_For_Missing()
        {
            (await _repo.Delete(5555)).Should().BeFalse();
        }

        [Test]
        public async Task Delete_Should_Return_False_For_Already_Deleted()
        {
            var brand = await _db.Brands.FindAsync(_brandId);
            brand!.IsDeleted = true;
            _db.Update(brand);
            await _db.SaveChangesAsync();
            (await _repo.Delete(_brandId)).Should().BeFalse();
        }

        [Test]
        public async Task Restore_Should_Work()
        {
            var brand = await _db.Brands.FindAsync(_brandId);
            brand!.IsDeleted = true;
            brand.DeleteDate = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            _db.Entry(brand).State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            var ok = await _repo.Restore(_brandId);
            ok.Should().BeTrue();
            await _db.SaveChangesAsync();

            var restored = await _db.Brands.FindAsync(_brandId);
            restored!.IsDeleted.Should().BeFalse();
            restored.DeleteDate.Should().BeNull();
            restored.UpdateDate.Should().NotBeNull();
        }

        [Test]
        public async Task Restore_Should_Return_False_When_Not_Deleted()
        {
            (await _repo.Restore(_brandId)).Should().BeFalse();
        }

        [Test]
        public async Task Restore_Should_Return_False_When_Missing()
        {
            (await _repo.Restore(4444)).Should().BeFalse();
        }

        [Test]
        public async Task Update_Should_Modify_Fields()
        {
            var existing = await _db.Brands.FindAsync(_brandId);
            existing!.Name = "Changed";
            existing.Description = "Updated Desc";
            existing.Website = "https://changed.example.com";
            existing.LogoUrl = "newlogo.png";
            existing.CategoryId = _categoryId;
            var updated = await _repo.Update(existing);
            updated.Should().NotBeNull();
            await _db.SaveChangesAsync();
            var tracked = await _db.Brands.FindAsync(_brandId);
            tracked!.Name.Should().Be("Changed");
            tracked.Description.Should().Be("Updated Desc");
            tracked.Website.Should().Be("https://changed.example.com");
            tracked.LogoUrl.Should().Be("newlogo.png");
            tracked.UpdateDate.Should().NotBeNull();
        }

        [Test]
        public async Task Update_Should_Return_Null_For_Missing()
        {
            var ghost = new GiftApi.Domain.Entities.Brand { Id = 9999, Name = "Ghost", CategoryId = _categoryId };
            (await _repo.Update(ghost)).Should().BeNull();
        }

        [Test]
        public async Task GetWithCategory_Should_Include_Category()
        {
            var b = await _repo.GetWithCategory(_brandId);
            b.Should().NotBeNull();
            b!.Category.Should().NotBeNull();
            b.Category.Id.Should().Be(_categoryId);
        }

        [Test]
        public async Task GetAllBrandsAsync_Should_Return_List()
        {
            var list = await _repo.GetAllBrandsAsync(CancellationToken.None);
            list.Should().NotBeNull();
            list!.Count.Should().Be(1);
        }

        [Test]
        public async Task GetAllBrandsWithCategoriesAsync_Should_Return_With_Categories()
        {
            var list = await _repo.GetAllBrandsWithCategoriesAsync(CancellationToken.None);
            list.Should().NotBeNull();
            list!.Count.Should().Be(1);
            list.First().Category.Should().NotBeNull();
        }

        [Test]
        public async Task GetBrandDtoByIdAsync_Should_Return_Dto()
        {
            var dto = await _repo.GetBrandDtoByIdAsync(_brandId, CancellationToken.None);
            dto.Should().NotBeNull();
            dto!.Id.Should().Be(_brandId);
            dto.Name.Should().Be("Brand One");
        }

        [Test]
        public async Task GetBrandDtoByIdAsync_Should_Return_Null_For_Missing()
        {
            (await _repo.GetBrandDtoByIdAsync(12345, CancellationToken.None)).Should().BeNull();
        }

        [Test]
        public async Task Create_Should_Return_New_Instance_With_Defaults()
        {
            var input = new GiftApi.Domain.Entities.Brand
            {
                Name = "New Brand",
                Description = "New Desc",
                Website = "https://new.example.com",
                LogoUrl = "new.png",
                CategoryId = _categoryId
            };
            var result = await _repo.Create(input);
            result.Should().NotBeNull();
            result!.Name.Should().Be("New Brand");
            result.IsDeleted.Should().BeFalse();
            result.CreateDate.Should().BeCloseTo(DateTime.UtcNow.AddHours(4), TimeSpan.FromMinutes(1));
            await _db.SaveChangesAsync();
            (await _db.Brands.CountAsync()).Should().Be(2);
        }
    }
}