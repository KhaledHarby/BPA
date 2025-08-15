using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaaSZero.Application.Common.Interfaces;
using SaaSZero.Infrastructure.Persistence;

namespace SaaSZero.API.Controllers
{
    public record MenuItemVm(string Key, string Text, string? Icon, string? Route, int Order, string? RequiredPermissionKey, List<MenuItemVm> Children);

    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly ITenantProvider _tenantProvider;

        public MenuController(AppDbContext dbContext, ITenantProvider tenantProvider)
        {
            _dbContext = dbContext;
            _tenantProvider = tenantProvider;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<List<MenuItemVm>>> Get([FromQuery] string culture = "en")
        {
            var tenantId = _tenantProvider.GetCurrentTenantId();

            var items = await _dbContext.MenuItems
                .Where(m => m.TenantId == tenantId && m.ParentId == null)
                .Include(m => m.Children)
                .Include(m => m.Localizations)
                .OrderBy(m => m.Order)
                .ToListAsync();

            string ResolveText(Domain.Navigation.MenuItem m)
            {
                var loc = m.Localizations.FirstOrDefault(l => l.Culture == culture)?.Text;
                return loc ?? m.DefaultText;
            }

            MenuItemVm Map(Domain.Navigation.MenuItem m) => new MenuItemVm(
                m.Key,
                ResolveText(m),
                m.Icon,
                m.Route,
                m.Order,
                m.RequiredPermissionKey,
                m.Children.OrderBy(c => c.Order).Select(Map).ToList()
            );

            return Ok(items.Select(Map).ToList());
        }
    }
}