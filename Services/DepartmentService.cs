using Microsoft.EntityFrameworkCore;
using SmartCard.Data;
using SmartCard.DTOs;
using SmartCard.Models;

namespace SmartCard.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DepartmentService> _logger;

        public DepartmentService(ApplicationDbContext context, ILogger<DepartmentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<DepartmentResponseDto>> GetAllDepartmentsAsync()
        {
            try
            {
                // Optimized query: Get departments with their employee counts in one query
                var departmentCounts = await _context.Employees
                    .Where(e => e.IsActive)
                    .GroupBy(e => e.DepartmentId)
                    .Select(g => new { DepartmentId = g.Key, Count = g.Count() })
                    .ToListAsync();

                var departments = await _context.Departments
                    .Where(d => d.IsActive)
                    .ToListAsync();

                var departmentDtos = departments.Select(d => new DepartmentResponseDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    EmployeeCount = departmentCounts.FirstOrDefault(dc => dc.DepartmentId == d.Id)?.Count ?? 0,
                    CreatedDate = d.CreatedDate,
                    UpdatedDate = d.UpdatedDate,
                    IsActive = d.IsActive
                }).ToList();

                _logger.LogInformation("Retrieved {Count} departments", departmentDtos.Count);
                return departmentDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all departments");
                throw;
            }
        }

        public async Task<DepartmentResponseDto?> GetDepartmentByIdAsync(int id)
        {
            try
            {
                var department = await _context.Departments
                    .Where(d => d.Id == id && d.IsActive)
                    .FirstOrDefaultAsync();

                if (department == null)
                {
                    _logger.LogWarning("Department with ID {Id} not found", id);
                    return null;
                }

                // Calculate employee count for this department
                var employeeCount = await _context.Employees
                    .Where(e => e.DepartmentId == department.Id && e.IsActive)
                    .CountAsync();

                var departmentDto = new DepartmentResponseDto
                {
                    Id = department.Id,
                    Name = department.Name,
                    EmployeeCount = employeeCount,
                    CreatedDate = department.CreatedDate,
                    UpdatedDate = department.UpdatedDate,
                    IsActive = department.IsActive
                };

                _logger.LogInformation("Retrieved department with ID {Id}", id);
                return departmentDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving department with ID {Id}", id);
                throw;
            }
        }

        public async Task<DepartmentResponseDto> CreateDepartmentAsync(DepartmentCreateDto createDto)
        {
            try
            {
                // Check if department with the same name already exists (case insensitive)
                var existingDepartment = await _context.Departments
                    .FirstOrDefaultAsync(d => d.Name.ToLower() == createDto.Name.ToLower() && d.IsActive);

                if (existingDepartment != null)
                {
                    _logger.LogWarning("Department with name {Name} already exists", createDto.Name);
                    throw new ArgumentException($"Department with name {createDto.Name} already exists");
                }

                var department = new Department
                {
                    Name = createDto.Name,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Departments.Add(department);
                await _context.SaveChangesAsync();

                var departmentDto = new DepartmentResponseDto
                {
                    Id = department.Id,
                    Name = department.Name,
                    CreatedDate = department.CreatedDate,
                    UpdatedDate = department.UpdatedDate,
                    IsActive = department.IsActive
                };

                _logger.LogInformation("Created department with ID {Id} and name {Name}", department.Id, department.Name);
                return departmentDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating department with name {Name}", createDto.Name);
                throw;
            }
        }

        public async Task<DepartmentResponseDto?> UpdateDepartmentAsync(DepartmentUpdateDto updateDto)
        {
            try
            {
                var department = await _context.Departments.FindAsync(updateDto.Id);
                if (department == null)
                {
                    _logger.LogWarning("Department with ID {Id} not found for update", updateDto.Id);
                    return null;
                }

                // Check if department with the same name already exists (case insensitive, excluding current department)
                var existingDepartment = await _context.Departments
                    .FirstOrDefaultAsync(d => d.Name.ToLower() == updateDto.Name.ToLower() && 
                                            d.Id != updateDto.Id && d.IsActive);

                if (existingDepartment != null)
                {
                    _logger.LogWarning("Department with name {Name} already exists", updateDto.Name);
                    throw new ArgumentException($"Department with name {updateDto.Name} already exists");
                }

                department.Name = updateDto.Name;
                department.UpdatedDate = DateTime.UtcNow;

                _context.Departments.Update(department);
                await _context.SaveChangesAsync();

                var departmentDto = new DepartmentResponseDto
                {
                    Id = department.Id,
                    Name = department.Name,
                    CreatedDate = department.CreatedDate,
                    UpdatedDate = department.UpdatedDate,
                    IsActive = department.IsActive
                };

                _logger.LogInformation("Updated department with ID {Id}", department.Id);
                return departmentDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating department with ID {Id}", updateDto.Id);
                throw;
            }
        }

        public async Task<bool> DeleteDepartmentAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var department = await _context.Departments.FindAsync(id);
                if (department == null)
                {
                    _logger.LogWarning("Department with ID {Id} not found for deletion", id);
                    return false;
                }

                // Réaffecter les employés du département à null (ou un département par défaut)
                var employees = await _context.Employees.Where(e => e.DepartmentId == id).ToListAsync();
                foreach (var employee in employees)
                {
                    employee.DepartmentId = null; // Désaffecter de ce département
                    _context.Employees.Update(employee);
                }

                if (employees.Any())
                {
                    await _context.SaveChangesAsync();
                }

                // Procéder avec la suppression du département
                department.IsActive = false; // Soft delete
                department.UpdatedDate = DateTime.UtcNow;
                _context.Departments.Update(department);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Soft deleted department with ID {Id} and reassigned {Count} employees", id, employees.Count);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting department with ID {Id}", id);
                throw;
            }
        }

        public async Task<int> GetEmployeeCountByDepartmentAsync(int departmentId)
        {
            try
            {
                var count = await _context.Employees
                    .Where(e => e.DepartmentId == departmentId && e.IsActive)
                    .CountAsync();

                _logger.LogInformation("Retrieved employee count {Count} for department {DepartmentId}", count, departmentId);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee count for department {DepartmentId}", departmentId);
                throw;
            }
        }
    }
}