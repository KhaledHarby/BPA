using System;
using SaaSZero.Domain.Entities;

namespace SaaSZero.Domain.Tenancy
{
    public class Tenant : AuditableEntityBase<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public bool IsActive { get; set; } = true;

        // Example: discriminator or connection string for per-tenant DB
        public string? ConnectionString { get; set; }
        public string? Culture { get; set; }
    }
}