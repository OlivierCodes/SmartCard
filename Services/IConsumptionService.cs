using SmartCard.DTOs;
using SmartCard.Models;

namespace SmartCard.Services
{
    public interface IConsumptionService
    {
        Task<IEnumerable<Consumption>> GetConsumptionsByEmployeeAsync(int employeeId);
        Task<IEnumerable<Consumption>> GetConsumptionsByPeriodAsync(int employeeId, DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalConsumptionByEmployeeAsync(int employeeId);
        Task<decimal> GetTotalConsumptionByPeriodAsync(int employeeId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<ConsumptionByPeriodDto>> GetConsumptionsByPeriodGroupedAsync(int employeeId, DateTime startDate, DateTime endDate, string groupBy);
        Task<IEnumerable<ConsumptionByPeriodDto>> GetConsumptionsForAllEmployeesByPeriodGroupedAsync(DateTime startDate, DateTime endDate, string groupBy);
        Task<decimal> GetTotalConsumptionForAllEmployeesByPeriodAsync(DateTime startDate, DateTime endDate);
        Task<Consumption> CreateConsumptionAsync(ConsumptionCreateDto createDto);
        Task<bool> DeleteConsumptionAsync(int id);
        Task<IEnumerable<MobileConsumptionHistoryDto>> GetConsumptionHistoryForMobileAsync(int employeeId);
        Task<IEnumerable<MobileConsumptionHistoryDto>> GetAllEmployeesConsumptionHistoryForMobileAsync();
        Task<decimal> GetTotalAvailableQuotaForAllEmployeesAsync();
        Task<decimal> GetTotalConsumedQuotaForAllEmployeesAsync();
        Task<IEnumerable<ConsumptionByPeriodDto>> GetConsumptionsForAllEmployeesByPeriodGroupedForMobileAsync(DateTime startDate, DateTime endDate, string groupBy = "day");
    }
}