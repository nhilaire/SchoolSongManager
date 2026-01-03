using System.Text.Json;
using SchoolSongManager.Api.Models;

namespace SchoolSongManager.Api.Services;

public class NurseryRhymeAssignmentRepository
{
    private readonly string _dataDirectory;
    private readonly string _assignmentFilePath;
    private readonly string _nurseryRhymesFilePath;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public NurseryRhymeAssignmentRepository(IConfiguration configuration)
    {
        // Pour Azure Web App, le répertoire data est à la racine
        var rootPath = configuration["DataPath"] ?? "/home/data";

        // En développement local, utiliser un répertoire dans le projet
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            rootPath = Path.Combine(Directory.GetCurrentDirectory(), "data");
        }

        _dataDirectory = rootPath;
        _assignmentFilePath = Path.Combine(_dataDirectory, "nursery-rhyme-assignments.json");
        _nurseryRhymesFilePath = Path.Combine(_dataDirectory, "nursery-rhymes.json");

        EnsureDataDirectoryExists();
        EnsureAssignmentFileExists();
    }

    private void EnsureDataDirectoryExists()
    {
        if (!Directory.Exists(_dataDirectory))
        {
            Directory.CreateDirectory(_dataDirectory);
        }
    }

    private void EnsureAssignmentFileExists()
    {
        if (!File.Exists(_assignmentFilePath))
        {
            var defaultAssignments = GenerateDefaultAssignments();
            var json = JsonSerializer.Serialize(defaultAssignments, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_assignmentFilePath, json);
        }
    }

    private List<NurseryRhymeAssignment> GenerateDefaultAssignments()
    {
        var assignments = new List<NurseryRhymeAssignment>();
        var currentYear = DateTime.Now.Year;
        var periods = new[] { "P1", "P2", "P3", "P4", "P5" };
        
        // Récupérer les IDs des comptines existantes
        var nurseryRhymeIds = GetExistingNurseryRhymeIds();
        if (nurseryRhymeIds.Count == 0)
        {
            // Si aucune comptine n'existe, retourner une liste vide
            return assignments;
        }
        
        // Générer des assignments pour les 5 dernières années
        for (int yearOffset = 4; yearOffset >= 0; yearOffset--)
        {
            var schoolYear = $"{currentYear - yearOffset - 1}/{currentYear - yearOffset}";
            
            for (int periodIndex = 0; periodIndex < periods.Length; periodIndex++)
            {
                var assignmentId = Guid.NewGuid().ToString();
                // Utiliser un ID de comptine réel de manière cyclique
                var nurseryRhymeId = nurseryRhymeIds[(yearOffset * periods.Length + periodIndex) % nurseryRhymeIds.Count];
                
                assignments.Add(new NurseryRhymeAssignment
                {
                    Id = assignmentId,
                    NurseryRhymeId = nurseryRhymeId,
                    SchoolYear = schoolYear,
                    Period = periods[periodIndex],
                    AssignedDate = GetPeriodStartDate(currentYear - yearOffset, periodIndex + 1),
                    CreatedAt = DateTime.UtcNow.AddYears(-yearOffset).AddMonths(-periodIndex),
                    UpdatedAt = DateTime.UtcNow.AddYears(-yearOffset).AddMonths(-periodIndex)
                });
            }
        }
        
        return assignments;
    }

    private List<string> GetExistingNurseryRhymeIds()
    {
        try
        {
            if (!File.Exists(_nurseryRhymesFilePath))
            {
                return new List<string>();
            }

            var json = File.ReadAllText(_nurseryRhymesFilePath);
            
            // Options de désérialisation pour gérer la différence de casse
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            var nurseryRhymes = JsonSerializer.Deserialize<List<NurseryRhyme>>(json, options);
            return nurseryRhymes?.Select(nr => nr.Id).ToList() ?? new List<string>();
        }
        catch (Exception ex)
        {
            // Log l'erreur pour debugging si nécessaire
            Console.WriteLine($"Erreur lors de la lecture des comptines: {ex.Message}");
            return new List<string>();
        }
    }

    private DateTime GetPeriodStartDate(int year, int period)
    {
        // Dates approximatives des débuts de périodes scolaires en France
        return period switch
        {
            1 => new DateTime(year - 1, 9, 1),   // P1: Septembre
            2 => new DateTime(year - 1, 11, 5),  // P2: Novembre (après Toussaint)
            3 => new DateTime(year, 1, 8),       // P3: Janvier (après Noël)
            4 => new DateTime(year, 2, 26),      // P4: Février/Mars (après hiver)
            5 => new DateTime(year, 4, 23),      // P5: Avril/Mai (après printemps)
            _ => new DateTime(year, 9, 1)
        };
    }

    public async Task<List<NurseryRhymeAssignment>> GetAllAssignmentsAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            return await GetAllAssignmentsInternalAsync();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<List<NurseryRhymeAssignment>> GetAllAssignmentsInternalAsync()
    {
        if (!File.Exists(_assignmentFilePath))
        {
            return new List<NurseryRhymeAssignment>();
        }

        var json = await File.ReadAllTextAsync(_assignmentFilePath);
        return JsonSerializer.Deserialize<List<NurseryRhymeAssignment>>(json) ?? new List<NurseryRhymeAssignment>();
    }

    public async Task<NurseryRhymeAssignment?> GetAssignmentByIdAsync(string id)
    {
        var assignments = await GetAllAssignmentsAsync();
        return assignments.FirstOrDefault(a => a.Id == id);
    }

    public async Task<List<NurseryRhymeAssignment>> GetAssignmentsBySchoolYearAsync(string schoolYear)
    {
        var assignments = await GetAllAssignmentsAsync();
        return assignments.Where(a => a.SchoolYear == schoolYear).ToList();
    }

    public async Task<List<NurseryRhymeAssignment>> GetAssignmentsByPeriodAsync(string schoolYear, string period)
    {
        var assignments = await GetAllAssignmentsAsync();
        return assignments.Where(a => a.SchoolYear == schoolYear && a.Period == period).ToList();
    }

    public async Task<NurseryRhymeAssignment> CreateAssignmentAsync(NurseryRhymeAssignment assignment)
    {
        await _semaphore.WaitAsync();
        try
        {
            var assignments = await GetAllAssignmentsInternalAsync();
            
            assignment.Id = Guid.NewGuid().ToString();
            assignment.CreatedAt = DateTime.UtcNow;
            assignment.UpdatedAt = DateTime.UtcNow;
            
            assignments.Add(assignment);
            
            var json = JsonSerializer.Serialize(assignments, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_assignmentFilePath, json);
            
            return assignment;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<NurseryRhymeAssignment?> UpdateAssignmentAsync(string id, NurseryRhymeAssignment updatedAssignment)
    {
        await _semaphore.WaitAsync();
        try
        {
            var assignments = await GetAllAssignmentsInternalAsync();
            var existingAssignment = assignments.FirstOrDefault(a => a.Id == id);
            
            if (existingAssignment == null)
            {
                return null;
            }
            
            existingAssignment.NurseryRhymeId = updatedAssignment.NurseryRhymeId;
            existingAssignment.SchoolYear = updatedAssignment.SchoolYear;
            existingAssignment.Period = updatedAssignment.Period;
            existingAssignment.AssignedDate = updatedAssignment.AssignedDate;
            existingAssignment.UpdatedAt = DateTime.UtcNow;
            
            var json = JsonSerializer.Serialize(assignments, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_assignmentFilePath, json);
            
            return existingAssignment;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> DeleteAssignmentAsync(string id)
    {
        await _semaphore.WaitAsync();
        try
        {
            var assignments = await GetAllAssignmentsInternalAsync();
            var assignmentToRemove = assignments.FirstOrDefault(a => a.Id == id);
            
            if (assignmentToRemove == null)
            {
                return false;
            }
            
            assignments.Remove(assignmentToRemove);
            
            var json = JsonSerializer.Serialize(assignments, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_assignmentFilePath, json);
            
            return true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<int> DeleteAllAssignmentsForPeriodAsync(string schoolYear, string period)
    {
        await _semaphore.WaitAsync();
        try
        {
            var assignments = await GetAllAssignmentsInternalAsync();
            var assignmentsToRemove = assignments.Where(a => a.SchoolYear == schoolYear && a.Period == period).ToList();
            
            if (assignmentsToRemove.Count == 0)
            {
                return 0;
            }
            
            foreach (var assignment in assignmentsToRemove)
            {
                assignments.Remove(assignment);
            }
            
            var json = JsonSerializer.Serialize(assignments, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_assignmentFilePath, json);
            
            return assignmentsToRemove.Count;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<List<string>> GetAvailableSchoolYearsAsync()
    {
        var assignments = await GetAllAssignmentsAsync();
        var assignedYears = assignments.Select(a => a.SchoolYear).Distinct().ToHashSet();

        // Generate default school years from 2020 to current year + 1
        var defaultYears = GenerateDefaultSchoolYears();

        // Merge assigned years with default years
        foreach (var year in assignedYears)
        {
            defaultYears.Add(year);
        }

        return defaultYears.OrderBy(y => y).ToList();
    }

    private HashSet<string> GenerateDefaultSchoolYears()
    {
        var years = new HashSet<string>();
        var currentYear = DateTime.Now.Year;
        var currentMonth = DateTime.Now.Month;

        // If we're in September or later, include next year
        var endYear = currentMonth >= 9 ? currentYear + 1 : currentYear;

        // Generate years from 2020 to current/next year
        for (int startYear = 2020; startYear <= endYear; startYear++)
        {
            years.Add($"{startYear}/{startYear + 1}");
        }

        return years;
    }
}