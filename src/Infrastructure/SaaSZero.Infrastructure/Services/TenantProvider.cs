using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SaaSZero.Application.Common.Interfaces;

namespace SaaSZero.Infrastructure.Services
{
    public class TenantProvider : ITenantProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid GetCurrentTenantId()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
            {
                return Guid.Empty;
            }

            // Resolve tenant from header or claim
            var fromHeader = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();
            if (Guid.TryParse(fromHeader, out var tenantHeader))
            {
                return tenantHeader;
            }

            var claim = context.User.FindFirst("tenant")?.Value;
            if (Guid.TryParse(claim, out var tenantClaim))
            {
                return tenantClaim;
            }

            return Guid.Empty;
        }
    }

    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? UserId
        {
            get
            {
                var id = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                return Guid.TryParse(id, out var g) ? g : null;
            }
        }

        public string? UserName => _httpContextAccessor.HttpContext?.User.Identity?.Name;

        public bool IsInRole(string role) => _httpContextAccessor.HttpContext?.User.IsInRole(role) ?? false;

        public bool HasPermission(string permissionKey)
        {
            return _httpContextAccessor.HttpContext?.User.Claims.Any(c => c.Type == "perm" && c.Value == permissionKey) ?? false;
        }
    }
}