using GiftApi.Application.Features.Manage.Voucher.Commands.BulkUpsert;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Voucher
{
    [TestFixture]
    public class BulkUpsertVouchersHandlerTests
    {
        ApplicationDbContext _db;
        IVoucherRepository _voucherRepo;
        IBrandRepository _brandRepo;
        IUnitOfWork _uow;
        BulkUpsertVouchersHandler _handler;

        [SetUp]
        public void Setup()
        {
            var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(opts);
            _voucherRepo = new VoucherRepository(_db);
            _brandRepo = new BrandRepository(_db);
            _uow = new UnitOfWork(_db);
            _handler = new BulkUpsertVouchersHandler(_voucherRepo, _brandRepo, _uow);
        }

        [TearDown]
        public void Cleanup()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task Empty_Items_Should_Return_400()
        {
            var cmd = new BulkUpsertVouchersCommand { Items = new List<BulkUpsertVouchersItemsResponse>() };
            var res = await _handler.Handle(cmd, CancellationToken.None);
            Assert.IsFalse(res.Success);
            Assert.AreEqual(400, res.StatusCode);
            Assert.That(res.Message, Does.Contain("No items"));
        }

        [Test]
        public async Task All_Invalid_Should_Return_400_All_Items_Failed()
        {
            var cmd = new BulkUpsertVouchersCommand
            {
                Items = new()
                {
                    new BulkUpsertVouchersItemsResponse { Title = "", Description = "Desc", Amount = 10, IsPercentage = false },
                    new BulkUpsertVouchersItemsResponse { Title = "Ok", Description = "", Amount = 10, IsPercentage = false },
                    new BulkUpsertVouchersItemsResponse { Title = "Bad Months", Description = "Desc", Amount = 10, IsPercentage = false, ValidMonths = -1 },
                    new BulkUpsertVouchersItemsResponse { Title = "Bad Amount", Description = "Desc", Amount = 0, IsPercentage = false }
                }
            };

            var res = await _handler.Handle(cmd, CancellationToken.None);

            Assert.IsFalse(res.Success);
            Assert.AreEqual(400, res.StatusCode);
            Assert.AreEqual(0, res.CreatedCount);
            Assert.AreEqual(0, res.UpdatedCount);
            Assert.AreEqual(4, res.FailedCount);
            Assert.That(res.Message, Does.Contain("All items failed"));
        }

        [Test]
        public async Task Should_Create_New_Vouchers()
        {
            _db.Brands.Add(new GiftApi.Domain.Entities.Brand { Id = 10, Name = "Brand10", IsDeleted = false });
            await _db.SaveChangesAsync();

            var cmd = new BulkUpsertVouchersCommand
            {
                Items = new()
                {
                    new BulkUpsertVouchersItemsResponse
                    {
                        Title = "Voucher A",
                        Description = "Desc A",
                        Amount = 50,
                        IsPercentage = false,
                        BrandId = 10,
                        Quantity = 100,
                        IsActive = true
                    },
                    new BulkUpsertVouchersItemsResponse
                    {
                        Title = "Voucher B",
                        Description = "Desc B",
                        Amount = 20,
                        IsPercentage = true,
                        ValidMonths = 3,
                        Unlimited = true,
                        Quantity = 0,
                        IsActive = false
                    }
                }
            };

            var res = await _handler.Handle(cmd, CancellationToken.None);

            Assert.AreEqual(200, res.StatusCode);
            Assert.IsTrue(res.Results.All(r => r.Success));
            Assert.IsTrue(res.Success);
            Assert.AreEqual(2, res.CreatedCount);
            Assert.AreEqual(0, res.UpdatedCount);
            Assert.AreEqual(0, res.FailedCount);

            Assert.AreEqual(2, _db.Vouchers.Count());
            var a = _db.Vouchers.First(v => v.Title == "Voucher A");
            Assert.AreEqual(10, a.BrandId);
            var b = _db.Vouchers.First(v => v.Title == "Voucher B");
            Assert.IsTrue(b.Unlimited);
            Assert.AreEqual(0, b.Quantity);
        }

        [Test]
        public async Task Should_Fail_When_Brand_Not_Exists_And_Create_Other_Succeeds()
        {
            _db.Brands.Add(new GiftApi.Domain.Entities.Brand { Id = 5, Name = "OkBrand", IsDeleted = false });
            await _db.SaveChangesAsync();

            var cmd = new BulkUpsertVouchersCommand
            {
                Items = new()
                {
                    new BulkUpsertVouchersItemsResponse
                    {
                        Title = "Good",
                        Description = "Desc",
                        Amount = 10,
                        IsPercentage = false,
                        BrandId = 5,
                        Quantity = 10,
                        IsActive = true
                    },
                    new BulkUpsertVouchersItemsResponse
                    {
                        Title = "BadBrand",
                        Description = "Desc",
                        Amount = 10,
                        IsPercentage = false,
                        BrandId = 999,
                        Quantity = 10,
                        IsActive = true
                    }
                }
            };

            var res = await _handler.Handle(cmd, CancellationToken.None);

            Assert.AreEqual(200, res.StatusCode); 
            Assert.IsFalse(res.Success);
            Assert.AreEqual(1, res.CreatedCount);
            Assert.AreEqual(0, res.UpdatedCount);
            Assert.AreEqual(1, res.FailedCount);
            Assert.That(res.Results.First(r => r.Title == "BadBrand").Error, Does.Contain("does not exist"));
        }

        [Test]
        public async Task Should_Update_Existing_Voucher()
        {
            var existing = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "Existing",
                Description = "Old",
                Amount = 15,
                IsPercentage = false,
                CreateDate = DateTime.UtcNow
            };
            _db.Vouchers.Add(existing);
            await _db.SaveChangesAsync();

            var cmd = new BulkUpsertVouchersCommand
            {
                Items = new()
                {
                    new BulkUpsertVouchersItemsResponse
                    {
                        Id = existing.Id,
                        Title = "Existing Updated",
                        Description = "New",
                        Amount = 30,
                        IsPercentage = false,
                        Quantity = 500,
                        IsActive = true
                    }
                }
            };

            var res = await _handler.Handle(cmd, CancellationToken.None);

            Assert.AreEqual(200, res.StatusCode);
            Assert.IsTrue(res.Results.Single().Updated);
            Assert.AreEqual(existing.Id, res.Results.Single().FinalId);
            Assert.AreEqual(0, res.CreatedCount);
            Assert.AreEqual(1, res.UpdatedCount);

            var updated = await _db.Vouchers.FirstAsync(v => v.Id == existing.Id);
            Assert.AreEqual("Existing Updated", updated.Title);
            Assert.AreEqual(30, updated.Amount);
            Assert.IsNotNull(updated.UpdateDate);
        }

        [Test]
        public async Task Update_With_NotFound_Id_Should_Fail_Item()
        {
            var cmd = new BulkUpsertVouchersCommand
            {
                Items = new()
                {
                    new BulkUpsertVouchersItemsResponse
                    {
                        Id = Guid.NewGuid(),
                        Title = "WillFail",
                        Description = "Desc",
                        Amount = 10,
                        IsPercentage = false,
                        Quantity = 1,
                        IsActive = true
                    },
                    new BulkUpsertVouchersItemsResponse
                    {
                        Title = "WillCreate",
                        Description = "Desc2",
                        Amount = 5,
                        IsPercentage = false,
                        Quantity = 2,
                        IsActive = true
                    }
                }
            };

            var res = await _handler.Handle(cmd, CancellationToken.None);

            Assert.AreEqual(200, res.StatusCode);
            Assert.IsFalse(res.Success); 
            Assert.AreEqual(1, res.CreatedCount);
            Assert.AreEqual(0, res.UpdatedCount);
            Assert.AreEqual(1, res.FailedCount);

            Assert.AreEqual(1, _db.Vouchers.Count(v => v.Title == "WillCreate"));
            Assert.AreEqual(0, _db.Vouchers.Count(v => v.Title == "WillFail"));
        }

        [Test]
        public async Task Mixed_Create_Update_And_Fail_Should_Report_Correct_Counts()
        {
            var existing = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "ToUpdate",
                Description = "Old",
                Amount = 10,
                IsPercentage = false,
                CreateDate = DateTime.UtcNow
            };
            _db.Vouchers.Add(existing);
            _db.Brands.Add(new GiftApi.Domain.Entities.Brand { Id = 22, Name = "Brand22", IsDeleted = false });
            await _db.SaveChangesAsync();

            var cmd = new BulkUpsertVouchersCommand
            {
                Items = new()
                {
                    new BulkUpsertVouchersItemsResponse { Title = "Create1", Description = "D1", Amount = 5, IsPercentage = false, BrandId = 22, Quantity = 10, IsActive = true },
                    
                    new BulkUpsertVouchersItemsResponse { Id = existing.Id, Title = "Updated Title", Description = "NewDesc", Amount = 99, IsPercentage = false, Quantity = 100, IsActive = true },
                   
                    new BulkUpsertVouchersItemsResponse { Title = "Invalid", Description = "D", Amount = 0, IsPercentage = false },
                }
            };

            var res = await _handler.Handle(cmd, CancellationToken.None);

            Assert.AreEqual(200, res.StatusCode);
            Assert.IsFalse(res.Success); 
            Assert.AreEqual(1, res.CreatedCount);
            Assert.AreEqual(1, res.UpdatedCount);
            Assert.AreEqual(1, res.FailedCount);

            var updated = await _db.Vouchers.FirstAsync(v => v.Id == existing.Id);
            Assert.AreEqual("Updated Title", updated.Title);
            Assert.AreEqual(99, updated.Amount);

            Assert.AreEqual(1, _db.Vouchers.Count(v => v.Title == "Create1"));
            Assert.AreEqual(0, _db.Vouchers.Count(v => v.Title == "Invalid"));
        }

        [Test]
        public async Task Duplicate_Update_Id_In_Payload_Should_Apply_Last_Values()
        {
            var existing = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "DupVoucher",
                Description = "Orig",
                Amount = 10,
                CreateDate = DateTime.UtcNow
            };
            _db.Vouchers.Add(existing);
            await _db.SaveChangesAsync();

            var cmd = new BulkUpsertVouchersCommand
            {
                Items = new()
                {
                    new BulkUpsertVouchersItemsResponse { Id = existing.Id, Title = "FirstUpdate", Description = "First", Amount = 20, IsPercentage = false },
                    new BulkUpsertVouchersItemsResponse { Id = existing.Id, Title = "SecondUpdate", Description = "Second", Amount = 40, IsPercentage = false },
                }
            };

            var res = await _handler.Handle(cmd, CancellationToken.None);

            Assert.AreEqual(200, res.StatusCode);
            Assert.IsTrue(res.Success);
            Assert.AreEqual(0, res.CreatedCount);
            Assert.AreEqual(2, res.UpdatedCount);
            Assert.AreEqual(0, res.FailedCount);

            var final = await _db.Vouchers.FirstAsync(v => v.Id == existing.Id);
            Assert.AreEqual("SecondUpdate", final.Title);
            Assert.AreEqual(40, final.Amount);
        }

        [Test]
        public async Task Brand_Deleted_Should_Fail_Item()
        {
            _db.Brands.Add(new GiftApi.Domain.Entities.Brand { Id = 77, Name = "DeletedBrand", IsDeleted = true });
            await _db.SaveChangesAsync();

            var cmd = new BulkUpsertVouchersCommand
            {
                Items = new()
                {
                    new BulkUpsertVouchersItemsResponse { Title = "BadBrandVoucher", Description = "Desc", Amount = 10, IsPercentage = false, BrandId = 77 }
                }
            };

            var res = await _handler.Handle(cmd, CancellationToken.None);
            Assert.AreEqual(400, res.StatusCode);
            Assert.IsFalse(res.Success);
            Assert.AreEqual(1, res.FailedCount);
            Assert.That(res.Results[0].Error, Does.Contain("does not exist"));
        }
    }
}