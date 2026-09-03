using SmartCard.DTOs;

namespace SmartCard.Services
{
    public interface ICardService
    {
        Task<IEnumerable<CardResponseDto>> GetAllCardsAsync();
        Task<CardResponseDto?> GetCardByIdAsync(int id);
        Task<CardResponseDto?> GetCardByNumberAsync(string cardNumber);
        Task<CardResponseDto> CreateCardAsync(CardCreateDto createDto);
        Task<CardResponseDto?> UpdateCardAsync(CardUpdateDto updateDto);
        Task<CardResponseDto?> UpdateCardStatusAsync(string cardNumber, CardStatusDto status);
        Task<CardValidationResponseDto?> ValidateCardWithDetailsAsync(string cardNumber);
        Task<bool> DeleteCardAsync(int id);
        Task<CardResponseDto?> AssignEmployeeToCardAsync(int cardId, int? employeeId);
    }
}