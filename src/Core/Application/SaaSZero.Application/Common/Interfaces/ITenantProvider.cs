using System;

namespace SaaSZero.Application.Common.Interfaces
{
    public interface ITenantProvider
    {
        Guid GetCurrentTenantId();
    }

    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        string? UserName { get; }
        bool IsInRole(string role);
        bool HasPermission(string permissionKey);
    }
}