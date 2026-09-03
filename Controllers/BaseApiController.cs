using Microsoft.AspNetCore.Mvc;
using SmartCard.Exceptions;

namespace SmartCard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseApiController : ControllerBase
    {
        protected IActionResult HandleException(Exception ex)
        {
            return ex switch
            {
                ArgumentException => BadRequest(new { error = ex.Message }),
                EmployeeNotFoundException => NotFound(new { error = ex.Message }),
                CardNotFoundException => NotFound(new { error = ex.Message }),
                InsufficientFuelException => BadRequest(new { error = ex.Message }),
                InvalidCardException => BadRequest(new { error = ex.Message }),
                _ => StatusCode(500, new { error = "An internal server error occurred" })
            };
        }
    }
}