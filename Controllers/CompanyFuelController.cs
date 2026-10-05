using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCard.DTOs;
using SmartCard.Models;
using SmartCard.Services;
using System.Security.Claims;

namespace SmartCard.Controllers
{
    [ApiController]
    [Route("api/company-fuel")]
    [Authorize]
    public class CompanyFuelController : ControllerBase
    {
        private readonly ICompanyFuelService _companyFuelService;
        private readonly ILogger<CompanyFuelController> _logger;

        public CompanyFuelController(ICompanyFuelService companyFuelService, ILogger<CompanyFuelController> logger)
        {
            _companyFuelService = companyFuelService;
            _logger = logger;
        }

        /// <summary>
        /// Récupère la synthèse du carburant de l'entreprise :
        /// - Stock total approvisionné
        /// - Total des quotas attribués aux employés
        /// - Total consommé
        /// - Reste en stock entreprise (disponible pour distribution)
        /// - Carburant à la disposition des employés (non encore consommé)
        /// - Stock physique réel restant
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult<CompanyFuelSummaryDto>> GetSummary()
        {
            var summary = await _companyFuelService.GetFuelSummaryAsync();
            return Ok(summary);
        }

        /// <summary>
        /// Récupère la liste de tous les approvisionnements de l'entreprise
        /// </summary>
        [HttpGet("supplies")]
        public async Task<ActionResult<IEnumerable<CompanyFuelSupply>>> GetSupplies()
        {
            var supplies = await _companyFuelService.GetAllSuppliesAsync();
            return Ok(supplies);
        }

        /// <summary>
        /// Récupère un approvisionnement par son identifiant
        /// </summary>
        [HttpGet("supplies/{id}")]
        public async Task<ActionResult<CompanyFuelSupply>> GetSupply(int id)
        {
            var supply = await _companyFuelService.GetSupplyByIdAsync(id);
            if (supply == null)
            {
                return NotFound(new { message = $"Approvisionnement avec l'identifiant {id} non trouvé." });
            }
            return Ok(supply);
        }

        /// <summary>
        /// Enregistre un nouvel approvisionnement en carburant pour l'entreprise
        /// </summary>
        [HttpPost("supplies")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CompanyFuelSupply>> AddSupply([FromBody] CompanyFuelSupplyCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdBy = User.FindFirstValue(ClaimTypes.Email) 
                            ?? User.FindFirstValue(ClaimTypes.Name) 
                            ?? User.Identity?.Name 
                            ?? "Admin";

            var supply = await _companyFuelService.AddSupplyAsync(dto, createdBy);
            return CreatedAtAction(nameof(GetSupply), new { id = supply.Id }, supply);
        }

        /// <summary>
        /// Met à jour un approvisionnement existant
        /// </summary>
        [HttpPut("supplies")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CompanyFuelSupply>> UpdateSupply([FromBody] CompanyFuelSupplyUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var supply = await _companyFuelService.UpdateSupplyAsync(dto);
            return Ok(supply);
        }

        /// <summary>
        /// Supprime un approvisionnement
        /// </summary>
        [HttpDelete("supplies/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSupply(int id)
        {
            var success = await _companyFuelService.DeleteSupplyAsync(id);
            if (!success)
            {
                return NotFound(new { message = $"Approvisionnement avec l'identifiant {id} introuvable." });
            }
            return NoContent();
        }
    }
}
