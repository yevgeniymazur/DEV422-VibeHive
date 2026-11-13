using System.ComponentModel.DataAnnotations;

namespace VibeHive.Contracts;

// Shared by the API and the WinForms screen
public class MusicAlbum
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Artist is required.")]
    public string Artist { get; set; } = string.Empty;

    public string Genre { get; set; } = string.Empty;

    [Range(1900, 2100, ErrorMessage = "Year must be between 1900 and 2100.")]
    public int Year { get; set; }

    public bool Available { get; set; } = true;
}
