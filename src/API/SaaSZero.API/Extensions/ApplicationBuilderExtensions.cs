using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SaaSZero.Domain.Identity;
using SaaSZero.Infrastructure.Persistence;
using SaaSZero.Infrastructure.Seed;

namespace SaaSZero.API.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static async Task UseDatabaseInitializationAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
            await DbSeeder.SeedAsync(db, userMgr, roleMgr);
        }
    }
}