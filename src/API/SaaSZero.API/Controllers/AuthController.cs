using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SaaSZero.Domain.Identity;
using SaaSZero.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SaaSZero.API.Controllers
{
    public record LoginRequest(string UserName, string Password, Guid TenantId);
    public record LoginResponse(string Token, DateTimeOffset ExpiresAtUtc);

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _dbContext;

        public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IConfiguration configuration, AppDbContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _dbContext = dbContext;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == request.UserName && u.TenantId == request.TenantId);
            if (user == null) return Unauthorized();

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded) return Unauthorized();

            var jwtSection = _configuration.GetSection("Jwt");
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));

            var roles = await _userManager.GetRolesAsync(user);
            var rolePermissions = await _dbContext.RolePermissions
                .Include(rp => rp.Permission)
                .Where(rp => rp.RoleId != Guid.Empty && roles.Contains(_dbContext.Roles.Where(r => r.Id == rp.RoleId).Select(r => r.Name).FirstOrDefault()!))
                .ToListAsync();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim("tenant", request.TenantId.ToString())
            };

            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
            claims.AddRange(rolePermissions.Select(p => new Claim("perm", p.Permission.Key)));

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return Ok(new LoginResponse(tokenString, DateTimeOffset.UtcNow.AddHours(8)));
        }
    }
}