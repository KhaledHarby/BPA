using System;
using System.Collections.Generic;
using SaaSZero.Domain.Entities;

namespace SaaSZero.Domain.Navigation
{
    public class MenuItem : TenantEntityBase<Guid>
    {
        public string Key { get; set; } = string.Empty; // unique key used by frontend
        public string DefaultText { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? Route { get; set; }
        public int Order { get; set; }
        public Guid? ParentId { get; set; }
        public MenuItem? Parent { get; set; }
        public ICollection<MenuItem> Children { get; set; } = new List<MenuItem>();
        public ICollection<MenuLocalization> Localizations { get; set; } = new List<MenuLocalization>();
        public string? RequiredPermissionKey { get; set; }
    }

    public class MenuLocalization : AuditableEntityBase<Guid>, IHasTenant
    {
        public Guid TenantId { get; set; }
        public Guid MenuItemId { get; set; }
        public MenuItem MenuItem { get; set; }
        public string Culture { get; set; } = "en";
        public string Text { get; set; } = string.Empty;
    }
}