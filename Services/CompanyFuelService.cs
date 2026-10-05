using Microsoft.EntityFrameworkCore;
using SmartCard.Data;
using SmartCard.DTOs;
using SmartCard.Models;

namespace SmartCard.Services
{
    public class CompanyFuelService : ICompanyFuelService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CompanyFuelService> _logger;

        public CompanyFuelService(ApplicationDbContext context, ILogger<CompanyFuelService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CompanyFuelSummaryDto> GetFuelSummaryAsync()
        {
            try
            {
                // Total approvisionné au niveau de l'entreprise
                var totalSupplied = await _context.CompanyFuelSupplies
                    .SumAsync(s => (decimal?)s.QuantityInLiters) ?? 0m;

                // Total alloué aux employés via les quotas
                var totalAllocated = await _context.FuelQuotas
                    .SumAsync(q => (decimal?)q.QuotaInLiters) ?? 0m;

                // Total consommé à la pompe
                var totalConsumed = await _context.Consumptions
                    .SumAsync(c => (decimal?)c.AmountInLiters) ?? 0m;

                var suppliesCount = await _context.CompanyFuelSupplies.CountAsync();

                // Ce qui reste dans la réserve de l'entreprise (non distribué)
                var remainingCompanyStock = totalSupplied - totalAllocated;

                // Ce qui est actuellement à la disposition des employés (distribué mais pas encore consommé)
                var availableForEmployees = totalAllocated - totalConsumed;

                // Stock physique total restant dans l'entreprise
                var remainingPhysicalStock = totalSupplied - totalConsumed;

                return new CompanyFuelSummaryDto
                {
                    TotalSuppliedLiters = totalSupplied,
                    TotalAllocatedLiters = totalAllocated,
                    TotalConsumedLiters = totalConsumed,
                    RemainingCompanyStockLiters = remainingCompanyStock,
                    AvailableForEmployeesLiters = availableForEmployees,
                    RemainingPhysicalStockLiters = remainingPhysicalStock,
                    TotalSuppliesCount = suppliesCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du calcul du résumé du stock de carburant");
                throw;
            }
        }

        public async Task<IEnumerable<CompanyFuelSupply>> GetAllSuppliesAsync()
        {
            try
            {
                return await _context.CompanyFuelSupplies
                    .OrderByDescending(s => s.SupplyDate)
                    .ThenByDescending(s => s.CreatedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des approvisionnements");
                throw;
            }
        }

        public async Task<CompanyFuelSupply?> GetSupplyByIdAsync(int id)
        {
            try
            {
                return await _context.CompanyFuelSupplies.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'approvisionnement {Id}", id);
                throw;
            }
        }

        public async Task<CompanyFuelSupply> AddSupplyAsync(CompanyFuelSupplyCreateDto dto, string? createdBy = null)
        {
            if (dto.QuantityInLiters <= 0)
            {
                throw new ArgumentException("La quantité de carburant doit être supérieure à zéro.");
            }

            try
            {
                var supply = new CompanyFuelSupply
                {
                    QuantityInLiters = dto.QuantityInLiters,
                    SupplyDate = dto.SupplyDate ?? DateTime.UtcNow,
                    Supplier = dto.Supplier?.Trim(),
                    ReferenceNumber = dto.ReferenceNumber?.Trim(),
                    Notes = dto.Notes?.Trim(),
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.UtcNow
                };

                _context.CompanyFuelSupplies.Add(supply);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Nouvel approvisionnement enregistré : {Quantity}L par {CreatedBy}", supply.QuantityInLiters, createdBy ?? "Inconnu");
                return supply;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'enregistrement de l'approvisionnement");
                throw;
            }
        }

        public async Task<CompanyFuelSupply> UpdateSupplyAsync(CompanyFuelSupplyUpdateDto dto)
        {
            if (dto.QuantityInLiters <= 0)
            {
                throw new ArgumentException("La quantité de carburant doit être supérieure à zéro.");
            }

            try
            {
                var supply = await _context.CompanyFuelSupplies.FindAsync(dto.Id);
                if (supply == null)
                {
                    throw new ArgumentException($"Approvisionnement avec l'identifiant {dto.Id} introuvable.");
                }

                supply.QuantityInLiters = dto.QuantityInLiters;
                if (dto.SupplyDate.HasValue)
                {
                    supply.SupplyDate = dto.SupplyDate.Value;
                }
                supply.Supplier = dto.Supplier?.Trim();
                supply.ReferenceNumber = dto.ReferenceNumber?.Trim();
                supply.Notes = dto.Notes?.Trim();

                _context.CompanyFuelSupplies.Update(supply);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Approvisionnement {Id} mis à jour avec succès", dto.Id);
                return supply;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de l'approvisionnement {Id}", dto.Id);
                throw;
            }
        }

        public async Task<bool> DeleteSupplyAsync(int id)
        {
            try
            {
                var supply = await _context.CompanyFuelSupplies.FindAsync(id);
                if (supply == null)
                {
                    _logger.LogWarning("Approvisionnement avec l'identifiant {Id} introuvable pour suppression.", id);
                    return false;
                }

                _context.CompanyFuelSupplies.Remove(supply);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Approvisionnement {Id} supprimé avec succès", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de l'approvisionnement {Id}", id);
                throw;
            }
        }

        public async Task<decimal> GetRemainingCompanyStockAsync()
        {
            var summary = await GetFuelSummaryAsync();
            return summary.RemainingCompanyStockLiters;
        }
    }
}
