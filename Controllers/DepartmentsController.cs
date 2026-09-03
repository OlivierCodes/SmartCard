using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCard.DTOs;
using SmartCard.Services;

namespace SmartCard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
     // Exige l'authentification pour tous les endpoints
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet]
        [Authorize] 
        public async Task<ActionResult<IEnumerable<DepartmentResponseDto>>> GetAllDepartments()
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();
            return Ok(departments);
        }

        [HttpGet("{id}")]
        [Authorize] 
        public async Task<ActionResult<DepartmentResponseDto>> GetDepartment(int id)
        {
            var department = await _departmentService.GetDepartmentByIdAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            return Ok(department);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<DepartmentResponseDto>> CreateDepartment(DepartmentCreateDto createDto)
        {
            var department = await _departmentService.CreateDepartmentAsync(createDto);
            return CreatedAtAction(nameof(GetDepartment), new { id = department.Id }, department);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<DepartmentResponseDto>> UpdateDepartment(int id, DepartmentUpdateDto updateDto)
        {
            if (id != updateDto.Id)
            {
                return BadRequest();
            }

            var department = await _departmentService.UpdateDepartmentAsync(updateDto);
            if (department == null)
            {
                return NotFound();
            }
            return Ok(department);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var result = await _departmentService.DeleteDepartmentAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpGet("{id}/employee-count")]
        [Authorize] 
        public async Task<ActionResult<int>> GetEmployeeCountByDepartment(int id)
        {
            var count = await _departmentService.GetEmployeeCountByDepartmentAsync(id);
            return Ok(count);
        }
    }
}