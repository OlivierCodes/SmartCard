using Microsoft.AspNetCore.Mvc;
using SmartCard.Data;
using SmartCard.DTOs;
using SmartCard.Services;

namespace SmartCard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;
        private readonly IEmployeeService _employeeService;
        private readonly ApplicationDbContext _context;

        public TestController(IDepartmentService departmentService, IEmployeeService employeeService, ApplicationDbContext context)
        {
            _departmentService = departmentService;
            _employeeService = employeeService;
            _context = context;
        }

        [HttpGet("test-department-creation")]
        public async Task<ActionResult<string>> TestDepartmentCreation()
        {
            try
            {
                var departmentDto = new DepartmentCreateDto
                {
                    Name = "Test Department"
                };

                var result = await _departmentService.CreateDepartmentAsync(departmentDto);
                return Ok($"Department created successfully with ID: {result.Id}, Name: {result.Name}");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating department: {ex.Message}");
            }
        }

        [HttpGet("test-employee-creation")]
        public async Task<ActionResult<string>> TestEmployeeCreation()
        {
            try
            {
                // First create a department for the employee
                var department = new DepartmentCreateDto { Name = "Test Dept for Employee" };
                var deptResult = await _departmentService.CreateDepartmentAsync(department);
                
                var employeeDto = new EmployeeCreateDto
                {
                    EmployeeNumber = "TEST001",
                    FirstName = "Test",
                    LastName = "Employee",
                    Email = "test@example.com",
                    DepartmentId = deptResult.Id,
                    MonthlyFuelQuota = 100
                };

                var result = await _employeeService.CreateEmployeeAsync(employeeDto);
                return Ok($"Employee created successfully with ID: {result.Id}, Name: {result.FirstName} {result.LastName}");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating employee: {ex.Message}");
            }
        }

        [HttpGet("check-db")]
        public ActionResult<string> CheckDatabase()
        {
            try
            {
                var deptCount = _context.Departments.Count();
                var empCount = _context.Employees.Count();
                
                return Ok($"Database connection OK. Departments: {deptCount}, Employees: {empCount}");
            }
            catch (Exception ex)
            {
                return BadRequest($"Database connection failed: {ex.Message}");
            }
        }
    }
}