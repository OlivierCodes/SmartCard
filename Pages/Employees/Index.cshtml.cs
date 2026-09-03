using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SmartCard.Pages.Employees
{
    [Authorize]
    public class EmployeesModel : PageModel
    {
        private readonly ILogger<EmployeesModel> _logger;

        public EmployeesModel(ILogger<EmployeesModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}