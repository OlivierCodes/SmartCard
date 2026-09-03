using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SmartCard.Models;

namespace SmartCard.Controllers
{
    [ApiController]
    [Route("api/mobile/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // JWT auth for mobile
    public class MobileRoleManagementController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public MobileRoleManagementController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet("roles")]
        public async Task<ActionResult<IEnumerable<string>>> GetRoles()
        {
            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return Ok(roles);
        }

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<MobileUserRoleDto>>> GetUsersWithRoles()
        {
            var users = _userManager.Users.ToList();
            var userRoles = new List<MobileUserRoleDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles.Add(new MobileUserRoleDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName!,
                    LastName = user.LastName!,
                    Roles = roles.ToList()
                });
            }

            return Ok(userRoles);
        }

        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request)
        {
            // Vérifier que l'utilisateur courant est un administrateur
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || !await _userManager.IsInRoleAsync(currentUser, "Admin"))
            {
                return Forbid();
            }

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return NotFound(new { message = "Utilisateur non trouvé" });
            }

            var result = await _userManager.AddToRoleAsync(user, request.RoleName);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(new { message = "Erreur d'attribution du rôle", errors });
            }

            return Ok(new { message = $"Rôle {request.RoleName} attribué avec succès à l'utilisateur" });
        }

        [HttpDelete("remove-role")]
        public async Task<IActionResult> RemoveRole([FromBody] AssignRoleRequest request)
        {
            // Vérifier que l'utilisateur courant est un administrateur
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || !await _userManager.IsInRoleAsync(currentUser, "Admin"))
            {
                return Forbid();
            }

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return NotFound(new { message = "Utilisateur non trouvé" });
            }

            var result = await _userManager.RemoveFromRoleAsync(user, request.RoleName);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(new { message = "Erreur de suppression du rôle", errors });
            }

            return Ok(new { message = $"Rôle {request.RoleName} supprimé avec succès de l'utilisateur" });
        }
    }
}