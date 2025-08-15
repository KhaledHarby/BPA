using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SaaSZero.Application.Common.Interfaces;
using SaaSZero.Domain.Auditing;
using SaaSZero.Domain.Entities;
using SaaSZero.Domain.Identity;
using SaaSZero.Domain.Navigation;
using SaaSZero.Domain.Tenancy;

namespace SaaSZero.Infrastructure.Persistence
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid, IdentityUserClaim<Guid>, AppUserRole, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
    {
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUserService _currentUser;

        public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenantProvider, ICurrentUserService currentUser) : base(options)
        {
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<AppRolePermission> RolePermissions => Set<AppRolePermission>();
        public DbSet<MenuItem> MenuItems => Set<MenuItem>();
        public DbSet<MenuLocalization> MenuLocalizations => Set<MenuLocalization>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<ExceptionLog> ExceptionLogs => Set<ExceptionLog>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Multi-tenant global filters and soft delete
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (typeof(IHasTenant).IsAssignableFrom(entityType.ClrType))
                {
                    var method = typeof(AppDbContext).GetMethod(nameof(ApplyTenantFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                        .MakeGenericMethod(entityType.ClrType);
                    method.Invoke(null, new object[] { builder, this });
                }

                if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
                {
                    var method = typeof(AppDbContext).GetMethod(nameof(ApplySoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                        .MakeGenericMethod(entityType.ClrType);
                    method.Invoke(null, new object[] { builder });
                }
            }

            builder.Entity<AppRolePermission>().HasKey(x => new { x.RoleId, x.PermissionId });
            builder.Entity<AppRolePermission>()
                .HasOne(x => x.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(x => x.RoleId);
            builder.Entity<AppRolePermission>()
                .HasOne(x => x.Permission)
                .WithMany()
                .HasForeignKey(x => x.PermissionId);

            builder.Entity<MenuItem>()
                .HasOne(m => m.Parent)
                .WithMany(m => m.Children)
                .HasForeignKey(m => m.ParentId);

            builder.Entity<MenuLocalization>()
                .HasOne(l => l.MenuItem)
                .WithMany(m => m.Localizations)
                .HasForeignKey(l => l.MenuItemId);
        }

        private static void ApplyTenantFilter<TEntity>(ModelBuilder builder, AppDbContext context) where TEntity : class, IHasTenant
        {
            builder.Entity<TEntity>().HasQueryFilter(e => e.TenantId == context._tenantProvider.GetCurrentTenantId());
        }

        private static void ApplySoftDeleteFilter<TEntity>(ModelBuilder builder) where TEntity : class, ISoftDeletable
        {
            builder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var utcNow = DateTimeOffset.UtcNow;
            var userId = _currentUser.UserId;

            foreach (var entry in ChangeTracker.Entries().ToList())
            {
                if (entry.Entity is IHasTenant tenantEntity && entry.State == EntityState.Added)
                {
                    if (tenantEntity.TenantId == Guid.Empty)
                    {
                        tenantEntity.TenantId = _tenantProvider.GetCurrentTenantId();
                    }
                }

                if (entry.Entity is IAuditable auditable)
                {
                    if (entry.State == EntityState.Added)
                    {
                        auditable.CreatedOnUtc = utcNow;
                        auditable.CreatedByUserId = userId;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        auditable.ModifiedOnUtc = utcNow;
                        auditable.ModifiedByUserId = userId;
                    }
                }

                if (entry.Entity is ISoftDeletable softDeletable && entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    softDeletable.IsDeleted = true;
                    softDeletable.DeletedOnUtc = utcNow;
                    softDeletable.DeletedByUserId = userId;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}