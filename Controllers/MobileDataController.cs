using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SmartCard.DTOs;
using SmartCard.Models;
using SmartCard.Services;
using System.Security.Claims;

namespace SmartCard.Controllers
{
    [ApiController]
    [Route("api/mobile/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // JWT auth for mobile
    public class MobileDataController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;
        private readonly ICardService _cardService;
        private readonly IConsumptionService _consumptionService;

        // Fix for CS1520 and IDE0290: Correct constructor name and use primary constructor syntax
        public MobileDataController(
            UserManager<ApplicationUser> userManager,
            IEmployeeService employeeService,
            IDepartmentService departmentService,
            ICardService cardService,
            IConsumptionService consumptionService)
        {
            _userManager = userManager;
            _employeeService = employeeService;
            _departmentService = departmentService;
            _cardService = cardService;
            _consumptionService = consumptionService;
        }

        [HttpGet("employees")]
        public async Task<ActionResult<IEnumerable<MobileEmployeeDto>>> GetEmployees()
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            var mobileEmployees = employees.Select(e => new MobileEmployeeDto
            {
                Id = e.Id,
                Name = $"{e.FirstName} {e.LastName}",
                EmployeeNumber = e.EmployeeNumber,
                AvailableQuota = (double)e.AvailableFuelQuota,
                Department = e.Department ?? string.Empty,
                CardNumber = e.Cards?.FirstOrDefault()?.CardNumber ?? string.Empty
            }).ToList();

            return Ok(mobileEmployees);
        }

        [HttpGet("departments")]
        public async Task<ActionResult<IEnumerable<MobileDepartmentDto>>> GetDepartments()
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();
            var mobileDepartments = departments.Select(d => new MobileDepartmentDto
            {
                Id = d.Id,
                Name = d.Name,
                // Remplacer EmployeeCount s'il n'existe pas
                EmployeeCount = d.EmployeeCount,
                CreatedDate = d.CreatedDate
            }).ToList();

            return Ok(mobileDepartments);
        }

        [HttpGet("cards")]
        public async Task<ActionResult<IEnumerable<MobileCardDto>>> GetCards()
        {
            var cards = await _cardService.GetAllCardsAsync();
            var mobileCards = cards.Select(c => new MobileCardDto
            {
                Id = c.Id,
                CardNumber = c.CardNumber,
                // Convertir CardStatusDto en string
                Status = c.Status.ToString(),
                CreatedDate = c.CreatedDate,
                EmployeeId = c.EmployeeId,
                // Assurez-vous que la propriété Employee existe
                EmployeeName = c.Employee != null ? $"{c.Employee.FirstName} {c.Employee.LastName}" : null
            }).ToList();

            return Ok(mobileCards);
        }

        [HttpGet("consumption-history/{employeeId}")]
        public async Task<ActionResult<IEnumerable<MobileConsumptionHistoryDto>>> GetConsumptionHistory(int employeeId)
        {
            var consumptionHistory = await _consumptionService.GetConsumptionHistoryForMobileAsync(employeeId);
            return Ok(consumptionHistory);
        }

        [HttpGet("all-employees-consumption-history")]
        public async Task<ActionResult<IEnumerable<MobileConsumptionHistoryDto>>> GetAllEmployeesConsumptionHistory()
        {
            var consumptionHistory = await _consumptionService.GetAllEmployeesConsumptionHistoryForMobileAsync();
            return Ok(consumptionHistory);
        }

        [HttpDelete("consumption/{id}")]
        public async Task<IActionResult> DeleteConsumption(int id)
        {
            var result = await _consumptionService.DeleteConsumptionAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Consumption record not found" });
            }
            return Ok(new { message = "Consumption deleted successfully" });
        }

        [HttpGet("total-available-quota")]
        public async Task<ActionResult<decimal>> GetTotalAvailableQuota()
        {
            var totalAvailable = await _consumptionService.GetTotalAvailableQuotaForAllEmployeesAsync();
            return Ok(totalAvailable);
        }

        [HttpGet("total-consumed-quota")]
        public async Task<ActionResult<decimal>> GetTotalConsumedQuota()
        {
            var totalConsumed = await _consumptionService.GetTotalConsumedQuotaForAllEmployeesAsync();
            return Ok(totalConsumed);
        }

        [HttpGet("all-employees-grouped-period")]
        public async Task<ActionResult<IEnumerable<ConsumptionByPeriodDto>>> GetConsumptionsForAllEmployeesGroupedByPeriod(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string groupBy = "day")
        {
            var groupedConsumptions = await _consumptionService.GetConsumptionsForAllEmployeesByPeriodGroupedForMobileAsync(startDate, endDate, groupBy);
            return Ok(groupedConsumptions);
        }
    }

    public class MobileDepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int EmployeeCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class MobileCardDto
    {
        public int Id { get; set; }
        public string CardNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public int? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
    }
}