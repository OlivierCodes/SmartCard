using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SmartCard.Pages
{
    [Authorize]
    public class ConsumptionsModel : PageModel
    {
        private readonly ILogger<ConsumptionsModel> _logger;

        public ConsumptionsModel(ILogger<ConsumptionsModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}