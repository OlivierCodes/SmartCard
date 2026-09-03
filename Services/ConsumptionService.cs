using Microsoft.EntityFrameworkCore;
using SmartCard.Data;
using SmartCard.DTOs;
using SmartCard.Models;

namespace SmartCard.Services
{
    public class ConsumptionService : IConsumptionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ConsumptionService> _logger;

        public ConsumptionService(ApplicationDbContext context, ILogger<ConsumptionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Consumption>> GetConsumptionsByEmployeeAsync(int employeeId)
        {
            try
            {
                var consumptions = await _context.Consumptions
                    .Where(c => c.EmployeeId == employeeId)
                    .OrderByDescending(c => c.TransactionDate)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} consumptions for employee {EmployeeId}", consumptions.Count, employeeId);
                return consumptions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving consumptions for employee {EmployeeId}", employeeId);
                throw;
            }
        }

        public async Task<IEnumerable<Consumption>> GetConsumptionsByPeriodAsync(int employeeId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var endOfPeriod = endDate.Date.AddDays(1);
                var consumptions = await _context.Consumptions
                    .Where(c => c.EmployeeId == employeeId && 
                               c.TransactionDate >= startDate.Date && 
                               c.TransactionDate < endOfPeriod)
                    .OrderByDescending(c => c.TransactionDate)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} consumptions for employee {EmployeeId} in period {StartDate} to {EndDate}", consumptions.Count, employeeId, startDate, endDate);
                return consumptions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving consumptions for employee {EmployeeId} in period {StartDate} to {EndDate}", employeeId, startDate, endDate);
                throw;
            }
        }

        public async Task<decimal> GetTotalConsumptionByEmployeeAsync(int employeeId)
        {
            try
            {
                var total = await _context.Consumptions
                    .Where(c => c.EmployeeId == employeeId)
                    .SumAsync(c => c.AmountInLiters);

                _logger.LogInformation("Retrieved total consumption {Total}L for employee {EmployeeId}", total, employeeId);
                return total;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total consumption for employee {EmployeeId}", employeeId);
                throw;
            }
        }

        public async Task<decimal> GetTotalConsumptionByPeriodAsync(int employeeId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var endOfPeriod = endDate.Date.AddDays(1);
                var total = await _context.Consumptions
                    .Where(c => c.EmployeeId == employeeId && 
                               c.TransactionDate >= startDate.Date && 
                               c.TransactionDate < endOfPeriod)
                    .SumAsync(c => c.AmountInLiters);

                _logger.LogInformation("Retrieved total consumption {Total}L for employee {EmployeeId} in period {StartDate} to {EndDate}", total, employeeId, startDate, endDate);
                return total;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total consumption for employee {EmployeeId} in period {StartDate} to {EndDate}", employeeId, startDate, endDate);
                throw;
            }
        }

        public async Task<IEnumerable<ConsumptionByPeriodDto>> GetConsumptionsByPeriodGroupedAsync(int employeeId, DateTime startDate, DateTime endDate, string groupBy)
        {
            try
            {
                var endOfPeriod = endDate.Date.AddDays(1);
                var consumptions = await _context.Consumptions
                    .Where(c => c.EmployeeId == employeeId && 
                               c.TransactionDate >= startDate.Date && 
                               c.TransactionDate < endOfPeriod)
                    .ToListAsync();

                var grouped = GroupConsumptionsByPeriod(consumptions, groupBy);

                _logger.LogInformation("Retrieved grouped consumptions for employee {EmployeeId} in period {StartDate} to {EndDate}, grouped by {GroupBy}", employeeId, startDate, endDate, groupBy);
                return grouped;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving grouped consumptions for employee {EmployeeId} in period {StartDate} to {EndDate}, grouped by {GroupBy}", employeeId, startDate, endDate, groupBy);
                throw;
            }
        }

        public async Task<IEnumerable<ConsumptionByPeriodDto>> GetConsumptionsForAllEmployeesByPeriodGroupedAsync(DateTime startDate, DateTime endDate, string groupBy)
        {
            try
            {
                var endOfPeriod = endDate.Date.AddDays(1);
                var consumptions = await _context.Consumptions
                    .Where(c => c.TransactionDate >= startDate.Date && 
                               c.TransactionDate < endOfPeriod)
                    .ToListAsync();

                var grouped = GroupConsumptionsByPeriod(consumptions, groupBy);

                _logger.LogInformation("Retrieved grouped consumptions for all employees in period {StartDate} to {EndDate}, grouped by {GroupBy}", startDate, endDate, groupBy);
                return grouped;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving grouped consumptions for all employees in period {StartDate} to {EndDate}, grouped by {GroupBy}", startDate, endDate, groupBy);
                throw;
            }
        }

        public async Task<decimal> GetTotalConsumptionForAllEmployeesByPeriodAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var endOfPeriod = endDate.Date.AddDays(1);
                var total = await _context.Consumptions
                    .Where(c => c.TransactionDate >= startDate.Date && 
                               c.TransactionDate < endOfPeriod)
                    .SumAsync(c => c.AmountInLiters);

                _logger.LogInformation("Retrieved total consumption {Total}L for all employees in period {StartDate} to {EndDate}", total, startDate, endDate);
                return total;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total consumption for all employees in period {StartDate} to {EndDate}", startDate, endDate);
                throw;
            }
        }

        public async Task<Consumption> CreateConsumptionAsync(ConsumptionCreateDto createDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Vérifier à nouveau le quota disponible pour éviter les conflits concurrents
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                var fuelQuota = await _context.FuelQuotas
                    .FirstOrDefaultAsync(fq => fq.EmployeeId == createDto.EmployeeId &&
                                             fq.Month == currentMonth &&
                                             fq.Year == currentYear);

                if (fuelQuota != null)
                {
                    var availableFuel = fuelQuota.QuotaInLiters - fuelQuota.UsedLiters;
                    if (availableFuel < createDto.AmountInLiters)
                    {
                        throw new InvalidOperationException($"Le quota disponible est insuffisant pour ce prélèvement. Quantité demandée: {createDto.AmountInLiters}L, Quota disponible: {availableFuel}L");
                    }
                }
                else
                {
                    // Si aucun quota n'existe, créer un quota avec le quota mensuel égal à la quantité prélevée
                    // ou lever une exception selon la politique métier
                    throw new InvalidOperationException($"Aucun quota n'est défini pour cet employé dans le mois en cours. Impossible de créer un prélèvement.");
                }

                var consumption = new Consumption
                {
                    EmployeeId = createDto.EmployeeId,
                    CardId = createDto.CardId,
                    AmountInLiters = createDto.AmountInLiters,
                    TransactionDate = createDto.TransactionDate,
                    Description = createDto.Description
                };

                _context.Consumptions.Add(consumption);
                await _context.SaveChangesAsync();

                // Mettre à jour le quota utilisé
                fuelQuota.UsedLiters += createDto.AmountInLiters;
                _context.FuelQuotas.Update(fuelQuota);
                await _context.SaveChangesAsync();

                // Mettre à jour la date de dernière utilisation de la carte
                var card = await _context.Cards.FindAsync(createDto.CardId);
                if (card != null)
                {
                    card.LastUsedDate = DateTime.UtcNow;
                    _context.Cards.Update(card);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                _logger.LogInformation("Created consumption with ID {Id} for employee {EmployeeId} and updated used quota", consumption.Id, consumption.EmployeeId);
                return consumption;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating consumption for employee {EmployeeId}", createDto.EmployeeId);
                throw;
            }
        }

        public async Task<bool> DeleteConsumptionAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var consumption = await _context.Consumptions
                    .Include(c => c.Employee)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (consumption == null)
                {
                    _logger.LogWarning("Consumption with ID {Id} not found for deletion", id);
                    return false;
                }

                // Récupérer la quantité à déduire avant la suppression
                var amountToDelete = consumption.AmountInLiters;
                var employeeId = consumption.EmployeeId;

                // Supprimer la consommation
                _context.Consumptions.Remove(consumption);
                await _context.SaveChangesAsync();

                // Réduire le quota utilisé
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                var fuelQuota = await _context.FuelQuotas
                    .FirstOrDefaultAsync(fq => fq.EmployeeId == employeeId &&
                                             fq.Month == currentMonth &&
                                             fq.Year == currentYear);

                if (fuelQuota != null && fuelQuota.UsedLiters >= amountToDelete)
                {
                    fuelQuota.UsedLiters -= amountToDelete;
                    _context.FuelQuotas.Update(fuelQuota);
                    await _context.SaveChangesAsync();
                }

                // Mettre à jour la date de dernière utilisation de la carte
                // On ne met pas à jour LastUsedDate lors de la suppression car cela ne représente pas une nouvelle utilisation
                // La date de dernière utilisation indique quand la carte a été DERNIEREMENT utilisée, pas quand elle a été libérée

                await transaction.CommitAsync();

                _logger.LogInformation("Deleted consumption with ID {Id} and updated used quota", id);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting consumption with ID {Id}", id);
                throw;
            }
        }

        private List<ConsumptionByPeriodDto> GroupConsumptionsByPeriod(List<Consumption> consumptions, string groupBy)
        {
            var grouped = new List<ConsumptionByPeriodDto>();

            if (groupBy.ToLower() == "day")
            {
                var groupedByDay = consumptions
                    .GroupBy(c => new { c.TransactionDate.Date })
                    .Select(g => new ConsumptionByPeriodDto
                    {
                        Period = g.Key.Date.ToString("yyyy-MM-dd"),
                        TotalAmount = g.Sum(c => c.AmountInLiters),
                        TotalConsumption = g.Sum(c => c.AmountInLiters),
                        Count = g.Count()
                    })
                    .OrderBy(dto => DateTime.Parse(dto.Period))
                    .ToList();

                grouped.AddRange(groupedByDay);
            }
            else if (groupBy.ToLower() == "week")
            {
                var groupedByWeek = consumptions
                    .GroupBy(c => GetWeek(c.TransactionDate))
                    .Select(g => new ConsumptionByPeriodDto
                    {
                        Period = g.Key,
                        TotalAmount = g.Sum(c => c.AmountInLiters),
                        TotalConsumption = g.Sum(c => c.AmountInLiters),
                        Count = g.Count()
                    })
                    .OrderBy(dto => dto.Period)
                    .ToList();

                grouped.AddRange(groupedByWeek);
            }
            else if (groupBy.ToLower() == "month")
            {
                var groupedByMonth = consumptions
                    .GroupBy(c => new { Year = c.TransactionDate.Year, Month = c.TransactionDate.Month })
                    .Select(g => new ConsumptionByPeriodDto
                    {
                        Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                        TotalAmount = g.Sum(c => c.AmountInLiters),
                        TotalConsumption = g.Sum(c => c.AmountInLiters),
                        Count = g.Count()
                    })
                    .OrderBy(dto => dto.Period)
                    .ToList();

                grouped.AddRange(groupedByMonth);
            }
            else // Default to day
            {
                var groupedByDay = consumptions
                    .GroupBy(c => new { c.TransactionDate.Date })
                    .Select(g => new ConsumptionByPeriodDto
                    {
                        Period = g.Key.Date.ToString("yyyy-MM-dd"),
                        TotalAmount = g.Sum(c => c.AmountInLiters),
                        TotalConsumption = g.Sum(c => c.AmountInLiters),
                        Count = g.Count()
                    })
                    .OrderBy(dto => DateTime.Parse(dto.Period))
                    .ToList();

                grouped.AddRange(groupedByDay);
            }

            return grouped;
        }

        private string GetWeek(DateTime date)
        {
            var start = date.Date;
            while (start.DayOfWeek != DayOfWeek.Monday)
            {
                start = start.AddDays(-1);
            }
            var end = start.AddDays(6);
            return $"{start:yyyy-MM-dd} to {end:yyyy-MM-dd}";
        }

        public async Task<IEnumerable<MobileConsumptionHistoryDto>> GetConsumptionHistoryForMobileAsync(int employeeId)
        {
            try
            {
                var consumptionHistory = await _context.Consumptions
                    .Where(c => c.EmployeeId == employeeId)
                    .Include(c => c.Employee)
                    .Include(c => c.Card)
                    .OrderByDescending(c => c.TransactionDate)
                    .Select(c => new MobileConsumptionHistoryDto
                    {
                        Id = c.Id,
                        EmployeeId = c.EmployeeId,
                        EmployeeName = c.Employee.FirstName + " " + c.Employee.LastName,
                        EmployeeNumber = c.Employee.EmployeeNumber,
                        CardId = c.CardId,
                        CardNumber = c.Card.CardNumber,
                        AmountInLiters = c.AmountInLiters,
                        TransactionDate = c.TransactionDate,
                        Description = c.Description,
                        CreatedDate = c.CreatedDate
                    })
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} consumption records for mobile history for employee {EmployeeId}", consumptionHistory.Count, employeeId);
                return consumptionHistory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving consumption history for mobile for employee {EmployeeId}", employeeId);
                throw;
            }
        }

        public async Task<IEnumerable<MobileConsumptionHistoryDto>> GetAllEmployeesConsumptionHistoryForMobileAsync()
        {
            try
            {
                var consumptionHistory = await _context.Consumptions
                    .Include(c => c.Employee)
                    .Include(c => c.Card)
                    .OrderByDescending(c => c.TransactionDate)
                    .ThenByDescending(c => c.EmployeeId)
                    .Select(c => new MobileConsumptionHistoryDto
                    {
                        Id = c.Id,
                        EmployeeId = c.EmployeeId,
                        EmployeeName = c.Employee.FirstName + " " + c.Employee.LastName,
                        EmployeeNumber = c.Employee.EmployeeNumber,
                        CardId = c.CardId,
                        CardNumber = c.Card.CardNumber,
                        AmountInLiters = c.AmountInLiters,
                        TransactionDate = c.TransactionDate,
                        Description = c.Description,
                        CreatedDate = c.CreatedDate
                    })
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} consumption records for mobile history for all employees", consumptionHistory.Count);
                return consumptionHistory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving consumption history for mobile for all employees");
                throw;
            }
        }

        public async Task<decimal> GetTotalAvailableQuotaForAllEmployeesAsync()
        {
            try
            {
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                var totalAvailable = await _context.FuelQuotas
                    .Where(fq => fq.Month == currentMonth && fq.Year == currentYear)
                    .SumAsync(fq => fq.QuotaInLiters - fq.UsedLiters);

                _logger.LogInformation("Retrieved total available quota: {TotalAvailable}L for all employees in month {Month}/{Year}", totalAvailable, currentMonth, currentYear);
                return totalAvailable;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total available quota for all employees");
                throw;
            }
        }

        public async Task<decimal> GetTotalConsumedQuotaForAllEmployeesAsync()
        {
            try
            {
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                var totalConsumed = await _context.FuelQuotas
                    .Where(fq => fq.Month == currentMonth && fq.Year == currentYear)
                    .SumAsync(fq => fq.UsedLiters);

                _logger.LogInformation("Retrieved total consumed quota: {TotalConsumed}L for all employees in month {Month}/{Year}", totalConsumed, currentMonth, currentYear);
                return totalConsumed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total consumed quota for all employees");
                throw;
            }
        }

        public async Task<IEnumerable<ConsumptionByPeriodDto>> GetConsumptionsForAllEmployeesByPeriodGroupedForMobileAsync(DateTime startDate, DateTime endDate, string groupBy = "day")
        {
            try
            {
                var consumptions = await _context.Consumptions
                    .Where(c => c.TransactionDate >= startDate &&
                               c.TransactionDate <= endDate)
                    .ToListAsync();

                var grouped = GroupConsumptionsByPeriod(consumptions, groupBy);

                _logger.LogInformation("Retrieved grouped consumptions for all employees in period {StartDate} to {EndDate}, grouped by {GroupBy}, total records: {Count}", startDate, endDate, groupBy, consumptions.Count);
                return grouped;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving grouped consumptions for all employees in period {StartDate} to {EndDate}, grouped by {GroupBy}", startDate, endDate, groupBy);
                throw;
            }
        }
    }
}