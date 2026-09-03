using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartCard.DTOs;
using SmartCard.Models;
using SmartCard.Services;
using System.Security.Claims;

namespace SmartCard.Controllers
{
    [ApiController]
    [Route("api/mobile/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")] // JWT auth for mobile
    public class PumpAttendantsController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmployeeService _employeeService;
        private readonly ICardService _cardService;
        private readonly IConsumptionService _consumptionService;
        private readonly IFuelQuotaService _fuelQuotaService;

        public PumpAttendantsController(
            UserManager<ApplicationUser> userManager,
            IEmployeeService employeeService,
            ICardService cardService,
            IConsumptionService consumptionService,
            IFuelQuotaService fuelQuotaService)
        {
            _userManager = userManager;
            _employeeService = employeeService;
            _cardService = cardService;
            _consumptionService = consumptionService;
            _fuelQuotaService = fuelQuotaService;
        }

        [HttpGet("employees/{cardNumber}")]
        public async Task<ActionResult<MobileEmployeeDto>> GetEmployeeByCard(string cardNumber)
        {
            // Vérifier les permissions du pompiste
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || !await _userManager.IsInRoleAsync(currentUser, "Pompiste"))
            {
                return Forbid();
            }

            var card = await _cardService.GetCardByNumberAsync(cardNumber);
            if (card == null)
            {
                return NotFound(new { message = "Carte non trouvée" });
            }

            if (card.Employee == null)
            {
                return NotFound(new { message = "Carte non attribuée à un employé" });
            }

            // Calculer le quota disponible pour l'employé
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var availableQuota = await _fuelQuotaService.GetAvailableFuelAsync(card.Employee.Id, currentMonth, currentYear);

            return new MobileEmployeeDto
            {
                Id = card.Employee.Id,
                Name = $"{card.Employee.FirstName} {card.Employee.LastName}",
                EmployeeNumber = card.Employee.EmployeeNumber,
                AvailableQuota = (double)availableQuota,
                CardNumber = cardNumber,
                Department = card.Employee.Department
            };
        }

        [HttpPost("dispense")]
        public async Task<ActionResult<MobileDispenseResponse>> DispenseFuel([FromBody] MobileDispenseRequest request)
        {
            // Vérifier les permissions du pompiste
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || !await _userManager.IsInRoleAsync(currentUser, "Pompiste"))
            {
                return Forbid();
            }

            var card = await _cardService.GetCardByNumberAsync(request.CardNumber);
            if (card == null)
            {
                return NotFound(new { message = "Carte non trouvée" });
            }

            if (card.Employee == null)
            {
                return NotFound(new { message = "Carte non attribuée à un employé" });
            }

            // Vérifier que la quantité demandée est positive
            if (request.Amount <= 0)
            {
                return BadRequest(new {
                    message = "La quantité de carburant à prélever doit être supérieure à zéro",
                    requestedAmount = request.Amount
                });
            }

            // Vérifier le quota
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var availableQuota = await _fuelQuotaService.GetAvailableFuelAsync(card.Employee.Id, currentMonth, currentYear);

            if (availableQuota < (decimal)request.Amount)
            {
                return BadRequest(new {
                    message = $"Le quota disponible est insuffisant pour ce prélèvement. Quantité demandée: {request.Amount}L, Quota disponible: {(double)availableQuota}L",
                    requestedAmount = request.Amount,
                    availableQuota = (double)availableQuota
                });
            }

            // Créer le DTO pour le service de consommation
            var consumptionCreateDto = new ConsumptionCreateDto
            {
                EmployeeId = card.Employee.Id,
                AmountInLiters = (decimal)request.Amount,
                CardId = card.Id,
                Description = request.Description
            };

            // Effectuer le prélèvement
            var consumption = await _consumptionService.CreateConsumptionAsync(consumptionCreateDto);

            // Mettre à jour le quota
            var remainingQuota = availableQuota - (decimal)request.Amount;

            return new MobileDispenseResponse
            {
                Success = true,
                EmployeeName = $"{card.Employee.FirstName} {card.Employee.LastName}",
                Amount = request.Amount,
                RemainingQuota = (double)remainingQuota,
                TransactionId = consumption.Id
            };
        }
    }

    public class MobileDispenseRequest
    {
        public string CardNumber { get; set; } = string.Empty;
        public double Amount { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class MobileDispenseResponse
    {
        public bool Success { get; set; }
        public string? EmployeeName { get; set; }
        public double Amount { get; set; }
        public double RemainingQuota { get; set; }
        public int? TransactionId { get; set; }
        public string? Message { get; set; }
    }
}