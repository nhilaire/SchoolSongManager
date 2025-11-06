namespace SchoolSongManager.Api.Models
{
    public class NurseryRhyme
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? ImageFileName { get; set; }
        public string? Url { get; set; }
        public string? AudioFileName { get; set; }
        public List<string> ThemeIds { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateNurseryRhymeRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Url { get; set; }
        public List<string> ThemeIds { get; set; } = new List<string>();
    }

    public class UpdateNurseryRhymeRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Url { get; set; }
        public List<string> ThemeIds { get; set; } = new List<string>();
    }
}