using System;

namespace SaaSZero.Application.Tenants.DTOs
{
    public record TenantDto(Guid Id, string Name, string? DisplayName, bool IsActive, string? Culture);

    public record CreateTenantRequest(string Name, string? DisplayName, string? Culture, string? ConnectionString);

    public record UpdateTenantRequest(string? DisplayName, bool? IsActive, string? Culture, string? ConnectionString);
}