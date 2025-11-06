using Microsoft.AspNetCore.Mvc;
using SchoolSongManager.Api.Models;
using SchoolSongManager.Api.Services;

namespace SchoolSongManager.Api.Controllers
{
    [ApiController]
    [Route("api/nursery-rhymes")]
    public class NurseryRhymesController : ControllerBase
    {
        private readonly INurseryRhymeRepository _repository;

        public NurseryRhymesController(INurseryRhymeRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NurseryRhyme>>> GetAll()
        {
            try
            {
                var rhymes = await _repository.GetAllAsync();
                return Ok(rhymes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la récupération des comptines", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<NurseryRhyme>> GetById(string id)
        {
            try
            {
                var rhyme = await _repository.GetByIdAsync(id);
                if (rhyme == null)
                {
                    return NotFound(new { message = "Comptine non trouvée" });
                }
                return Ok(rhyme);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la récupération de la comptine", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<NurseryRhyme>> Create([FromForm] CreateNurseryRhymeRequest request, IFormFile? audioFile = null, IFormFile? imageFile = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Title))
                {
                    return BadRequest(new { message = "Le titre est obligatoire" });
                }

                var rhyme = new NurseryRhyme
                {
                    Title = request.Title.Trim(),
                    Url = request.Url?.Trim()
                };

                var createdRhyme = await _repository.CreateAsync(rhyme);

                // Sauvegarder le fichier audio s'il est fourni
                if (audioFile != null && audioFile.Length > 0)
                {
                    var audioFileName = await _repository.SaveAudioFileAsync(createdRhyme.Id, audioFile);
                    if (!string.IsNullOrEmpty(audioFileName))
                    {
                        createdRhyme.AudioFileName = audioFileName;
                    }
                }

                // Sauvegarder le fichier image s'il est fourni
                if (imageFile != null && imageFile.Length > 0)
                {
                    var imageFileName = await _repository.SaveImageFileAsync(createdRhyme.Id, imageFile);
                    if (!string.IsNullOrEmpty(imageFileName))
                    {
                        createdRhyme.ImageFileName = imageFileName;
                    }
                }

                // Mettre à jour si des fichiers ont été ajoutés
                if (!string.IsNullOrEmpty(createdRhyme.AudioFileName) || !string.IsNullOrEmpty(createdRhyme.ImageFileName))
                {
                    await _repository.UpdateAsync(createdRhyme.Id, createdRhyme);
                }

                return CreatedAtAction(nameof(GetById), new { id = createdRhyme.Id }, createdRhyme);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la création de la comptine", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<NurseryRhyme>> Update(string id, [FromForm] UpdateNurseryRhymeRequest request, IFormFile? audioFile = null, IFormFile? imageFile = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Title))
                {
                    return BadRequest(new { message = "Le titre est obligatoire" });
                }

                var existingRhyme = await _repository.GetByIdAsync(id);
                if (existingRhyme == null)
                {
                    return NotFound(new { message = "Comptine non trouvée" });
                }

                var updatedRhyme = new NurseryRhyme
                {
                    Title = request.Title.Trim(),
                    Url = request.Url?.Trim()
                };

                // Traiter le nouveau fichier audio s'il est fourni
                if (audioFile != null && audioFile.Length > 0)
                {
                    // Supprimer l'ancien fichier audio s'il existe
                    if (!string.IsNullOrEmpty(existingRhyme.AudioFileName))
                    {
                        _repository.DeleteAudioFile(existingRhyme.AudioFileName);
                    }

                    // Sauvegarder le nouveau fichier audio
                    var audioFileName = await _repository.SaveAudioFileAsync(id, audioFile);
                    updatedRhyme.AudioFileName = audioFileName;
                }

                // Traiter le nouveau fichier image s'il est fourni
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Supprimer l'ancienne image s'elle existe
                    if (!string.IsNullOrEmpty(existingRhyme.ImageFileName))
                    {
                        _repository.DeleteImageFile(existingRhyme.ImageFileName);
                    }

                    // Sauvegarder la nouvelle image
                    var imageFileName = await _repository.SaveImageFileAsync(id, imageFile);
                    updatedRhyme.ImageFileName = imageFileName;
                }

                var result = await _repository.UpdateAsync(id, updatedRhyme);
                if (result == null)
                {
                    return NotFound(new { message = "Comptine non trouvée" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la mise à jour de la comptine", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                var deleted = await _repository.DeleteAsync(id);
                if (!deleted)
                {
                    return NotFound(new { message = "Comptine non trouvée" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la suppression de la comptine", error = ex.Message });
            }
        }

        [HttpGet("audio/{fileName}")]
        public async Task<ActionResult> GetAudio(string fileName)
        {
            try
            {
                var filePath = _repository.GetAudioFilePath(fileName);
                
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(new { message = "Fichier audio non trouvé" });
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var contentType = GetAudioContentType(fileName);
                
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la récupération du fichier audio", error = ex.Message });
            }
        }

        [HttpGet("images/{fileName}")]
        public async Task<ActionResult> GetImage(string fileName)
        {
            try
            {
                var filePath = _repository.GetImageFilePath(fileName);
                
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(new { message = "Image non trouvée" });
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var contentType = GetImageContentType(fileName);
                
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la récupération de l'image", error = ex.Message });
            }
        }

        private static string GetAudioContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".mp3" => "audio/mpeg",
                ".wav" => "audio/wav",
                ".ogg" => "audio/ogg",
                ".m4a" => "audio/mp4",
                ".aac" => "audio/aac",
                _ => "application/octet-stream"
            };
        }

        private static string GetImageContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".svg" => "image/svg+xml",
                _ => "application/octet-stream"
            };
        }
    }
}