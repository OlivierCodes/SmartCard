using Microsoft.EntityFrameworkCore;
using SmartCard.Data;
using SmartCard.DTOs;
using SmartCard.Models;

namespace SmartCard.Services
{
    public class FuelQuotaService : IFuelQuotaService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FuelQuotaService> _logger;

        public FuelQuotaService(ApplicationDbContext context, ILogger<FuelQuotaService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<FuelQuota>> GetQuotasByEmployeeAsync(int employeeId)
        {
            try
            {
                var quotas = await _context.FuelQuotas
                    .Where(fq => fq.EmployeeId == employeeId)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} fuel quotas for employee {EmployeeId}", quotas.Count, employeeId);
                return quotas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving fuel quotas for employee {EmployeeId}", employeeId);
                throw;
            }
        }

        public async Task<FuelQuota?> GetQuotaByMonthAsync(int employeeId, int month, int year)
        {
            try
            {
                var quota = await _context.FuelQuotas
                    .FirstOrDefaultAsync(fq => fq.EmployeeId == employeeId && fq.Month == month && fq.Year == year);

                if (quota == null)
                {
                    _logger.LogWarning("Fuel quota not found for employee {EmployeeId} in month {Month} year {Year}", employeeId, month, year);
                    return null;
                }

                _logger.LogInformation("Retrieved fuel quota for employee {EmployeeId} in month {Month} year {Year}", employeeId, month, year);
                return quota;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving fuel quota for employee {EmployeeId} in month {Month} year {Year}", employeeId, month, year);
                throw;
            }
        }

        public async Task<FuelQuota> UpdateQuotaAsync(FuelQuotaUpdateDto updateDto)
        {
            try
            {
                var quota = await _context.FuelQuotas.FirstOrDefaultAsync(fq => fq.Id == updateDto.Id);

                if (quota == null)
                {
                    _logger.LogWarning("Fuel quota with ID {Id} not found for update", updateDto.Id);
                    throw new ArgumentException($"Fuel quota with ID {updateDto.Id} not found");
                }

                // Prevent updating to used liters that exceed quota
                if (updateDto.UsedLiters > quota.QuotaInLiters)
                {
                    _logger.LogWarning("Attempt to set used liters {UsedLiters} exceeding quota {QuotaInLiters}", updateDto.UsedLiters, quota.QuotaInLiters);
                    throw new ArgumentException("Used liters cannot exceed quota liters");
                }

                quota.QuotaInLiters = updateDto.QuotaInLiters;
                quota.UsedLiters = updateDto.UsedLiters;
                quota.UpdatedDate = DateTime.UtcNow;

                _context.FuelQuotas.Update(quota);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated fuel quota with ID {Id}", quota.Id);
                return quota;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating fuel quota with ID {Id}", updateDto.Id);
                throw;
            }
        }

        public async Task<decimal> GetAvailableFuelAsync(int employeeId, int month, int year)
        {
            try
            {
                var quota = await _context.FuelQuotas
                    .FirstOrDefaultAsync(fq => fq.EmployeeId == employeeId && fq.Month == month && fq.Year == year);

                if (quota == null)
                {
                    _logger.LogWarning("Fuel quota not found for employee {EmployeeId} in month {Month} year {Year}", employeeId, month, year);
                    return 0;
                }

                var availableFuel = quota.AvailableLiters;
                _logger.LogInformation("Retrieved available fuel {AvailableFuel}L for employee {EmployeeId} in month {Month} year {Year}", availableFuel, employeeId, month, year);
                return availableFuel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available fuel for employee {EmployeeId} in month {Month} year {Year}", employeeId, month, year);
                throw;
            }
        }

        public async Task<decimal> GetTotalAvailableFuelAsync()
        {
            try
            {
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                var quotas = await _context.FuelQuotas
                    .Where(fq => fq.Month == currentMonth && fq.Year == currentYear)
                    .ToListAsync();

                var totalAvailable = quotas.Sum(q => q.AvailableLiters);
                _logger.LogInformation("Retrieved total available fuel {TotalAvailable}L for current month {Month} year {Year}", totalAvailable, currentMonth, currentYear);
                return totalAvailable;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total available fuel");
                throw;
            }
        }

        public async Task<FuelQuota> CreateFuelQuotaAsync(FuelQuotaCreateDto createDto)
        {
            try
            {
                // Check if a quota already exists for this employee in this month/year
                var existingQuota = await _context.FuelQuotas
                    .FirstOrDefaultAsync(fq => fq.EmployeeId == createDto.EmployeeId && 
                                            fq.Month == createDto.Month && 
                                            fq.Year == createDto.Year);

                if (existingQuota != null)
                {
                    _logger.LogWarning("Fuel quota already exists for employee {EmployeeId} in month {Month} year {Year}", createDto.EmployeeId, createDto.Month, createDto.Year);
                    throw new ArgumentException($"Fuel quota already exists for employee {createDto.EmployeeId} in month {createDto.Month} year {createDto.Year}");
                }

                var fuelQuota = new FuelQuota
                {
                    EmployeeId = createDto.EmployeeId,
                    Month = createDto.Month,
                    Year = createDto.Year,
                    QuotaInLiters = createDto.QuotaInLiters,
                    UsedLiters = createDto.UsedLiters,
                    CreatedDate = DateTime.UtcNow
                };

                _context.FuelQuotas.Add(fuelQuota);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created fuel quota with ID {Id} for employee {EmployeeId}", fuelQuota.Id, fuelQuota.EmployeeId);
                return fuelQuota;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating fuel quota for employee {EmployeeId}", createDto.EmployeeId);
                throw;
            }
        }

        public async Task<bool> DeleteFuelQuotaAsync(int id)
        {
            try
            {
                var quota = await _context.FuelQuotas.FindAsync(id);
                if (quota == null)
                {
                    _logger.LogWarning("Fuel quota with ID {Id} not found for deletion", id);
                    return false;
                }

                _context.FuelQuotas.Remove(quota);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted fuel quota with ID {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting fuel quota with ID {Id}", id);
                throw;
            }
        }
    }
}