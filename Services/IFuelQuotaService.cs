using SmartCard.DTOs;
using SmartCard.Models;

namespace SmartCard.Services
{
    public interface IFuelQuotaService
    {
        Task<IEnumerable<FuelQuota>> GetQuotasByEmployeeAsync(int employeeId);
        Task<FuelQuota?> GetQuotaByMonthAsync(int employeeId, int month, int year);
        Task<FuelQuota> UpdateQuotaAsync(FuelQuotaUpdateDto updateDto);
        Task<decimal> GetAvailableFuelAsync(int employeeId, int month, int year);
        Task<decimal> GetTotalAvailableFuelAsync();
        Task<FuelQuota> CreateFuelQuotaAsync(FuelQuotaCreateDto createDto);
        Task<bool> DeleteFuelQuotaAsync(int id);
    }
}