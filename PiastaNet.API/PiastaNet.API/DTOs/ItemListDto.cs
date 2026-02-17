using PiastaNet.API.Models;

namespace PiastaNet.API.DTOs
{
    public record ItemListDto(
        int Id,
        string Name,
        int? Length,
        string Description,
        string? Thumbnail,
        ItemType Type,
        int? Copies,
        List<string> Categories,
        int? MinPlayers,
        int? MaxPlayers
    );

    public record ItemBaseCreateDto(
        string Name,
        int? Length,
        string? Description,
        string? Thumbnail,
        int? Copies,
        List<string>? Categories
    );

    public record ItemBaseUpdateDto(
        string Name,
        int? Length,
        string? Description,
        string? Thumbnail,
        int? Copies,
        List<string>? Categories
    );

}
