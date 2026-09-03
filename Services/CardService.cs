using Microsoft.EntityFrameworkCore;
using SmartCard.Data;
using SmartCard.DTOs;
using SmartCard.Models;

namespace SmartCard.Services
{
    public class CardService : ICardService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CardService> _logger;

        public CardService(ApplicationDbContext context, ILogger<CardService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<CardResponseDto>> GetAllCardsAsync()
        {
            try
            {
                var cards = await _context.Cards
                    .Include(c => c.Employee)
                    .ToListAsync();

                var cardDtos = cards.Select(c => MapToDto(c)).ToList();

                _logger.LogInformation("Retrieved {Count} cards", cardDtos.Count);
                return cardDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all cards");
                throw;
            }
        }

        public async Task<CardResponseDto?> GetCardByIdAsync(int id)
        {
            try
            {
                var card = await _context.Cards
                    .Include(c => c.Employee)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (card == null)
                {
                    _logger.LogWarning("Card with ID {Id} not found", id);
                    return null;
                }

                var cardDto = MapToDto(card);

                _logger.LogInformation("Retrieved card with ID {Id}", id);
                return cardDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving card with ID {Id}", id);
                throw;
            }
        }

        public async Task<CardResponseDto?> GetCardByNumberAsync(string cardNumber)
        {
            try
            {
                var card = await _context.Cards
                    .Include(c => c.Employee)
                    .FirstOrDefaultAsync(c => c.CardNumber == cardNumber);

                if (card == null)
                {
                    _logger.LogWarning("Card with number {CardNumber} not found", cardNumber);
                    return null;
                }

                var cardDto = MapToDto(card);

                _logger.LogInformation("Retrieved card with number {CardNumber}", cardNumber);
                return cardDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving card with number {CardNumber}", cardNumber);
                throw;
            }
        }

        public async Task<CardResponseDto?> UpdateCardStatusAsync(string cardNumber, CardStatusDto status)
        {
            try
            {
                var card = await _context.Cards
                    .Include(c => c.Employee)
                    .FirstOrDefaultAsync(c => c.CardNumber == cardNumber);

                if (card == null)
                {
                    _logger.LogWarning("Card with number {CardNumber} not found for status update", cardNumber);
                    return null;
                }

                card.Status = MapToModelStatus(status);
                card.UpdatedDate = DateTime.UtcNow;

                _context.Cards.Update(card);
                await _context.SaveChangesAsync();

                var cardDto = MapToDto(card);

                _logger.LogInformation("Updated status of card {CardNumber} to {Status}", cardNumber, status);
                return cardDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status of card {CardNumber}", cardNumber);
                throw;
            }
        }

        public async Task<CardValidationResponseDto?> ValidateCardWithDetailsAsync(string cardNumber)
        {
            try
            {
                var card = await _context.Cards
                    .Include(c => c.Employee)
                    .ThenInclude(e => e.Department)
                    .Include(c => c.Employee)
                    .ThenInclude(e => e.FuelQuotas)
                    .Include(c => c.Employee)
                    .ThenInclude(e => e.Consumptions)
                    .FirstOrDefaultAsync(c => c.CardNumber == cardNumber);

                if (card == null)
                {
                    _logger.LogWarning("Card with number {CardNumber} not found for validation", cardNumber);
                    return new CardValidationResponseDto
                    {
                        IsValid = false,
                        HasSufficientFuel = false,
                        AvailableFuel = 0,
                        EmployeeName = string.Empty,
                        EmployeeNumber = string.Empty,
                        ErrorMessage = "Carte non trouvée"
                    };
                }

                // Vérifier si la carte est active
                if (card.Status != CardStatus.Active)
                {
                    _logger.LogWarning("Card with number {CardNumber} is not active (status: {Status})", cardNumber, card.Status);
                    return new CardValidationResponseDto
                    {
                        IsValid = false,
                        HasSufficientFuel = false,
                        AvailableFuel = 0,
                        EmployeeName = card.Employee != null ? $"{card.Employee.FirstName} {card.Employee.LastName}" : string.Empty,
                        EmployeeNumber = card.Employee?.EmployeeNumber ?? string.Empty,
                        ErrorMessage = $"Carte inactive (statut: {card.Status})"
                    };
                }

                if (card.Employee == null)
                {
                    _logger.LogWarning("Card with number {CardNumber} is not assigned to any employee", cardNumber);
                    return new CardValidationResponseDto
                    {
                        IsValid = false,
                        HasSufficientFuel = false,
                        AvailableFuel = 0,
                        EmployeeName = string.Empty,
                        EmployeeNumber = string.Empty,
                        ErrorMessage = "Carte non attribuée à un employé"
                    };
                }

                // Calculer le quota disponible pour le mois en cours
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                var currentQuota = await _context.FuelQuotas
                    .FirstOrDefaultAsync(fq => fq.EmployeeId == card.Employee.Id &&
                                              fq.Month == currentMonth &&
                                              fq.Year == currentYear);

                decimal availableFuel = 0;
                bool hasSufficientFuel = false;

                if (currentQuota != null)
                {
                    availableFuel = currentQuota.QuotaInLiters - currentQuota.UsedLiters;
                    hasSufficientFuel = availableFuel > 0;
                }

                // Mettre à jour la date de dernière utilisation
                card.LastUsedDate = DateTime.UtcNow;
                _context.Cards.Update(card);
                await _context.SaveChangesAsync();

                var validationResult = new CardValidationResponseDto
                {
                    IsValid = true,
                    HasSufficientFuel = hasSufficientFuel,
                    AvailableFuel = availableFuel,
                    EmployeeName = $"{card.Employee.FirstName} {card.Employee.LastName}",
                    EmployeeNumber = card.Employee.EmployeeNumber,
                    ErrorMessage = null
                };

                _logger.LogInformation("Validated card {CardNumber} with details", cardNumber);
                return validationResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating card with number {CardNumber}", cardNumber);
                throw;
            }
        }

        public async Task<CardResponseDto> CreateCardAsync(CardCreateDto createDto)
        {
            try
            {
                _logger.LogInformation("Creating card with data: Number={CardNumber}, Status={Status}, EmployeeId={EmployeeId}",
                    createDto.CardNumber, createDto.Status, createDto.EmployeeId);

                // Check if card number already exists
                var existingCard = await _context.Cards
                    .FirstOrDefaultAsync(c => c.CardNumber == createDto.CardNumber);

                if (existingCard != null)
                {
                    _logger.LogWarning("Card with number {CardNumber} already exists", createDto.CardNumber);
                    throw new ArgumentException($"Card with number {createDto.CardNumber} already exists");
                }

                // Vérifier si l'employé existe si un ID est fourni
                if (createDto.EmployeeId.HasValue)
                {
                    var employeeExists = await _context.Employees.AnyAsync(e => e.Id == createDto.EmployeeId.Value);
                    if (!employeeExists)
                    {
                        _logger.LogWarning("Employee with ID {EmployeeId} does not exist when creating card {CardNumber}",
                            createDto.EmployeeId.Value, createDto.CardNumber);
                        throw new ArgumentException($"Employee with ID {createDto.EmployeeId.Value} does not exist");
                    }
                }

                var card = new Card
                {
                    CardNumber = createDto.CardNumber,
                    Status = MapToModelStatus(createDto.Status),
                    EmployeeId = createDto.EmployeeId,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Cards.Add(card);
                await _context.SaveChangesAsync();

                var cardDto = MapToDto(card);

                _logger.LogInformation("Created new card with ID {Id} and number {CardNumber}", card.Id, card.CardNumber);
                return cardDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating card with number {CardNumber}", createDto.CardNumber);
                throw;
            }
        }

        public async Task<CardResponseDto?> UpdateCardAsync(CardUpdateDto updateDto)
        {
            try
            {
                var card = await _context.Cards.FindAsync(updateDto.Id);
                if (card == null)
                {
                    _logger.LogWarning("Card with ID {Id} not found for update", updateDto.Id);
                    return null;
                }

                // Check if card number already exists for another card
                var existingCard = await _context.Cards
                    .FirstOrDefaultAsync(c => c.CardNumber == updateDto.CardNumber && c.Id != updateDto.Id);

                if (existingCard != null)
                {
                    throw new ArgumentException($"Card with number {updateDto.CardNumber} already exists");
                }

                card.CardNumber = updateDto.CardNumber;
                card.Status = MapToModelStatus(updateDto.Status);
                card.EmployeeId = updateDto.EmployeeId;
                card.UpdatedDate = DateTime.UtcNow;

                _context.Cards.Update(card);
                await _context.SaveChangesAsync();

                var cardDto = MapToDto(card);

                _logger.LogInformation("Updated card with ID {Id}", updateDto.Id);
                return cardDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating card with ID {Id}", updateDto.Id);
                throw;
            }
        }

        public async Task<bool> DeleteCardAsync(int id)
        {
            try
            {
                var card = await _context.Cards
                    .Include(c => c.Consumptions) // Include related consumptions
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (card == null)
                {
                    _logger.LogWarning("Card with ID {Id} not found for deletion", id);
                    return false;
                }

                // Check if card has related consumption records
                if (card.Consumptions.Any())
                {
                    // Instead of throwing an exception, deactivate the card
                    card.Status = CardStatus.Inactive;
                    card.UpdatedDate = DateTime.UtcNow;
                    _context.Cards.Update(card);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Card {Id} deactivated instead of deletion due to associated consumption records", id);
                    return true;
                }

                // If no related records, remove the card
                _context.Cards.Remove(card);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted card with ID {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting card with ID {Id}", id);
                throw;
            }
        }

        public async Task<CardResponseDto?> AssignEmployeeToCardAsync(int cardId, int? employeeId)
        {
            try
            {
                var card = await _context.Cards.FindAsync(cardId);
                if (card == null)
                {
                    _logger.LogWarning("Card with ID {Id} not found for employee assignment", cardId);
                    return null;
                }

                if (employeeId.HasValue)
                {
                    var employee = await _context.Employees.FindAsync(employeeId.Value);
                    if (employee == null)
                    {
                        _logger.LogWarning("Employee with ID {Id} not found for card assignment", employeeId.Value);
                        return null;
                    }
                }

                card.EmployeeId = employeeId;
                card.Status = employeeId.HasValue ? CardStatus.Active : CardStatus.Inactive; // Set status based on assignment
                card.UpdatedDate = DateTime.UtcNow;

                _context.Cards.Update(card);
                await _context.SaveChangesAsync();

                // Reload with employee data
                var updatedCard = await _context.Cards
                    .Include(c => c.Employee)
                    .FirstOrDefaultAsync(c => c.Id == cardId);

                var cardDto = MapToDto(updatedCard);

                _logger.LogInformation("Assigned employee {EmployeeId} to card {CardId}", employeeId, cardId);
                return cardDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning employee to card {CardId}", cardId);
                throw;
            }
        }

        private CardResponseDto MapToDto(Card card)
        {
            return new CardResponseDto
            {
                Id = card.Id,
                CardNumber = card.CardNumber,
                Status = MapToDtoStatus(card.Status),
                Employee = card.Employee != null ? new EmployeeResponseDto
                {
                    Id = card.Employee.Id,
                    EmployeeNumber = card.Employee.EmployeeNumber,
                    FirstName = card.Employee.FirstName,
                    LastName = card.Employee.LastName,
                    Email = card.Employee.Email,
                    Department = card.Employee.Department?.Name,
                    DepartmentId = card.Employee.DepartmentId,
                    MonthlyFuelQuota = card.Employee.MonthlyFuelQuota
                } : null,
                EmployeeId = card.EmployeeId,
                CreatedDate = card.CreatedDate,
                LastUsedDate = card.LastUsedDate
            };
        }

        private CardStatus MapToModelStatus(CardStatusDto statusDto)
        {
            return statusDto switch
            {
                CardStatusDto.Active => CardStatus.Active,
                CardStatusDto.Inactive => CardStatus.Inactive,
                CardStatusDto.Suspended => CardStatus.Suspended,
                CardStatusDto.Lost => CardStatus.Lost,
                _ => CardStatus.Inactive
            };
        }

        private CardStatusDto MapToDtoStatus(CardStatus status)
        {
            return status switch
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