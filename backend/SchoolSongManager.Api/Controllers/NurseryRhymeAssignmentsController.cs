using Microsoft.AspNetCore.Mvc;
using SchoolSongManager.Api.Models;
using SchoolSongManager.Api.Services;

namespace SchoolSongManager.Api.Controllers;

[ApiController]
[Route("api/nurseryrhymeassignments")]
public class NurseryRhymeAssignmentsController : ControllerBase
{
    private readonly NurseryRhymeAssignmentRepository _assignmentRepository;
    private readonly INurseryRhymeRepository _nurseryRhymeRepository;
    private readonly IPdfGeneratorService _pdfGeneratorService;

    public NurseryRhymeAssignmentsController(
        NurseryRhymeAssignmentRepository assignmentRepository,
        INurseryRhymeRepository nurseryRhymeRepository,
        IPdfGeneratorService pdfGeneratorService)
    {
        _assignmentRepository = assignmentRepository;
        _nurseryRhymeRepository = nurseryRhymeRepository;
        _pdfGeneratorService = pdfGeneratorService;
    }

    [HttpGet]
    public async Task<ActionResult<List<NurseryRhymeAssignmentDto>>> GetAllAssignments()
    {
        try
        {
            var assignments = await _assignmentRepository.GetAllAssignmentsAsync();
            var nurseryRhymes = await _nurseryRhymeRepository.GetAllAsync();
            
            var assignmentDtos = assignments.Select(assignment =>
            {
                var nurseryRhyme = nurseryRhymes.FirstOrDefault(nr => nr.Id == assignment.NurseryRhymeId);
                return new NurseryRhymeAssignmentDto
                {
                    Id = assignment.Id,
                    NurseryRhymeId = assignment.NurseryRhymeId,
                    NurseryRhyme = nurseryRhyme,
                    SchoolYear = assignment.SchoolYear,
                    Period = assignment.Period,
                    AssignedDate = assignment.AssignedDate,
                    CreatedAt = assignment.CreatedAt,
                    UpdatedAt = assignment.UpdatedAt
                };
            }).ToList();

            return Ok(assignmentDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erreur lors de la récupération des assignments: {ex.Message}");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<NurseryRhymeAssignmentDto>> GetAssignmentById(string id)
    {
        try
        {
            var assignment = await _assignmentRepository.GetAssignmentByIdAsync(id);
            if (assignment == null)
            {
                return NotFound($"Assignment avec l'ID {id} non trouvé");
            }

            var nurseryRhyme = await _nurseryRhymeRepository.GetByIdAsync(assignment.NurseryRhymeId);
            
            var assignmentDto = new NurseryRhymeAssignmentDto
            {
                Id = assignment.Id,
                NurseryRhymeId = assignment.NurseryRhymeId,
                NurseryRhyme = nurseryRhyme,
                SchoolYear = assignment.SchoolYear,
                Period = assignment.Period,
                AssignedDate = assignment.AssignedDate,
                CreatedAt = assignment.CreatedAt,
                UpdatedAt = assignment.UpdatedAt
            };

            return Ok(assignmentDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erreur lors de la récupération de l'assignment: {ex.Message}");
        }
    }

    [HttpGet("school-year/{schoolYear}")]
    public async Task<ActionResult<List<NurseryRhymeAssignmentDto>>> GetAssignmentsBySchoolYear(string schoolYear)
    {
        try
        {
            var assignments = await _assignmentRepository.GetAssignmentsBySchoolYearAsync(schoolYear);
            var nurseryRhymes = await _nurseryRhymeRepository.GetAllAsync();
            
            var assignmentDtos = assignments.Select(assignment =>
            {
                var nurseryRhyme = nurseryRhymes.FirstOrDefault(nr => nr.Id == assignment.NurseryRhymeId);
                return new NurseryRhymeAssignmentDto
                {
                    Id = assignment.Id,
                    NurseryRhymeId = assignment.NurseryRhymeId,
                    NurseryRhyme = nurseryRhyme,
                    SchoolYear = assignment.SchoolYear,
                    Period = assignment.Period,
                    AssignedDate = assignment.AssignedDate,
                    CreatedAt = assignment.CreatedAt,
                    UpdatedAt = assignment.UpdatedAt
                };
            }).ToList();

            return Ok(assignmentDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erreur lors de la récupération des assignments: {ex.Message}");
        }
    }

    [HttpGet("period/{schoolYear}/{period}")]
    public async Task<ActionResult<List<NurseryRhymeAssignmentDto>>> GetAssignmentsByPeriod(string schoolYear, string period)
    {
        try
        {
            var assignments = await _assignmentRepository.GetAssignmentsByPeriodAsync(schoolYear, period);
            var nurseryRhymes = await _nurseryRhymeRepository.GetAllAsync();
            
            var assignmentDtos = assignments.Select(assignment =>
            {
                var nurseryRhyme = nurseryRhymes.FirstOrDefault(nr => nr.Id == assignment.NurseryRhymeId);
                return new NurseryRhymeAssignmentDto
                {
                    Id = assignment.Id,
                    NurseryRhymeId = assignment.NurseryRhymeId,
                    NurseryRhyme = nurseryRhyme,
                    SchoolYear = assignment.SchoolYear,
                    Period = assignment.Period,
                    AssignedDate = assignment.AssignedDate,
                    CreatedAt = assignment.CreatedAt,
                    UpdatedAt = assignment.UpdatedAt
                };
            }).ToList();

            return Ok(assignmentDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erreur lors de la récupération des assignments: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<ActionResult<NurseryRhymeAssignmentDto>> CreateAssignment([FromBody] CreateNurseryRhymeAssignmentRequest request)
    {
        try
        {
            // Vérifier que la comptine existe
            var nurseryRhyme = await _nurseryRhymeRepository.GetByIdAsync(request.NurseryRhymeId);
            if (nurseryRhyme == null)
            {
                return BadRequest($"Comptine avec l'ID {request.NurseryRhymeId} non trouvée");
            }

            var assignment = new NurseryRhymeAssignment
            {
                NurseryRhymeId = request.NurseryRhymeId,
                SchoolYear = request.SchoolYear,
                Period = request.Period,
                AssignedDate = request.AssignedDate
            };

            var createdAssignment = await _assignmentRepository.CreateAssignmentAsync(assignment);
            
            var assignmentDto = new NurseryRhymeAssignmentDto
            {
                Id = createdAssignment.Id,
                NurseryRhymeId = createdAssignment.NurseryRhymeId,
                NurseryRhyme = nurseryRhyme,
                SchoolYear = createdAssignment.SchoolYear,
                Period = createdAssignment.Period,
                AssignedDate = createdAssignment.AssignedDate,
                CreatedAt = createdAssignment.CreatedAt,
                UpdatedAt = createdAssignment.UpdatedAt
            };

            return CreatedAtAction(nameof(GetAssignmentById), new { id = createdAssignment.Id }, assignmentDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erreur lors de la création de l'assignment: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<NurseryRhymeAssignmentDto>> UpdateAssignment(string id, [FromBody] UpdateNurseryRhymeAssignmentRequest request)
    {
        try
        {
            // Vérifier que la comptine existe
            var nurseryRhyme = await _nurseryRhymeRepository.GetByIdAsync(request.NurseryRhymeId);
            if (nurseryRhyme == null)
            {
                return BadRequest($"Comptine avec l'ID {request.NurseryRhymeId} non trouvée");
            }

            var assignment = new NurseryRhymeAssignment
            {
                NurseryRhymeId = request.NurseryRhymeId,
                SchoolYear = request.SchoolYear,
                Period = request.Period,
                AssignedDate = request.AssignedDate
            };

            var updatedAssignment = await _assignmentRepository.UpdateAssignmentAsync(id, assignment);
            
            if (updatedAssignment == null)
            {
                return NotFound($"Assignment avec l'ID {id} non trouvé");
            }

            var assignmentDto = new NurseryRhymeAssignmentDto
            {
                Id = updatedAssignment.Id,
                NurseryRhymeId = updatedAssignment.NurseryRhymeId,
                NurseryRhyme = nurseryRhyme,
                SchoolYear = updatedAssignment.SchoolYear,
                Period = updatedAssignment.Period,
                AssignedDate = updatedAssignment.AssignedDate,
                CreatedAt = updatedAssignment.CreatedAt,
                UpdatedAt = updatedAssignment.UpdatedAt
            };

            return Ok(assignmentDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erreur lors de la mise à jour de l'assignment: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAssignment(string id)
    {
        try
        {
            var deleted = await _assignmentRepository.DeleteAssignmentAsync(id);
            
            if (!deleted)
            {
                return NotFound($"Assignment avec l'ID {id} non trouvé");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erreur lors de la suppression de l'assignment: {ex.Message}");
        }
    }

    [HttpDelete("period/{schoolYear}/{period}")]
    public async Task<ActionResult<DeleteAssignmentsResponse>> DeleteAllAssignmentsForPeriod(string schoolYear, string period)
    {
        try
        {
            var deletedCount = await _assignmentRepository.DeleteAllAssignmentsForPeriodAsync(schoolYear, period);
            
            return Ok(new DeleteAssignmentsResponse 
            { 
                DeletedCount = deletedCount,
                Message = $"{deletedCount} assignment(s) supprimé(s) pour {schoolYear} - {period}"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erreur lors de la suppression des assignments: {ex.Message}");
        }
    }

    [HttpGet("school-years")]
    public async Task<ActionResult<List<string>>> GetAvailableSchoolYears()
    {
        try
        {
            var schoolYears = await _assignmentRepository.GetAvailableSchoolYearsAsync();
            return Ok(schoolYears);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erreur lors de la récupération des années scolaires: {ex.Message}");
        }
    }

    [HttpPost("generate-pdf")]
    public async Task<ActionResult> GeneratePdf([FromBody] GeneratePdfRequest request)
    {
        try
        {
            // Récupérer les comptines sélectionnées
            var nurseryRhymes = new List<NurseryRhyme>();
            foreach (var rhymeId in request.NurseryRhymeIds)
            {
                var rhyme = await _nurseryRhymeRepository.GetByIdAsync(rhymeId);
                if (rhyme != null)
                {
                    nurseryRhymes.Add(rhyme);
                }
            }

            if (nurseryRhymes.Count == 0)
            {
                return BadRequest("Aucune comptine trouvée avec les IDs fournis");
            }

            // Générer le PDF
            var pdfBytes = _pdfGeneratorService.GenerateNurseryRhymesPdf(nurseryRhymes, request.SchoolYear, request.Period);
            
            // Retourner le PDF
            var fileName = $"comptines_{request.SchoolYear.Replace("/", "-")}_{request.Period}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erreur lors de la génération du PDF: {ex.Message}");
        }
    }
}

// DTOs pour les requests et responses
public class NurseryRhymeAssignmentDto
{
    public string Id { get; set; } = string.Empty;
    public string NurseryRhymeId { get; set; } = string.Empty;
    public NurseryRhyme? NurseryRhyme { get; set; }
    public string SchoolYear { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateNurseryRhymeAssignmentRequest
{
    public string NurseryRhymeId { get; set; } = string.Empty;
    public string SchoolYear { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
}

public class UpdateNurseryRhymeAssignmentRequest
{
    public string NurseryRhymeId { get; set; } = string.Empty;
    public string SchoolYear { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
}

public class GeneratePdfRequest
{
    public List<string> NurseryRhymeIds { get; set; } = new();
    public string SchoolYear { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
}

public class DeleteAssignmentsResponse
{
    public int DeletedCount { get; set; }
    public string Message { get; set; } = string.Empty;
}