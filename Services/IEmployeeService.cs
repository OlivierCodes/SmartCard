using SmartCard.DTOs;

namespace SmartCard.Services
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeResponseDto>> GetAllEmployeesAsync();
        Task<EmployeeResponseDto?> GetEmployeeByIdAsync(int id);
        Task<EmployeeResponseDto?> GetEmployeeByNumberAsync(string employeeNumber);
        Task<EmployeeResponseDto> CreateEmployeeAsync(EmployeeCreateDto createDto);
        Task<EmployeeResponseDto?> UpdateEmployeeAsync(EmployeeUpdateDto updateDto);
        Task<bool> DeleteEmployeeAsync(int id);
        Task<bool> AssignCardToEmployeeAsync(int employeeId, string cardNumber);
        Task<int> GetEmployeeCountAsync();
    }
}