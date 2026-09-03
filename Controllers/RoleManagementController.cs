using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartCard.Models;

namespace SmartCard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RoleManagementController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleManagementController(
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
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserRoleDto>>> GetUsersWithRoles()
        {
            var users = _userManager.Users.ToList();
            var userRoles = new List<UserRoleDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles.Add(new UserRoleDto
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

        [HttpPost("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateUpdateDto request)
        {
            if (string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Le mot de passe est requis pour la création d'un utilisateur" });
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Erreur lors de la création de l'utilisateur", errors = result.Errors.Select(e => e.Description) });
            }

            if (request.Roles != null && request.Roles.Any())
            {
                await _userManager.AddToRolesAsync(user, request.Roles);
            }

            return Ok(new { message = "Utilisateur créé avec succès", userId = user.Id });
        }

        [HttpPut("users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserCreateUpdateDto request)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Utilisateur non trouvé" });
            }

            user.Email = request.Email;
            user.UserName = request.Email;
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Erreur lors de la mise à jour de l'utilisateur", errors = result.Errors.Select(e => e.Description) });
            }

            if (!string.IsNullOrEmpty(request.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _userManager.ResetPasswordAsync(user, token, request.Password);
            }

            // Mettre à jour les rôles
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            
            if (request.Roles != null && request.Roles.Any())
            {
                await _userManager.AddToRolesAsync(user, request.Roles);
            }

            return Ok(new { message = "Utilisateur mis à jour avec succès" });
        }

        [HttpDelete("users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Utilisateur non trouvé" });
            }

            // Ne pas permettre de supprimer soi-même
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null && currentUser.Id == id)
            {
                return BadRequest(new { message = "Vous ne pouvez pas supprimer votre propre compte" });
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Erreur lors de la suppression de l'utilisateur", errors = result.Errors.Select(e => e.Description) });
            }

            return Ok(new { message = "Utilisateur supprimé avec succès" });
        }

        [HttpPost("assign-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request)
        {
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

        [HttpPost("remove-role")]
        [HttpDelete("remove-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveRole([FromBody] AssignRoleRequest request)
        {
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

        [HttpPost("create-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
        {
            var role = new IdentityRole(request.RoleName);
            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(new { message = "Erreur de création du rôle", errors });
            }

            return Ok(new { message = $"Rôle {request.RoleName} créé avec succès" });
        }

        [HttpDelete("role/{roleName}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                return NotFound(new { message = "Rôle non trouvé" });
            }

            // Vérifier si le rôle est attribué à des utilisateurs
            var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
            if (usersInRole.Any())
            {
                return BadRequest(new { message = $"Impossible de supprimer le rôle '{roleName}' car il est attribué à {usersInRole.Count} utilisateur(s). Retirez le rôle de tous les utilisateurs d'abord." });
            }

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(new { message = "Erreur de suppression du rôle", errors });
            }

            return Ok(new { message = $"Rôle {roleName} supprimé avec succès" });
        }
    }
}