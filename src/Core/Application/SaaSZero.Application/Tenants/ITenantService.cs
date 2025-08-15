using System.Collections.Generic;
using System.Threading.Tasks;
using SaaSZero.Application.Tenants.DTOs;

namespace SaaSZero.Application.Tenants
{
    public interface ITenantService
    {
        Task<IReadOnlyList<TenantDto>> GetAllAsync();
        Task<TenantDto?> GetAsync(System.Guid id);
        Task<System.Guid> CreateAsync(CreateTenantRequest request);
        Task UpdateAsync(System.Guid id, UpdateTenantRequest request);
        Task DeleteAsync(System.Guid id);
    }
}