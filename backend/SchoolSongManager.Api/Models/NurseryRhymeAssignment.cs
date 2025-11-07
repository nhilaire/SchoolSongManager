using System;

namespace SchoolSongManager.Api.Models;

public class NurseryRhymeAssignment
{
    public string Id { get; set; } = string.Empty;
    public string NurseryRhymeId { get; set; } = string.Empty;
    public string SchoolYear { get; set; } = string.Empty; // Format: "2024/2025"
    public string Period { get; set; } = string.Empty; // Format: "P1", "P2", "P3", "P4", "P5"
    public DateTime AssignedDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}