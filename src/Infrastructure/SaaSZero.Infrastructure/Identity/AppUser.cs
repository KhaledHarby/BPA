using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using SaaSZero.Domain.Entities;

namespace SaaSZero.Domain.Identity
{
    public class AppUser : IdentityUser<Guid>, IHasTenant, IAuditable, ISoftDeletable
    {
        public Guid TenantId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedOnUtc { get; set; }
        public Guid? DeletedByUserId { get; set; }
        public DateTimeOffset CreatedOnUtc { get; set; }
        public Guid? CreatedByUserId { get; set; }
        public DateTimeOffset? ModifiedOnUtc { get; set; }
        public Guid? ModifiedByUserId { get; set; }

        public ICollection<AppUserRole> UserRoles { get; set; } = new List<AppUserRole>();
    }

    public class AppRole : IdentityRole<Guid>, IHasTenant
    {
        public Guid TenantId { get; set; }
        public ICollection<AppRolePermission> RolePermissions { get; set; } = new List<AppRolePermission>();
    }

    public class AppUserRole : IdentityUserRole<Guid>
    {
        public virtual AppUser User { get; set; }
        public virtual AppRole Role { get; set; }
    }

    public class Permission : AuditableEntityBase<Guid>, IHasTenant
    {
        public Guid TenantId { get; set; }
        public string Key { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Group { get; set; }
    }

    public class AppRolePermission
    {
        public Guid RoleId { get; set; }
        public AppRole Role { get; set; }
        public Guid PermissionId { get; set; }
        public Permission Permission { get; set; }
    }
}