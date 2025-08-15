using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SaaSZero.Application.Common.Interfaces;
using SaaSZero.Domain.Auditing;
using SaaSZero.Infrastructure.Persistence;

namespace SaaSZero.API.Middleware
{
	public class ExceptionHandlingMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<ExceptionHandlingMiddleware> _logger;
		private readonly ITenantProvider _tenantProvider;

		public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, ITenantProvider tenantProvider)
		{
			_next = next;
			_logger = logger;
			_tenantProvider = tenantProvider;
		}

		public async Task Invoke(HttpContext context, AppDbContext db)
		{
			try
			{
				await _next(context);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unhandled exception");
				var tenantId = _tenantProvider.GetCurrentTenantId();
				var log = new ExceptionLog
				{
					Id = Guid.NewGuid(),
					TenantId = tenantId,
					Message = ex.Message,
					StackTrace = ex.StackTrace ?? string.Empty,
					Source = ex.Source,
					Path = context.Request.Path,
					StatusCode = (int)HttpStatusCode.InternalServerError,
					CreatedOnUtc = DateTimeOffset.UtcNow
				};
				db.ExceptionLogs.Add(log);
				await db.SaveChangesAsync();

				context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				context.Response.ContentType = "application/problem+json";
				var body = JsonSerializer.Serialize(new
				{
					title = "An unexpected error occurred.",
					status = 500,
					traceId = context.TraceIdentifier
				});
				await context.Response.WriteAsync(body);
			}
		}
	}
}