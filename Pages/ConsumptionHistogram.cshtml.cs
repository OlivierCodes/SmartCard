using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SmartCard.Pages
{
    [Authorize]
    public class ConsumptionHistogramModel : PageModel
    {
        private readonly ILogger<ConsumptionHistogramModel> _logger;

        public ConsumptionHistogramModel(ILogger<ConsumptionHistogramModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}