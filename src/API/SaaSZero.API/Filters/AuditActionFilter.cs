using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using SaaSZero.Application.Common.Interfaces;
using SaaSZero.Domain.Auditing;
using SaaSZero.Infrastructure.Persistence;

namespace SaaSZero.API.Filters
{
	public class AuditActionFilter : IAsyncActionFilter
	{
		private readonly AppDbContext _db;
		private readonly ITenantProvider _tenantProvider;

		public AuditActionFilter(AppDbContext db, ITenantProvider tenantProvider)
		{
			_db = db;
			_tenantProvider = tenantProvider;
		}

		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			var executed = await next();
			var tenantId = _tenantProvider.GetCurrentTenantId();
			var log = new AuditLog
			{
				Id = Guid.NewGuid(),
				TenantId = tenantId,
				Action = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}",
				EntityName = executed.Controller?.GetType().Name ?? string.Empty,
				CreatedOnUtc = DateTimeOffset.UtcNow
			};
			_db.AuditLogs.Add(log);
			await _db.SaveChangesAsync();
		}
	}
}