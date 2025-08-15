using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SaaSZero.Application.Tenants;
using SaaSZero.Application.Tenants.DTOs;
using SaaSZero.Domain.Tenancy;
using SaaSZero.Infrastructure.Persistence;

namespace SaaSZero.Infrastructure.Services
{
    public class TenantService : ITenantService
    {
        private readonly AppDbContext _dbContext;

        public TenantService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<TenantDto>> GetAllAsync()
        {
            var tenants = await _dbContext.Tenants.AsNoTracking().OrderBy(t => t.Name).ToListAsync();
            return tenants.Select(MapToDto).ToList();
        }

        public async Task<TenantDto?> GetAsync(Guid id)
        {
            var tenant = await _dbContext.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
            return tenant == null ? null : MapToDto(tenant);
        }

        public async Task<Guid> CreateAsync(CreateTenantRequest request)
        {
            var entity = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                DisplayName = request.DisplayName,
                Culture = request.Culture,
                ConnectionString = request.ConnectionString,
                IsActive = true
            };

            await _dbContext.Tenants.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task UpdateAsync(Guid id, UpdateTenantRequest request)
        {
            var tenant = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.Id == id);
            if (tenant == null)
            {
                throw new KeyNotFoundException("Tenant not found");
            }

            if (request.DisplayName != null) tenant.DisplayName = request.DisplayName;
            if (request.IsActive.HasValue) tenant.IsActive = request.IsActive.Value;
            if (request.Culture != null) tenant.Culture = request.Culture;
            if (request.ConnectionString != null) tenant.ConnectionString = request.ConnectionString;

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var tenant = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.Id == id);
            if (tenant == null) return;
            _dbContext.Tenants.Remove(tenant);
            await _dbContext.SaveChangesAsync();
        }

        private static TenantDto MapToDto(Tenant t) => new TenantDto(t.Id, t.Name, t.DisplayName, t.IsActive, t.Culture);
    }
}