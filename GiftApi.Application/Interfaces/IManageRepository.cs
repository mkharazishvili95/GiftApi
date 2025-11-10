namespace GiftApi.Application.Interfaces
{
    public interface IManageRepository
    {
        Task<bool> ChangeUsedStatus(Guid id);
    }
}
