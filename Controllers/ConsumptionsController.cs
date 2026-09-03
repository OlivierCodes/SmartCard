using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCard.DTOs;
using SmartCard.Models;
using SmartCard.Services;

namespace SmartCard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConsumptionsController : ControllerBase
    {
        private readonly IConsumptionService _consumptionService;

        public ConsumptionsController(IConsumptionService consumptionService)
        {
            _consumptionService = consumptionService;
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<IEnumerable<Consumption>>> GetConsumptionsByEmployee(int employeeId)
        {
            var consumptions = await _consumptionService.GetConsumptionsByEmployeeAsync(employeeId);
            return Ok(consumptions);
        }

        [HttpGet("employee/{employeeId}/period")]
        public async Task<ActionResult<IEnumerable<Consumption>>> GetConsumptionsByPeriod(
            int employeeId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var consumptions = await _consumptionService.GetConsumptionsByPeriodAsync(employeeId, startDate, endDate);
            return Ok(consumptions);
        }

        [HttpGet("employee/{employeeId}/total")]
        public async Task<ActionResult<decimal>> GetTotalConsumptionByEmployee(int employeeId)
        {
            var total = await _consumptionService.GetTotalConsumptionByEmployeeAsync(employeeId);
            return Ok(total);
        }

        [HttpGet("employee/{employeeId}/total-period")]
        public async Task<ActionResult<decimal>> GetTotalConsumptionByPeriod(
            int employeeId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var total = await _consumptionService.GetTotalConsumptionByPeriodAsync(employeeId, startDate, endDate);
            return Ok(total);
        }

        [HttpGet("employee/{employeeId}/grouped-period")]
        public async Task<ActionResult<IEnumerable<ConsumptionByPeriodDto>>> GetConsumptionsGroupedByPeriod(
            int employeeId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string groupBy = "day")
        {
            var groupedConsumptions = await _consumptionService.GetConsumptionsByPeriodGroupedAsync(employeeId, startDate, endDate, groupBy);
            return Ok(groupedConsumptions);
        }

        [HttpGet("all-employees/grouped-period")]
        public async Task<ActionResult<IEnumerable<ConsumptionByPeriodDto>>> GetConsumptionsForAllEmployeesGroupedByPeriod(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string groupBy = "day")
        {
            var groupedConsumptions = await _consumptionService.GetConsumptionsForAllEmployeesByPeriodGroupedAsync(startDate, endDate, groupBy);
            return Ok(groupedConsumptions);
        }

        [HttpGet("total-all-employees-period")]
        public async Task<ActionResult<decimal>> GetTotalConsumptionForAllEmployeesByPeriod(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var total = await _consumptionService.GetTotalConsumptionForAllEmployeesByPeriodAsync(startDate, endDate);
            return Ok(total);
        }

        [HttpPost]
        public async Task<ActionResult<Consumption>> CreateConsumption([FromBody] ConsumptionCreateDto createDto)
        {
            var consumption = await _consumptionService.CreateConsumptionAsync(createDto);
            return CreatedAtAction(nameof(GetConsumptionsByEmployee), new { employeeId = consumption.EmployeeId }, consumption);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConsumption(int id)
        {
            var result = await _consumptionService.DeleteConsumptionAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}