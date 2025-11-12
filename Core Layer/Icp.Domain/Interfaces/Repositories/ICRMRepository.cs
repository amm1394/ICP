using Core.Icp.Domain.Entities.CRM;

namespace Core.Icp.Domain.Interfaces.Repositories
{
    public interface ICRMRepository : IRepository<CRM>
    {
        Task<IEnumerable<CRM>> GetActiveCRMsAsync(CancellationToken cancellationToken = default);
        Task<CRM?> GetByCRMIdAsync(string crmId, CancellationToken cancellationToken = default);
        Task<CRM?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<CRM?> GetWithCertifiedValuesAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<CRM>> GetByManufacturerAsync(string manufacturer, CancellationToken cancellationToken = default);
        Task<IEnumerable<CRM>> GetExpiringCRMsAsync(DateTime beforeDate, CancellationToken cancellationToken = default);
    }
}