using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCard.DTOs;
using SmartCard.Services;

namespace SmartCard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CardsController : ControllerBase
    {
        private readonly ICardService _cardService;

        public CardsController(ICardService cardService)
        {
            _cardService = cardService;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CardResponseDto>>> GetAllCards()
        {
            var cards = await _cardService.GetAllCardsAsync();
            return Ok(cards);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<CardResponseDto>> GetCard(int id)
        {
            var card = await _cardService.GetCardByIdAsync(id);
            if (card == null)
            {
                return NotFound();
            }
            return Ok(card);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<CardResponseDto>> CreateCard([FromBody] CardCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                    .ToArray();

                var errorMessages = string.Join(", ", errors.SelectMany(e => e.Errors));
                return BadRequest(new { error = $"Erreurs de validation: {errorMessages}", details = errors });
            }

            try
            {
                var card = await _cardService.CreateCardAsync(createDto);
                return CreatedAtAction(nameof(GetCard), new { id = card.Id }, card);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                // Log l'erreur complète pour débogage
                // En production, ne pas renvoyer l'erreur complète pour des raisons de sécurité
                return StatusCode(500, new { error = "Une erreur interne s'est produite lors de la création de la carte" });
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<CardResponseDto>> UpdateCard(int id, CardUpdateDto updateDto)
        {
            if (id != updateDto.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                    .ToArray();

                var errorMessages = string.Join(", ", errors.SelectMany(e => e.Errors));
                return BadRequest(new { error = $"Erreurs de validation: {errorMessages}", details = errors });
            }

            try
            {
                var card = await _cardService.UpdateCardAsync(updateDto);
                if (card == null)
                {
                    return NotFound();
                }
                return Ok(card);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Une erreur interne s'est produite lors de la mise à jour de la carte" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteCard(int id)
        {
            try
            {
                var result = await _cardService.DeleteCardAsync(id);
                if (!result)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Une erreur interne s'est produite lors de la suppression de la carte" });
            }
        }

        [HttpPost("assign-employee")]
        [Authorize]
        public async Task<ActionResult<CardResponseDto>> AssignEmployeeToCard([FromBody] CardEmployeeAssignmentDto assignmentDto)
        {
            try
            {
                var card = await _cardService.AssignEmployeeToCardAsync(assignmentDto.CardId, assignmentDto.EmployeeId);
                if (card == null)
                {
                    return NotFound();
                }
                return Ok(card);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Une erreur interne s'est produite lors de l'attribution de la carte à l'employé" });
            }
        }
    }
}