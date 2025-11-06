using SchoolSongManager.Api.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SchoolSongManager.Api.Services
{
    public interface INurseryRhymeRepository
    {
        Task<IEnumerable<NurseryRhyme>> GetAllAsync();
        Task<NurseryRhyme?> GetByIdAsync(string id);
        Task<NurseryRhyme> CreateAsync(NurseryRhyme rhyme);
        Task<NurseryRhyme?> UpdateAsync(string id, NurseryRhyme rhyme);
        Task<bool> DeleteAsync(string id);
        Task<string?> SaveAudioFileAsync(string rhymeId, IFormFile audioFile);
        Task<string?> SaveImageFileAsync(string rhymeId, IFormFile imageFile);
        bool DeleteAudioFile(string fileName);
        bool DeleteImageFile(string fileName);
        string GetAudioFilePath(string fileName);
        string GetImageFilePath(string fileName);
    }

    public class NurseryRhymeRepository : INurseryRhymeRepository
    {
        private readonly string _dataPath;
        private readonly string _audioPath;
        private readonly string _imagePath;
        private readonly string _jsonFilePath;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly JsonSerializerOptions _jsonOptions;

        public NurseryRhymeRepository(IConfiguration configuration)
        {
            // Pour Azure Web App, le répertoire data est à la racine
            var rootPath = configuration["DataPath"] ?? "/home/data";
            
            // En développement local, utiliser un répertoire dans le projet
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                rootPath = Path.Combine(Directory.GetCurrentDirectory(), "data");
            }

            _dataPath = rootPath;
            _audioPath = Path.Combine(_dataPath, "audio");
            _imagePath = Path.Combine(_dataPath, "images");
            _jsonFilePath = Path.Combine(_dataPath, "nursery-rhymes.json");

            // Créer les répertoires s'ils n'existent pas
            Directory.CreateDirectory(_dataPath);
            Directory.CreateDirectory(_audioPath);
            Directory.CreateDirectory(_imagePath);

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        public async Task<IEnumerable<NurseryRhyme>> GetAllAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (!File.Exists(_jsonFilePath))
                {
                    return new List<NurseryRhyme>();
                }

                var json = await File.ReadAllTextAsync(_jsonFilePath);
                var rhymes = JsonSerializer.Deserialize<List<NurseryRhyme>>(json, _jsonOptions);
                return rhymes ?? new List<NurseryRhyme>();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<NurseryRhyme?> GetByIdAsync(string id)
        {
            var rhymes = await GetAllAsync();
            return rhymes.FirstOrDefault(r => r.Id == id);
        }

        public async Task<NurseryRhyme> CreateAsync(NurseryRhyme rhyme)
        {
            await _semaphore.WaitAsync();
            try
            {
                rhyme.Id = Guid.NewGuid().ToString();
                rhyme.CreatedAt = DateTime.UtcNow;
                rhyme.UpdatedAt = DateTime.UtcNow;

                var rhymes = (await GetAllInternalAsync()).ToList();
                rhymes.Add(rhyme);

                await SaveAllInternalAsync(rhymes);
                return rhyme;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<NurseryRhyme?> UpdateAsync(string id, NurseryRhyme updatedRhyme)
        {
            await _semaphore.WaitAsync();
            try
            {
                var rhymes = (await GetAllInternalAsync()).ToList();
                var existingRhyme = rhymes.FirstOrDefault(r => r.Id == id);

                if (existingRhyme == null)
                {
                    return null;
                }

                existingRhyme.Title = updatedRhyme.Title;
                existingRhyme.Url = updatedRhyme.Url;
                existingRhyme.UpdatedAt = DateTime.UtcNow;

                // Conserver les fichiers existants si aucun nouveau fichier n'est fourni
                if (!string.IsNullOrEmpty(updatedRhyme.AudioFileName))
                {
                    existingRhyme.AudioFileName = updatedRhyme.AudioFileName;
                }
                
                if (!string.IsNullOrEmpty(updatedRhyme.ImageFileName))
                {
                    existingRhyme.ImageFileName = updatedRhyme.ImageFileName;
                }

                await SaveAllInternalAsync(rhymes);
                return existingRhyme;
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
                var rhymes = (await GetAllInternalAsync()).ToList();
                var rhyme = rhymes.FirstOrDefault(r => r.Id == id);

                if (rhyme == null)
                {
                    return false;
                }

                // Supprimer les fichiers s'ils existent
                if (!string.IsNullOrEmpty(rhyme.AudioFileName))
                {
                    DeleteAudioFile(rhyme.AudioFileName);
                }
                
                if (!string.IsNullOrEmpty(rhyme.ImageFileName))
                {
                    DeleteImageFile(rhyme.ImageFileName);
                }

                rhymes.Remove(rhyme);
                await SaveAllInternalAsync(rhymes);
                return true;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<string?> SaveAudioFileAsync(string rhymeId, IFormFile audioFile)
        {
            if (audioFile == null || audioFile.Length == 0)
            {
                return null;
            }

            var extension = Path.GetExtension(audioFile.FileName);
            var fileName = $"{rhymeId}_audio_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(_audioPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await audioFile.CopyToAsync(stream);
            }

            return fileName;
        }

        public async Task<string?> SaveImageFileAsync(string rhymeId, IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return null;
            }

            var extension = Path.GetExtension(imageFile.FileName);
            var fileName = $"{rhymeId}_image_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(_imagePath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return fileName;
        }

        public async Task<bool> DeleteAudioFileAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            var filePath = Path.Combine(_audioPath, fileName);
            
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        public async Task<bool> DeleteImageFileAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            var filePath = Path.Combine(_imagePath, fileName);
            
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        public bool DeleteAudioFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            var filePath = Path.Combine(_audioPath, fileName);
            
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        public bool DeleteImageFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            var filePath = Path.Combine(_imagePath, fileName);
            
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        public string GetAudioFilePath(string fileName)
        {
            return Path.Combine(_audioPath, fileName);
        }

        public string GetImageFilePath(string fileName)
        {
            return Path.Combine(_imagePath, fileName);
        }

        private async Task<IEnumerable<NurseryRhyme>> GetAllInternalAsync()
        {
            if (!File.Exists(_jsonFilePath))
            {
                return new List<NurseryRhyme>();
            }

            var json = await File.ReadAllTextAsync(_jsonFilePath);
            var rhymes = JsonSerializer.Deserialize<List<NurseryRhyme>>(json, _jsonOptions);
            return rhymes ?? new List<NurseryRhyme>();
        }

        private async Task SaveAllInternalAsync(IEnumerable<NurseryRhyme> rhymes)
        {
            var json = JsonSerializer.Serialize(rhymes, _jsonOptions);
            await File.WriteAllTextAsync(_jsonFilePath, json);
        }
    }
}