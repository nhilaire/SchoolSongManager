using SchoolSongManager.Api.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SchoolSongManager.Api.Services
{
    public interface IThemeRepository
    {
        Task<IEnumerable<Theme>> GetAllAsync();
        Task<Theme?> GetByIdAsync(string id);
        Task<Theme> CreateAsync(Theme theme);
        Task<Theme?> UpdateAsync(string id, Theme theme);
        Task<bool> DeleteAsync(string id);
        Task<bool> IsThemeUsedInRhymesAsync(string themeId);
    }

    public class ThemeRepository : IThemeRepository
    {
        private readonly string _dataPath;
        private readonly string _jsonFilePath;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly INurseryRhymeRepository _nurseryRhymeRepository;

        public ThemeRepository(IConfiguration configuration, INurseryRhymeRepository nurseryRhymeRepository)
        {
            _nurseryRhymeRepository = nurseryRhymeRepository;
            
            // Pour Azure Web App, le répertoire data est à la racine
            var rootPath = configuration["DataPath"] ?? "/home/data";
            
            // En développement local, utiliser un répertoire dans le projet
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                rootPath = Path.Combine(Directory.GetCurrentDirectory(), "data");
            }

            _dataPath = rootPath;
            _jsonFilePath = Path.Combine(_dataPath, "themes.json");

            // Créer le répertoire s'il n'existe pas
            Directory.CreateDirectory(_dataPath);

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        public async Task<IEnumerable<Theme>> GetAllAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (!File.Exists(_jsonFilePath))
                {
                    // Créer des thèmes par défaut
                    var defaultThemes = new List<Theme>
                    {
                        new Theme { Id = Guid.NewGuid().ToString(), Name = "Animaux", Color = "#e74c3c", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                        new Theme { Id = Guid.NewGuid().ToString(), Name = "Nature", Color = "#27ae60", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                        new Theme { Id = Guid.NewGuid().ToString(), Name = "Saisons", Color = "#f39c12", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                        new Theme { Id = Guid.NewGuid().ToString(), Name = "Corps humain", Color = "#9b59b6", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                        new Theme { Id = Guid.NewGuid().ToString(), Name = "Famille", Color = "#34495e", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                    };
                    
                    await SaveAllInternalAsync(defaultThemes);
                    return defaultThemes;
                }

                var json = await File.ReadAllTextAsync(_jsonFilePath);
                var themes = JsonSerializer.Deserialize<List<Theme>>(json, _jsonOptions);
                return themes ?? new List<Theme>();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<Theme?> GetByIdAsync(string id)
        {
            var themes = await GetAllAsync();
            return themes.FirstOrDefault(t => t.Id == id);
        }

        public async Task<Theme> CreateAsync(Theme theme)
        {
            await _semaphore.WaitAsync();
            try
            {
                theme.Id = Guid.NewGuid().ToString();
                theme.CreatedAt = DateTime.UtcNow;
                theme.UpdatedAt = DateTime.UtcNow;

                var themes = (await GetAllInternalAsync()).ToList();
                themes.Add(theme);

                await SaveAllInternalAsync(themes);
                return theme;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<Theme?> UpdateAsync(string id, Theme updatedTheme)
        {
            await _semaphore.WaitAsync();
            try
            {
                var themes = (await GetAllInternalAsync()).ToList();
                var existingTheme = themes.FirstOrDefault(t => t.Id == id);

                if (existingTheme == null)
                {
                    return null;
                }

                existingTheme.Name = updatedTheme.Name;
                existingTheme.Color = updatedTheme.Color;
                existingTheme.UpdatedAt = DateTime.UtcNow;

                await SaveAllInternalAsync(themes);
                return existingTheme;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            await _semaphore.WaitAsync();
            try
            {
                var themes = (await GetAllInternalAsync()).ToList();
                var theme = themes.FirstOrDefault(t => t.Id == id);

                if (theme == null)
                {
                    return false;
                }

                themes.Remove(theme);
                await SaveAllInternalAsync(themes);
                return true;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> IsThemeUsedInRhymesAsync(string themeId)
        {
            var rhymes = await _nurseryRhymeRepository.GetAllAsync();
            return rhymes.Any(r => r.ThemeIds.Contains(themeId));
        }

        private async Task<IEnumerable<Theme>> GetAllInternalAsync()
        {
            if (!File.Exists(_jsonFilePath))
            {
                return new List<Theme>();
            }

            var json = await File.ReadAllTextAsync(_jsonFilePath);
            var themes = JsonSerializer.Deserialize<List<Theme>>(json, _jsonOptions);
            return themes ?? new List<Theme>();
        }

        private async Task SaveAllInternalAsync(IEnumerable<Theme> themes)
        {
            var json = JsonSerializer.Serialize(themes, _jsonOptions);
            await File.WriteAllTextAsync(_jsonFilePath, json);
        }
    }
}