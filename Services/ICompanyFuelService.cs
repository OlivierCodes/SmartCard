using SmartCard.DTOs;
using SmartCard.Models;

namespace SmartCard.Services
{
    public interface ICompanyFuelService
    {
        Task<CompanyFuelSummaryDto> GetFuelSummaryAsync();
        Task<IEnumerable<CompanyFuelSupply>> GetAllSuppliesAsync();
        Task<CompanyFuelSupply?> GetSupplyByIdAsync(int id);
        Task<CompanyFuelSupply> AddSupplyAsync(CompanyFuelSupplyCreateDto dto, string? createdBy = null);
        Task<CompanyFuelSupply> UpdateSupplyAsync(CompanyFuelSupplyUpdateDto dto);
        Task<bool> DeleteSupplyAsync(int id);
        Task<decimal> GetRemainingCompanyStockAsync();
    }
}
