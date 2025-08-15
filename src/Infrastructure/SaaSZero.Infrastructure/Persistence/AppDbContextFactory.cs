using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace SaaSZero.Infrastructure.Persistence
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(config.GetConnectionString("DefaultConnection"));

            return new AppDbContext(optionsBuilder.Options, new DesignTenantProvider(), new DesignCurrentUser());
        }

        private class DesignTenantProvider : SaaSZero.Application.Common.Interfaces.ITenantProvider
        {
            public Guid GetCurrentTenantId() => Guid.Empty;
        }

        private class DesignCurrentUser : SaaSZero.Application.Common.Interfaces.ICurrentUserService
        {
            public Guid? UserId => null;
            public string? UserName => null;
            public bool IsInRole(string role) => false;
            public bool HasPermission(string permissionKey) => false;
        }
    }
}