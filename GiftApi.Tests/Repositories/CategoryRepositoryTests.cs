using FluentAssertions;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using GiftApi.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Repositories
{
    [TestFixture]
    public class CategoryRepositoryTests
    {
        ApplicationDbContext _db = null!;
        CategoryRepository _repo = null!;
        int _categoryId;

        [SetUp]
        public async Task SetUp()
        {
            _db = DbContextHelper.GetInMemoryDbContext();
            _repo = new CategoryRepository(_db);

            var category = new GiftApi.Domain.Entities.Category
            {
                Name = "Cat One",
                Description = "Desc",
                CreateDate = DateTime.UtcNow,
                IsDeleted = false
            };
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();
            _categoryId = category.Id;

            var brand1 = new GiftApi.Domain.Entities.Brand
            {
                Name = "Brand A",
                CategoryId = _categoryId,
                CreateDate = DateTime.UtcNow,
                IsDeleted = false
            };
            var brand2 = new GiftApi.Domain.Entities.Brand
            {
                Name = "Brand B",
                CategoryId = _categoryId,
                CreateDate = DateTime.UtcNow,
                IsDeleted = true
            };
            _db.Brands.AddRange(brand1, brand2);
            await _db.SaveChangesAsync();
        }

        [TearDown]
        public void TearDown()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task CategoryExists_Should_Return_True_For_Existing()
        {
            (await _repo.CategoryExists(_categoryId)).Should().BeTrue();
        }

        [Test]
        public async Task CategoryExists_Should_Return_False_For_Deleted()
        {
            var cat = await _db.Categories.FindAsync(_categoryId);
            cat!.IsDeleted = true;
            _db.Update(cat);
            await _db.SaveChangesAsync();
            (await _repo.CategoryExists(_categoryId)).Should().BeFalse();
        }

        [Test]
        public async Task CategoryExists_Should_Return_False_For_Missing()
        {
            (await _repo.CategoryExists(9999)).Should().BeFalse();
        }

        [Test]
        public async Task Get_Should_Return_Category()
        {
            var c = await _repo.Get(_categoryId);
            c.Should().NotBeNull();
            c!.Name.Should().Be("Cat One");
        }

        [Test]
        public async Task Get_Should_Return_Null_For_Missing()
        {
            (await _repo.Get(8888)).Should().BeNull();
        }

        [Test]
        public async Task Delete_Should_Mark_Deleted()
        {
            var tracked = await _db.Categories.FindAsync(_categoryId);
            _db.Entry(tracked!).State = EntityState.Detached;

            var ok = await _repo.Delete(_categoryId);
            ok.Should().BeTrue();
            await _db.SaveChangesAsync();

            var c = await _db.Categories.FindAsync(_categoryId);
            c!.IsDeleted.Should().BeTrue();
            c.DeleteDate.Should().NotBeNull();
        }

        [Test]
        public async Task Delete_Should_Return_False_For_Missing()
        {
            (await _repo.Delete(5555)).Should().BeFalse();
        }

        [Test]
        public async Task Delete_Should_Return_False_For_Already_Deleted()
        {
            var cat = await _db.Categories.FindAsync(_categoryId);
            cat!.IsDeleted = true;
            _db.Update(cat);
            await _db.SaveChangesAsync();
            (await _repo.Delete(_categoryId)).Should().BeFalse();
        }

        [Test]
        public async Task Restore_Should_Work()
        {
            var cat = await _db.Categories.FindAsync(_categoryId);
            cat!.IsDeleted = true;
            cat.DeleteDate = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            _db.Entry(cat).State = EntityState.Detached;

            var ok = await _repo.Restore(_categoryId);
            ok.Should().BeTrue();
            await _db.SaveChangesAsync();

            var restored = await _db.Categories.FindAsync(_categoryId);
            restored!.IsDeleted.Should().BeFalse();
            restored.DeleteDate.Should().BeNull();
            restored.UpdateDate.Should().NotBeNull();
        }

        [Test]
        public async Task Restore_Should_Return_False_When_Not_Deleted()
        {
            (await _repo.Restore(_categoryId)).Should().BeFalse();
        }

        [Test]
        public async Task Restore_Should_Return_False_When_Missing()
        {
            (await _repo.Restore(4444)).Should().BeFalse();
        }

        [Test]
        public async Task Edit_Should_Modify_Fields()
        {
            var existing = await _db.Categories.FindAsync(_categoryId);
            existing!.Name = "Changed";
            existing.Description = "Updated Desc";
            existing.Logo = "logo.png";
            var edited = await _repo.Edit(existing);
            edited.Should().NotBeNull();
            await _db.SaveChangesAsync();
            var tracked = await _db.Categories.FindAsync(_categoryId);
            tracked!.Name.Should().Be("Changed");
            tracked.Description.Should().Be("Updated Desc");
            tracked.Logo.Should().Be("logo.png");
            tracked.UpdateDate.Should().NotBeNull();
        }

        [Test]
        public async Task Edit_Should_Return_Null_For_Missing()
        {
            var ghost = new GiftApi.Domain.Entities.Category { Id = 9999, Name = "Ghost", CreateDate = DateTime.UtcNow };
            (await _repo.Edit(ghost)).Should().BeNull();
        }

        [Test]
        public async Task Edit_Should_Return_Null_For_Deleted()
        {
            var cat = await _db.Categories.FindAsync(_categoryId);
            cat!.IsDeleted = true;
            _db.Update(cat);
            await _db.SaveChangesAsync();
            (await _repo.Edit(cat)).Should().BeNull();
        }

        [Test]
        public async Task GetWithBrands_Should_Include_Only_NotDeleted_Brands()
        {
            var c = await _repo.GetWithBrands(_categoryId);
            c.Should().NotBeNull();
            c!.Brands.Count(b => !b.IsDeleted).Should().Be(1);
            c.Brands.Where(b => !b.IsDeleted).Select(b => b.Name).Should().Contain("Brand A");
        }

        [Test]
        public async Task GetWithBrands_Should_Return_Null_For_Deleted_Category()
        {
            var cat = await _db.Categories.FindAsync(_categoryId);
            cat!.IsDeleted = true;
            _db.Update(cat);
            await _db.SaveChangesAsync();
            (await _repo.GetWithBrands(_categoryId)).Should().BeNull();
        }

        [Test]
        public async Task GetAllCategoriesWithBrandsAsync_Should_Return_List_Filtered()
        {
            var list = await _repo.GetAllCategoriesWithBrandsAsync(CancellationToken.None);
            list.Should().NotBeNull();
            list!.Count.Should().Be(1);
            var first = list.First();
            first.Brands.Count(b => !b.IsDeleted).Should().Be(1);
            first.Brands.Where(b => !b.IsDeleted).Select(b => b.Name).Should().Contain("Brand A");
        }

        [Test]
        public async Task GetAll_Should_Return_Active_Categories()
        {
            var list = await _repo.GetAll(CancellationToken.None);
            list.Should().NotBeNull();
            list!.Count.Should().Be(1);
        }

        [Test]
        public async Task Create_Should_Return_New_Instance_With_Defaults()
        {
            var input = new GiftApi.Domain.Entities.Category
            {
                Name = "New Category",
                Description = "New Desc",
                Logo = "logo2.png"
            };
            var result = await _repo.Create(input);
            result.Should().NotBeNull();
            result!.Name.Should().Be("New Category");
            result.IsDeleted.Should().BeFalse();
            result.CreateDate.Should().BeCloseTo(DateTime.UtcNow.AddHours(4), TimeSpan.FromMinutes(1));
            await _db.SaveChangesAsync();
            (await _db.Categories.CountAsync()).Should().Be(2);
        }
    }
}