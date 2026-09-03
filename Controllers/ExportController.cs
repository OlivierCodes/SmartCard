using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCard.Services;
using System.Text;

namespace SmartCard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ExportController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;
        private readonly ICardService _cardService;
        private readonly IConsumptionService _consumptionService;

        public ExportController(
            IEmployeeService employeeService,
            IDepartmentService departmentService,
            ICardService cardService,
            IConsumptionService consumptionService)
        {
            _employeeService = employeeService;
            _departmentService = departmentService;
            _cardService = cardService;
            _consumptionService = consumptionService;
        }

        [HttpGet("json")]
        public async Task<IActionResult> ExportToJson()
        {
            var data = new
            {
                ExportDate = DateTime.Now,
                Departments = await _departmentService.GetAllDepartmentsAsync(),
                Employees = await _employeeService.GetAllEmployeesAsync(),
                Cards = await _cardService.GetAllCardsAsync(),
                Consumptions = await _consumptionService.GetAllEmployeesConsumptionHistoryForMobileAsync()
            };

            return Ok(data);
        }

        [HttpGet("csv/employees")]
        public async Task<IActionResult> ExportEmployeesToCsv()
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            var sb = new StringBuilder();
            sb.AppendLine("Id;Matricule;Prenom;Nom;Email;Departement;Quota;StockDisponible;Statut");

            foreach (var emp in employees)
            {
                sb.AppendLine($"{emp.Id};{emp.EmployeeNumber};{emp.FirstName};{emp.LastName};{emp.Email};{emp.Department};{emp.MonthlyFuelQuota};{emp.AvailableFuelQuota};{(emp.IsActive ? "Actif" : "Inactif")}");
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"employees_{DateTime.Now:yyyyMMdd}.csv");
        }

        [HttpGet("csv/consumptions")]
        public async Task<IActionResult> ExportConsumptionsToCsv()
        {
            var consumptions = await _consumptionService.GetAllEmployeesConsumptionHistoryForMobileAsync();
            var sb = new StringBuilder();
            sb.AppendLine("Id;Date;Employe;Matricule;Quantite(L);Description");

            foreach (var c in consumptions)
            {
                sb.AppendLine($"{c.Id};{c.TransactionDate};{c.EmployeeName};{c.EmployeeNumber};{c.AmountInLiters};{c.Description}");
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"consommations_{DateTime.Now:yyyyMMdd}.csv");
        }

        [HttpGet("csv/cards")]
        public async Task<IActionResult> ExportCardsToCsv()
        {
            var cards = await _cardService.GetAllCardsAsync();
            var sb = new StringBuilder();
            sb.AppendLine("Id;NumeroCarte;Statut;DateCreation;Employe;Matricule");

            foreach (var c in cards)
            {
                var employeeName = c.Employee != null ? $"{c.Employee.FirstName} {c.Employee.LastName}" : "Non attribuée";
                var employeeNum = c.Employee != null ? c.Employee.EmployeeNumber : "";
                sb.AppendLine($"{c.Id};{c.CardNumber};{c.Status};{c.CreatedDate};{employeeName};{employeeNum}");
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"cartes_{DateTime.Now:yyyyMMdd}.csv");
        }
    }
}