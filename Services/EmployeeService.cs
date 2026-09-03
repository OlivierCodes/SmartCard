using Microsoft.EntityFrameworkCore;
using SmartCard.Data;
using SmartCard.DTOs;
using SmartCard.Exceptions;
using SmartCard.Models;

namespace SmartCard.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(ApplicationDbContext context, ILogger<EmployeeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<EmployeeResponseDto>> GetAllEmployeesAsync()
        {
            try
            {
                var employees = await _context.Employees
                    .Include(e => e.Cards)
                    .Include(e => e.FuelQuotas)
                    .Include(e => e.Department)
                    .ToListAsync();

                var employeeDtos = new List<EmployeeResponseDto>();

                foreach (var emp in employees)
                {
                    var currentMonth = DateTime.Now.Month;
                    var currentYear = DateTime.Now.Year;

                    var quota = await _context.FuelQuotas
                        .FirstOrDefaultAsync(fq => fq.EmployeeId == emp.Id && fq.Month == currentMonth && fq.Year == currentYear);

                    var availableFuel = quota != null ? quota.QuotaInLiters - quota.UsedLiters : 0;

                    var employeeDto = new EmployeeResponseDto
                    {
                        Id = emp.Id,
                        EmployeeNumber = emp.EmployeeNumber,
                        FirstName = emp.FirstName,
                        LastName = emp.LastName,
                        Email = emp.Email,
                        Department = emp.Department != null ? emp.Department.Name : null,
                        DepartmentId = emp.DepartmentId,
                        MonthlyFuelQuota = emp.MonthlyFuelQuota,
                        AvailableFuelQuota = availableFuel,
                        Cards = emp.Cards.Select(c => new CardResponseDto
                        {
                            Id = c.Id,
                            CardNumber = c.CardNumber,
                            Status = MapCardStatusToDto(c.Status),
                            CreatedDate = c.CreatedDate,
                            LastUsedDate = c.LastUsedDate
                        }).ToList(),
                        CreatedDate = emp.CreatedDate,
                        UpdatedDate = emp.UpdatedDate,
                        IsActive = emp.IsActive
                    };

                    employeeDtos.Add(employeeDto);
                }

                _logger.LogInformation("Retrieved {Count} employees", employeeDtos.Count);
                return employeeDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all employees");
                throw;
            }
        }

        public async Task<EmployeeResponseDto?> GetEmployeeByIdAsync(int id)
        {
            try
            {
                var employee = await _context.Employees
                    .Include(e => e.Cards)
                    .Include(e => e.FuelQuotas)
                    .Include(e => e.Department)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (employee == null)
                {
                    _logger.LogWarning("Employee with ID {Id} not found", id);
                    return null;
                }

                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                var quota = await _context.FuelQuotas
                    .FirstOrDefaultAsync(fq => fq.EmployeeId == employee.Id && fq.Month == currentMonth && fq.Year == currentYear);

                var availableFuel = quota != null ? quota.QuotaInLiters - quota.UsedLiters : 0;

                _logger.LogInformation("Retrieved employee with ID {Id}", id);
                return new EmployeeResponseDto
                {
                    Id = employee.Id,
                    EmployeeNumber = employee.EmployeeNumber,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Email = employee.Email,
                    Department = employee.Department != null ? employee.Department.Name : null,
                    DepartmentId = employee.DepartmentId,
                    MonthlyFuelQuota = employee.MonthlyFuelQuota,
                    AvailableFuelQuota = availableFuel,
                    Cards = employee.Cards.Select(c => new CardResponseDto
                    {
                        Id = c.Id,
                        CardNumber = c.CardNumber,
                        Status = MapCardStatusToDto(c.Status),
                        CreatedDate = c.CreatedDate,
                        LastUsedDate = c.LastUsedDate
                    }).ToList(),
                    CreatedDate = employee.CreatedDate,
                    UpdatedDate = employee.UpdatedDate,
                    IsActive = employee.IsActive
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee with ID {Id}", id);
                throw;
            }
        }

        public async Task<EmployeeResponseDto?> GetEmployeeByNumberAsync(string employeeNumber)
        {
            try
            {
                var employee = await _context.Employees
                    .Include(e => e.Cards)
                    .Include(e => e.FuelQuotas)
                    .Include(e => e.Department)
                    .FirstOrDefaultAsync(e => e.EmployeeNumber == employeeNumber);

                if (employee == null)
                {
                    _logger.LogWarning("Employee with number {EmployeeNumber} not found", employeeNumber);
                    return null;
                }

                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                var quota = await _context.FuelQuotas
                    .FirstOrDefaultAsync(fq => fq.EmployeeId == employee.Id && fq.Month == currentMonth && fq.Year == currentYear);

                var availableFuel = quota != null ? quota.QuotaInLiters - quota.UsedLiters : 0;

                _logger.LogInformation("Retrieved employee with number {EmployeeNumber}", employeeNumber);
                return new EmployeeResponseDto
                {
                    Id = employee.Id,
                    EmployeeNumber = employee.EmployeeNumber,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Email = employee.Email,
                    Department = employee.Department != null ? employee.Department.Name : null,
                    DepartmentId = employee.DepartmentId,
                    MonthlyFuelQuota = employee.MonthlyFuelQuota,
                    AvailableFuelQuota = availableFuel,
                    Cards = employee.Cards.Select(c => new CardResponseDto
                    {
                        Id = c.Id,
                        CardNumber = c.CardNumber,
                        Status = MapCardStatusToDto(c.Status),
                        CreatedDate = c.CreatedDate,
                        LastUsedDate = c.LastUsedDate
                    }).ToList(),
                    CreatedDate = employee.CreatedDate,
                    UpdatedDate = employee.UpdatedDate,
                    IsActive = employee.IsActive
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee with number {EmployeeNumber}", employeeNumber);
                throw;
            }
        }

        public async Task<EmployeeResponseDto> CreateEmployeeAsync(EmployeeCreateDto createDto)
        {
            try
            {
                // Generate unique 6-digit employee number
                string employeeNumber = await GenerateUniqueEmployeeNumberAsync();

                var employee = new Employee
                {
                    EmployeeNumber = employeeNumber,
                    FirstName = createDto.FirstName,
                    LastName = createDto.LastName,
                    Email = createDto.Email,
                    DepartmentId = createDto.DepartmentId,
                    MonthlyFuelQuota = createDto.MonthlyFuelQuota,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                // Create initial fuel quota for current month
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                var fuelQuota = new FuelQuota
                {
                    EmployeeId = employee.Id,
                    Month = currentMonth,
                    Year = currentYear,
                    QuotaInLiters = employee.MonthlyFuelQuota,
                    CreatedDate = DateTime.UtcNow
                };

                _context.FuelQuotas.Add(fuelQuota);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new employee with ID {Id} and number {EmployeeNumber}", employee.Id, employee.EmployeeNumber);
                return new EmployeeResponseDto
                {
                    Id = employee.Id,
                    EmployeeNumber = employee.EmployeeNumber,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Email = employee.Email,
                    Department = employee.Department?.Name,
                    DepartmentId = employee.DepartmentId,
                    MonthlyFuelQuota = employee.MonthlyFuelQuota,
                    AvailableFuelQuota = employee.MonthlyFuelQuota,
                    Cards = new List<CardResponseDto>(),
                    CreatedDate = employee.CreatedDate,
                    UpdatedDate = employee.UpdatedDate,
                    IsActive = employee.IsActive
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating employee");
                throw;
            }
        }

        private async Task<string> GenerateUniqueEmployeeNumberAsync()
        {
            var random = new Random();
            string employeeNumber;
            bool exists;

            do
            {
                // Generate a random 6-digit number string
                employeeNumber = random.Next(100000, 1000000).ToString();
                exists = await _context.Employees.AnyAsync(e => e.EmployeeNumber == employeeNumber);
            } while (exists);

            return employeeNumber;
        }

        public async Task<EmployeeResponseDto?> UpdateEmployeeAsync(EmployeeUpdateDto updateDto)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(updateDto.Id);
                if (employee == null)
                {
                    _logger.LogWarning("Employee with ID {Id} not found for update", updateDto.Id);
                    return null;
                }

                // If EmployeeNumber is provided, check for uniqueness (excluding current employee)
                if (!string.IsNullOrEmpty(updateDto.EmployeeNumber))
                {
                    var existingEmployee = await _context.Employees
                        .FirstOrDefaultAsync(e => e.EmployeeNumber == updateDto.EmployeeNumber && e.Id != updateDto.Id);

                    if (existingEmployee != null)
                    {
                        throw new ArgumentException($"Employee with number {updateDto.EmployeeNumber} already exists");
                    }
                    employee.EmployeeNumber = updateDto.EmployeeNumber;
                }

                employee.FirstName = updateDto.FirstName;
                employee.LastName = updateDto.LastName;
                employee.Email = updateDto.Email;
                employee.DepartmentId = updateDto.DepartmentId;
                employee.MonthlyFuelQuota = updateDto.MonthlyFuelQuota;
                employee.UpdatedDate = DateTime.UtcNow;

                _context.Employees.Update(employee);
                await _context.SaveChangesAsync();

                // Update current month's fuel quota if it exists
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                var existingQuota = await _context.FuelQuotas
                    .FirstOrDefaultAsync(fq => fq.EmployeeId == employee.Id && fq.Month == currentMonth && fq.Year == currentYear);

                if (existingQuota != null)
                {
                    existingQuota.QuotaInLiters = employee.MonthlyFuelQuota;
                    existingQuota.UpdatedDate = DateTime.UtcNow;
                    _context.FuelQuotas.Update(existingQuota);
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("Updated employee with ID {Id}", updateDto.Id);
                return new EmployeeResponseDto
                {
                    Id = employee.Id,
                    EmployeeNumber = employee.EmployeeNumber,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Email = employee.Email,
                    Department = employee.Department?.Name,
                    DepartmentId = employee.DepartmentId,
                    MonthlyFuelQuota = employee.MonthlyFuelQuota,
                    AvailableFuelQuota = employee.MonthlyFuelQuota - (existingQuota?.UsedLiters ?? 0),
                    Cards = employee.Cards.Select(c => new CardResponseDto
                    {
                        Id = c.Id,
                        CardNumber = c.CardNumber,
                        Status = MapCardStatusToDto(c.Status),
                        CreatedDate = c.CreatedDate,
                        LastUsedDate = c.LastUsedDate
                    }).ToList(),
                    CreatedDate = employee.CreatedDate,
                    UpdatedDate = employee.UpdatedDate,
                    IsActive = employee.IsActive
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating employee with ID {Id}", updateDto.Id);
                throw;
            }
        }

        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var employee = await _context.Employees
                        .Include(e => e.FuelQuotas)
                        .Include(e => e.Cards)
                        .Include(e => e.Consumptions)
                        .FirstOrDefaultAsync(e => e.Id == id);

                    if (employee == null)
                    {
                        _logger.LogWarning("Employee with ID {Id} not found for deletion", id);
                        return false;
                    }

                    // Delete related FuelQuotas first (due to cascade relationship)
                    if (employee.FuelQuotas.Any())
                    {
                        _context.FuelQuotas.RemoveRange(employee.FuelQuotas);
                        await _context.SaveChangesAsync();
                    }

                    // Update related Cards to have null EmployeeId (due to SetNull relationship)
                    if (employee.Cards.Any())
                    {
                        foreach (var card in employee.Cards)
                        {
                            card.EmployeeId = null;
                            card.Status = CardStatus.Inactive; // Mark as inactive instead of deleting
                            _context.Cards.Update(card);
                        }
                        await _context.SaveChangesAsync();
                    }

                    // Remove associated consumptions before deleting the employee
                    if (employee.Consumptions.Any())
                    {
                        _context.Consumptions.RemoveRange(employee.Consumptions);
                        await _context.SaveChangesAsync();
                    }

                    // Now remove the employee
                    _context.Employees.Remove(employee);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    _logger.LogInformation("Deleted employee with ID {Id}", id);
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting employee with ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> AssignCardToEmployeeAsync(int employeeId, string cardNumber)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(employeeId);
                if (employee == null)
                {
                    _logger.LogWarning("Employee with ID {Id} not found for card assignment", employeeId);
                    return false;
                }

                var card = await _context.Cards.FirstOrDefaultAsync(c => c.CardNumber == cardNumber);
                if (card == null)
                {
                    _logger.LogWarning("Card with number {CardNumber} not found for assignment", cardNumber);
                    return false;
                }

                card.EmployeeId = employeeId;
                card.Status = CardStatus.Active;
                _context.Cards.Update(card);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Assigned card {CardNumber} to employee {EmployeeId}", cardNumber, employeeId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning card {CardNumber} to employee {EmployeeId}", cardNumber, employeeId);
                throw;
            }
        }

        public async Task<int> GetEmployeeCountAsync()
        {
            try
            {
                // Pour s'assurer qu'on compte tous les employés, y compris les inactifs
                var count = await _context.Employees.CountAsync();
                _logger.LogInformation("Retrieved employee count: {Count}", count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee count");
                throw;
            }
        }

        private CardStatusDto MapCardStatusToDto(CardStatus modelStatus)
        {
            return modelStatus switch
            {
                CardStatus.Active => CardStatusDto.Active,
                CardStatus.Inactive => CardStatusDto.Inactive,
                CardStatus.Suspended => CardStatusDto.Suspended,
                CardStatus.Lost => CardStatusDto.Lost,
                _ => CardStatusDto.Inactive
            };
        }
    }
}