namespace PiastaNet.API.DTOs
{

    public record BoardgameCreateDto(
        ItemBaseCreateDto Item,
        int MinPlayers,
        int MaxPlayers,
        int? BggId,
        double? BggRating,
        double? BggAverageRating,
        int? BggRank,
        int? LearnDifficulty,
        int? PlayDifficulty
    );

    public record BoardgameUpdateDto(
        ItemBaseUpdateDto Item,
        int MinPlayers,
        int MaxPlayers,
        int? BggId,
        double? BggRating,
        double? BggAverageRating,
        int? BggRank,
        int? LearnDifficulty,
        int? PlayDifficulty
    );

    public record VideogameCreateDto(
        ItemBaseCreateDto Item,
        int MinPlayers,
        int MaxPlayers,
        int PlayingTime,
        int? Difficulty,
        int? Platform
    );

    public record VideogameUpdateDto(
        ItemBaseUpdateDto Item,
        int MinPlayers,
        int MaxPlayers,
        int PlayingTime,
        int? Difficulty,
        int? Platform
    );

}
