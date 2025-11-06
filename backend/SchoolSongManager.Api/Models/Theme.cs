namespace SchoolSongManager.Api.Models
{
    public class Theme
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = "#3498db"; // Couleur par défaut
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateThemeRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = "#3498db";
    }

    public class UpdateThemeRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = "#3498db";
    }
}