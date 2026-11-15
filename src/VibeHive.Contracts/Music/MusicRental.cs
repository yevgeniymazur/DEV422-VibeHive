namespace VibeHive.Contracts;

// Created by Wilber Amaya-Maurisio
// Updated by Yevgeniy Mazur - 11/15/2026
// Changed UserId from int to Guid to support shared authentication


public class MusicRental
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int AlbumId { get; set; }
    public string AlbumTitle { get; set; } = string.Empty;
    public DateTime RentalDate { get; set; }
    public DateTime? ReturnDate { get; set; }
}
