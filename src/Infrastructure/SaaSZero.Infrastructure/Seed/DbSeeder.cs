using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SaaSZero.Domain.Identity;
using SaaSZero.Domain.Navigation;
using SaaSZero.Domain.Tenancy;
using SaaSZero.Infrastructure.Persistence;

namespace SaaSZero.Infrastructure.Seed
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext db, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            await db.Database.MigrateAsync();

            // Create default tenant
            var defaultTenant = await db.Tenants.FirstOrDefaultAsync(t => t.Name == "Default");
            if (defaultTenant == null)
            {
                defaultTenant = new Tenant
                {
                    Id = Guid.NewGuid(),
                    Name = "Default",
                    DisplayName = "Default Tenant",
                    IsActive = true,
                    Culture = "en"
                };
                db.Tenants.Add(defaultTenant);
                await db.SaveChangesAsync();
            }

            // Roles
            var adminRole = await roleManager.Roles.FirstOrDefaultAsync(r => r.Name == "Admin" && r.TenantId == defaultTenant.Id);
            if (adminRole == null)
            {
                adminRole = new AppRole { Id = Guid.NewGuid(), Name = "Admin", NormalizedName = "ADMIN", TenantId = defaultTenant.Id };
                await roleManager.CreateAsync(adminRole);
            }

            // Admin user
            var adminUser = await userManager.Users.FirstOrDefaultAsync(u => u.UserName == "admin" && u.TenantId == defaultTenant.Id);
            if (adminUser == null)
            {
                adminUser = new AppUser
                {
                    Id = Guid.NewGuid(),
                    UserName = "admin",
                    NormalizedUserName = "ADMIN",
                    Email = "admin@tenant.local",
                    NormalizedEmail = "ADMIN@TENANT.LOCAL",
                    EmailConfirmed = true,
                    TenantId = defaultTenant.Id
                };
                await userManager.CreateAsync(adminUser, "Admin123!@#");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Permissions
            var permissions = new[]
            {
                ("Permissions.Tenants.View", "View Tenants"),
                ("Permissions.Tenants.Manage", "Manage Tenants"),
                ("Permissions.Users.View", "View Users"),
                ("Permissions.Users.Manage", "Manage Users")
            };
            foreach (var (key, display) in permissions)
            {
                if (!await db.Permissions.AnyAsync(p => p.Key == key && p.TenantId == defaultTenant.Id))
                {
                    db.Permissions.Add(new Permission
                    {
                        Id = Guid.NewGuid(),
                        TenantId = defaultTenant.Id,
                        Key = key,
                        DisplayName = display
                    });
                }
            }
            await db.SaveChangesAsync();

            // Assign all permissions to Admin
            var allPerms = await db.Permissions.Where(p => p.TenantId == defaultTenant.Id).ToListAsync();
            foreach (var perm in allPerms)
            {
                if (!await db.RolePermissions.AnyAsync(rp => rp.RoleId == adminRole.Id && rp.PermissionId == perm.Id))
                {
                    db.RolePermissions.Add(new AppRolePermission { RoleId = adminRole.Id, PermissionId = perm.Id });
                }
            }
            await db.SaveChangesAsync();

            // Menus
            if (!await db.MenuItems.AnyAsync(m => m.TenantId == defaultTenant.Id))
            {
                var root = new MenuItem
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenant.Id,
                    Key = "dashboard",
                    DefaultText = "Dashboard",
                    Icon = "dashboard",
                    Order = 0
                };
                var tenants = new MenuItem
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenant.Id,
                    Key = "tenants",
                    DefaultText = "Tenants",
                    Icon = "groups",
                    Route = "/tenants",
                    Order = 1,
                    RequiredPermissionKey = "Permissions.Tenants.View"
                };

                db.MenuItems.AddRange(root, tenants);
                await db.SaveChangesAsync();
            }
        }
    }
}