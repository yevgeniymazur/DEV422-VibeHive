using Grpc.Core;
using VibeHive.Inventory.Grpc;

namespace VibeHive.Inventory.Services;

// Owner: TheKeybladeofSolace — album inventory gRPC service and CRUD operations.
public class AlbumInventoryService : AlbumInventory.AlbumInventoryBase
{
    // TODO: Override AddAlbum, GetAlbum, UpdateAlbum, DeleteAlbum, and ListAlbums.
    // Generated base methods return gRPC Unimplemented until this module is built.
    public static readonly List<Album> _albums = new();
    public override Task<Album> AddAlbum(Album request, ServerCallContext context)
    {
        _albums.Add(request);

        return Task.FromResult(request);
    }

    public override Task<Album> GetAlbum (AlbumId request, ServerCallContext context)
    {
        var album = _albums.FirstOrDefault(a => a.Id == request.Id);

        if (album == null)
        {
            throw new RpcException(
                new Status(StatusCode.NotFound, "That album ain't there."));
        }

        return Task.FromResult(album);
    }

    public override Task<DeleteAlbumReply> DeleteAlbum (AlbumId reqeust, ServerCallContext context)
    {
        var album = _albums.FirstOrDefault(a => a.Id == reqeust.Id);

        if (album == null)
        {
            throw new RpcException(
                new Status(StatusCode.NotFound, "Sorry, we could not find that album."));
        }

        _albums.Remove(album);

        return Task.FromResult(new DeleteAlbumReply
        {
            Deleted = true
        });
    }

    public override Task<Album> UpdateAlbum (Album request, ServerCallContext context)
    {
        var album = _albums.FirstOrDefault(a => a.Id == request.Id);

        if (album == null)
        {
            throw new RpcException(
                new Status(StatusCode.NotFound, "We could not find that album. Try again."));
        }

        album.Title = request.Title;
        album.Artist = request.Artist;
        album.Genre = request.Genre;
        album.Year = request.Year;
        album.Available = request.Available;

        return Task.FromResult(album);
    }

    public override async Task ListAlbums(ListAlbumsRequest request, IServerStreamWriter<Album> replyStream, ServerCallContext context)
    {
        foreach (var album in _albums)
        {
            await replyStream.WriteAsync(album);
        }
    }
}
