using VibeHive.Contracts;

namespace VibeHive.Api.Music;
// Created by Wilber Amaya-Maurisio
// In memory storage for the music rental module, resets when the API restarts
public static class MusicStore
{
    public static List<MusicAlbum> Albums { get; } = new();
    public static List<MusicRental> Rentals { get; } = new();

    private static int _nextAlbumId = 0;
    private static int _nextRentalId = 0;

    public static int NextAlbumId() => Interlocked.Increment(ref _nextAlbumId);
    public static int NextRentalId() => Interlocked.Increment(ref _nextRentalId);
}
