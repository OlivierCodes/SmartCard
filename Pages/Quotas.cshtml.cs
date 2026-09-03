using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SmartCard.Pages
{
    [Authorize]
    public class QuotasModel : PageModel
    {
        private readonly ILogger<QuotasModel> _logger;

        public QuotasModel(ILogger<QuotasModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}