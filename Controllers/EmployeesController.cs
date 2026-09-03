using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCard.DTOs;
using SmartCard.Services;

namespace SmartCard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<EmployeeResponseDto>>> GetAllEmployees()
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            return Ok(employees);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<EmployeeResponseDto>> GetEmployee(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            return Ok(employee);
        }

        [HttpGet("number/{employeeNumber}")]
        [Authorize]
        public async Task<ActionResult<EmployeeResponseDto>> GetEmployeeByNumber(string employeeNumber)
        {
            var employee = await _employeeService.GetEmployeeByNumberAsync(employeeNumber);
            if (employee == null)
            {
                return NotFound();
            }
            return Ok(employee);
        }

        [HttpPost]
        public async Task<ActionResult<EmployeeResponseDto>> CreateEmployee(EmployeeCreateDto createDto)
        {
            var employee = await _employeeService.CreateEmployeeAsync(createDto);
            return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<EmployeeResponseDto>> UpdateEmployee(int id, EmployeeUpdateDto updateDto)
        {
            if (id != updateDto.Id)
            {
                return BadRequest();
            }

            var employee = await _employeeService.UpdateEmployeeAsync(updateDto);
            if (employee == null)
            {
                return NotFound();
            }
            return Ok(employee);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var result = await _employeeService.DeleteEmployeeAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpGet("count")]
        [AllowAnonymous]
        public async Task<ActionResult<int>> GetEmployeeCount()
        {
            var count = await _employeeService.GetEmployeeCountAsync();
            return Ok(count);
        }
    }
}