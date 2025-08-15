using System;
using SaaSZero.Domain.Entities;

namespace SaaSZero.Domain.Auditing
{
    public class AuditLog : TenantEntityBase<Guid>
    {
        public string Action { get; set; } = string.Empty; // e.g., User.Created
        public string EntityName { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public string? ChangesJson { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }

    public class ExceptionLog : TenantEntityBase<Guid>
    {
        public string Message { get; set; } = string.Empty;
        public string StackTrace { get; set; } = string.Empty;
        public string? Source { get; set; }
        public string? Path { get; set; }
        public int StatusCode { get; set; }
    }
}