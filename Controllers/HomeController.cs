using Microsoft.AspNetCore.Mvc;

namespace SmartCard.Controllers
{
    // Ce contrôleur gère les routes racine et Home
    [Route("[controller]")]
    public class HomeController : Controller
    {
        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            // Si l'utilisateur est connecté, rediriger vers la page d'index (dashboard)
            if (User.Identity.IsAuthenticated)
            {
                return Redirect("/Index"); // Rediriger vers la page Index Razor
            }

            // Sinon, rediriger vers la page de login
            return RedirectToAction("Index", "Login");
        }

        [HttpGet]
        [Route("Home")]
        public IActionResult Home()
        {
            // Même logique pour /Home
            if (User.Identity.IsAuthenticated)
            {
                return Redirect("/Index"); // Rediriger vers la page Index Razor
            }

            return RedirectToAction("Index", "Login");
        }
    }

}