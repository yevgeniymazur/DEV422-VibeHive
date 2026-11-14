namespace VibeHive.Contracts;

// Created by Wilber Amaya-Maurisio
public class MusicRental
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int AlbumId { get; set; }
    public string AlbumTitle { get; set; } = string.Empty;
    public DateTime RentalDate { get; set; }
    public DateTime? ReturnDate { get; set; }
}
