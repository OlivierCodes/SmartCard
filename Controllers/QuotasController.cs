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
    public class QuotasController : ControllerBase
    {
        private readonly IFuelQuotaService _fuelQuotaService;

        public QuotasController(IFuelQuotaService fuelQuotaService)
        {
            _fuelQuotaService = fuelQuotaService;
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<IEnumerable<FuelQuota>>> GetQuotasByEmployee(int employeeId)
        {
            var quotas = await _fuelQuotaService.GetQuotasByEmployeeAsync(employeeId);
            return Ok(quotas);
        }

        [HttpGet("employee/{employeeId}/month/{month}/year/{year}")]
        public async Task<ActionResult<FuelQuota>> GetQuotaByMonth(int employeeId, int month, int year)
        {
            var quota = await _fuelQuotaService.GetQuotaByMonthAsync(employeeId, month, year);
            if (quota == null)
            {
                return NotFound();
            }
            return Ok(quota);
        }

        [HttpPost]
        public async Task<ActionResult<FuelQuota>> CreateQuota([FromBody] FuelQuotaCreateDto createDto)
        {
            var quota = await _fuelQuotaService.CreateFuelQuotaAsync(createDto);
            return CreatedAtAction(nameof(GetQuotaByMonth), new { employeeId = quota.EmployeeId, month = quota.Month, year = quota.Year }, quota);
        }

        [HttpPut]
        public async Task<ActionResult<FuelQuota>> UpdateQuota([FromBody] FuelQuotaUpdateDto updateDto)
        {
            var quota = await _fuelQuotaService.UpdateQuotaAsync(updateDto);
            return Ok(quota);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuota(int id)
        {
            var result = await _fuelQuotaService.DeleteFuelQuotaAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpGet("employee/{employeeId}/available")]
        public async Task<ActionResult<decimal>> GetAvailableFuel(int employeeId)
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var availableFuel = await _fuelQuotaService.GetAvailableFuelAsync(employeeId, currentMonth, currentYear);
            return Ok(availableFuel);
        }

        [HttpGet("available-total")]
        public async Task<ActionResult<decimal>> GetTotalAvailableFuel()
        {
            var totalAvailable = await _fuelQuotaService.GetTotalAvailableFuelAsync();
            return Ok(totalAvailable);
        }
    }
}