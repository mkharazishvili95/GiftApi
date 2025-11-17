using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Manage.Voucher.Commands.BulkUpsert
{
    public class BulkUpsertVouchersHandler : IRequestHandler<BulkUpsertVouchersCommand, BulkUpsertVouchersResponse>
    {
        readonly IVoucherRepository _voucherRepository;
        readonly IBrandRepository _brandRepository;
        readonly IUnitOfWork _unitOfWork;

        public BulkUpsertVouchersHandler(IVoucherRepository voucherRepository, IBrandRepository brandRepository, IUnitOfWork unitOfWork)
        {
            _voucherRepository = voucherRepository;
            _brandRepository = brandRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<BulkUpsertVouchersResponse> Handle(BulkUpsertVouchersCommand request, CancellationToken cancellationToken)
        {
            var response = new BulkUpsertVouchersResponse();
            if (request.Items == null || request.Items.Count == 0)
            {
                response.Success = false;
                response.StatusCode = 400;
                response.Message = "No items provided.";
                return response;
            }

            var updateIds = request.Items.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).Distinct().ToList();
            var existingMap = updateIds.Count > 0
                ? (await _voucherRepository.GetByIdsAsync(updateIds))
                      .ToDictionary(v => v.Id, v => v)
                : new Dictionary<Guid, Domain.Entities.Voucher>();

            var now = DateTime.UtcNow.AddHours(4);

            var toCreate = new List<Domain.Entities.Voucher>();
            var toUpdate = new List<Domain.Entities.Voucher>();

            foreach (var item in request.Items)
            {
                var result = new BulkVoucherItemResult
                {
                    InputId = item.Id,
                    Title = item.Title
                };

                if (string.IsNullOrWhiteSpace(item.Title))
                {
                    result.Error = "Title is required.";
                    result.Success = false;
                    response.Results.Add(result);
                    continue;
                }
                if (string.IsNullOrWhiteSpace(item.Description))
                {
                    result.Error = "Description is required.";
                    result.Success = false;
                    response.Results.Add(result);
                    continue;
                }
                if (item.Amount <= 0 && !item.IsPercentage)
                {
                    result.Error = "Non-percentage vouchers must have positive Amount.";
                    result.Success = false;
                    response.Results.Add(result);
                    continue;
                }
                if (item.ValidMonths < 0)
                {
                    result.Error = "ValidMonths must be >= 0.";
                    result.Success = false;
                    response.Results.Add(result);
                    continue;
                }

                if (item.BrandId.HasValue)
                {
                    var brandExists = await _brandRepository.BrandExists(item.BrandId.Value);
                    if (!brandExists)
                    {
                        result.Error = $"Brand with Id {item.BrandId.Value} does not exist or is deleted.";
                        result.Success = false;
                        response.Results.Add(result);
                        continue;
                    }
                }

                if (!item.Id.HasValue)
                {
                    var entity = new Domain.Entities.Voucher
                    {
                        Id = Guid.NewGuid(),
                        Title = item.Title,
                        Description = item.Description,
                        Amount = item.Amount,
                        IsPercentage = item.IsPercentage,
                        BrandId = item.BrandId,
                        ValidMonths = item.ValidMonths,
                        Unlimited = item.Unlimited,
                        Quantity = item.Quantity,
                        Redeemed = item.Redeemed,
                        IsActive = item.IsActive,
                        CreateDate = now,
                        UpdateDate = null,
                        ImageUrl = item.ImageUrl,
                        IsDeleted = false
                    };
                    toCreate.Add(entity);

                    result.Created = true;
                    result.Success = true;
                    result.FinalId = entity.Id;
                }
                else
                {
                    if (!existingMap.TryGetValue(item.Id.Value, out var entity))
                    {
                        result.Error = $"Voucher with Id {item.Id.Value} not found.";
                        result.Success = false;
                        response.Results.Add(result);
                        continue;
                    }

                    entity.Title = item.Title;
                    entity.Description = item.Description;
                    entity.Amount = item.Amount;
                    entity.IsPercentage = item.IsPercentage;
                    entity.BrandId = item.BrandId;
                    entity.ValidMonths = item.ValidMonths;
                    entity.Unlimited = item.Unlimited;
                    entity.Quantity = item.Quantity;
                    entity.Redeemed = item.Redeemed;
                    entity.IsActive = item.IsActive;
                    entity.UpdateDate = now;
                    entity.ImageUrl = item.ImageUrl;

                    toUpdate.Add(entity);

                    result.Updated = true;
                    result.Success = true;
                    result.FinalId = entity.Id;
                }

                response.Results.Add(result);
            }

            if (toCreate.Count == 0 && toUpdate.Count == 0)
            {
                response.Success = false;
                response.StatusCode = 400;
                response.Message = "All items failed validation.";
                return response;
            }

            if (toCreate.Count > 0)
                await _voucherRepository.AddRangeAsync(toCreate);
            if (toUpdate.Count > 0)
                await _voucherRepository.UpdateRangeAsync(toUpdate);

            await _unitOfWork.SaveChangesAsync();

            response.Success = response.Results.All(r => r.Success);
            response.StatusCode = 200;
            response.Message = $"Bulk operation finished. Created: {response.CreatedCount}, Updated: {response.UpdatedCount}, Failed: {response.FailedCount}.";
            return response;
        }
    }
}