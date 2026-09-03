using SmartCard.DTOs;

namespace SmartCard.Services
{
    public interface IDepartmentService
    {
        Task<IEnumerable<DepartmentResponseDto>> GetAllDepartmentsAsync();
        Task<DepartmentResponseDto?> GetDepartmentByIdAsync(int id);
        Task<DepartmentResponseDto> CreateDepartmentAsync(DepartmentCreateDto createDto);
        Task<DepartmentResponseDto?> UpdateDepartmentAsync(DepartmentUpdateDto updateDto);
        Task<bool> DeleteDepartmentAsync(int id);
        Task<int> GetEmployeeCountByDepartmentAsync(int departmentId);
    }
}