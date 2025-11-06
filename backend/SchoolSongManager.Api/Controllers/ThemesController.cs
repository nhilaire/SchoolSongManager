using Microsoft.AspNetCore.Mvc;
using SchoolSongManager.Api.Models;
using SchoolSongManager.Api.Services;

namespace SchoolSongManager.Api.Controllers
{
    [ApiController]
    [Route("api/themes")]
    public class ThemesController : ControllerBase
    {
        private readonly IThemeRepository _repository;

        public ThemesController(IThemeRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Theme>>> GetAll()
        {
            try
            {
                var themes = await _repository.GetAllAsync();
                return Ok(themes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la récupération des thèmes", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Theme>> GetById(string id)
        {
            try
            {
                var theme = await _repository.GetByIdAsync(id);
                if (theme == null)
                {
                    return NotFound(new { message = "Thème non trouvé" });
                }
                return Ok(theme);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la récupération du thème", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<Theme>> Create([FromBody] CreateThemeRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return BadRequest(new { message = "Le nom du thème est obligatoire" });
                }

                var theme = new Theme
                {
                    Name = request.Name.Trim(),
                    Color = request.Color?.Trim() ?? "#3498db"
                };

                var createdTheme = await _repository.CreateAsync(theme);
                return CreatedAtAction(nameof(GetById), new { id = createdTheme.Id }, createdTheme);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la création du thème", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Theme>> Update(string id, [FromBody] UpdateThemeRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return BadRequest(new { message = "Le nom du thème est obligatoire" });
                }

                var updatedTheme = new Theme
                {
                    Name = request.Name.Trim(),
                    Color = request.Color?.Trim() ?? "#3498db"
                };

                var result = await _repository.UpdateAsync(id, updatedTheme);
                if (result == null)
                {
                    return NotFound(new { message = "Thème non trouvé" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la mise à jour du thème", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                // Vérifier si le thème est utilisé dans des comptines
                var isUsed = await _repository.IsThemeUsedInRhymesAsync(id);
                if (isUsed)
                {
                    return BadRequest(new { message = "Ce thème ne peut pas être supprimé car il est utilisé dans une ou plusieurs comptines" });
                }

                var deleted = await _repository.DeleteAsync(id);
                if (!deleted)
                {
                    return NotFound(new { message = "Thème non trouvé" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la suppression du thème", error = ex.Message });
            }
        }
    }
}