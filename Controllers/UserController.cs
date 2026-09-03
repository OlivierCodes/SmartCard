using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartCard.DTOs;
using SmartCard.Models;

namespace SmartCard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet("current")]
        public async Task<ActionResult<UserInfoDto>> GetCurrentUser()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var userInfo = new UserInfoDto
            {
                Id = user.Id,
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                Email = user.Email ?? "",
                FullName = $"{user.FirstName ?? ""} {user.LastName ?? ""}".Trim()
            };

            return Ok(userInfo);
        }
    }
}