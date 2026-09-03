using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using SmartCard.Models;
using SmartCard.Data;
using Microsoft.EntityFrameworkCore;

namespace SmartCard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestDbController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public TestDbController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<object>>> GetUsers()
        {
            var users = await _userManager.Users.Select(u => new {
                u.Id,
                u.UserName,
                u.Email,
                u.FirstName,
                u.LastName,
                u.CreatedDate
            }).ToListAsync();

            return Ok(users);
        }
    }
}