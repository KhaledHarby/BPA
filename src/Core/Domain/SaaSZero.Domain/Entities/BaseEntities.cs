using System;

namespace SaaSZero.Domain.Entities
{
    public interface IHasTenant
    {
        Guid TenantId { get; set; }
    }

    public interface ISoftDeletable
    {
        bool IsDeleted { get; set; }
        DateTimeOffset? DeletedOnUtc { get; set; }
        Guid? DeletedByUserId { get; set; }
    }

    public interface IAuditable
    {
        DateTimeOffset CreatedOnUtc { get; set; }
        Guid? CreatedByUserId { get; set; }
        DateTimeOffset? ModifiedOnUtc { get; set; }
        Guid? ModifiedByUserId { get; set; }
    }

    public abstract class EntityBase<TId>
    {
        public TId Id { get; set; }
    }

    public abstract class AuditableEntityBase<TId> : EntityBase<TId>, IAuditable
    {
        public DateTimeOffset CreatedOnUtc { get; set; }
        public Guid? CreatedByUserId { get; set; }
        public DateTimeOffset? ModifiedOnUtc { get; set; }
        public Guid? ModifiedByUserId { get; set; }
    }

    public abstract class TenantEntityBase<TId> : AuditableEntityBase<TId>, IHasTenant, ISoftDeletable
    {
        public Guid TenantId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedOnUtc { get; set; }
        public Guid? DeletedByUserId { get; set; }
    }
}