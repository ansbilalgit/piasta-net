

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PiastaNet.API.Data;
using PiastaNet.API.DTOs;
using PiastaNet.API.Models;
using PiastaNet.API.Services;

namespace PiastaNet.Tests;

public class GameEventServiceTests
{
    private readonly AppDbContext _context;
    private readonly GameEventService _service;

    public GameEventServiceTests()
    {
        _context = TestDbContextFactory.Create();
        _service = new GameEventService(_context);
    }

    private async Task SeedGame(int gameId = 1)
    {
        _context.Items.Add(new Item
        {
            Id = gameId,
            Name = "Test Game",
            Type = ItemType.Boardgame,
            Boardgame = new Boardgame
            {
                MinPlayers = 2,
                MaxPlayers = 4
            }
        });

        await _context.SaveChangesAsync();
    }

    // =========================================
    // CREATE TESTS
    // =========================================

    [Fact]
    public async Task CreateAsync_Should_Create_Event()
    {
        await SeedGame();

        var dto = new CreateGameEventDto
        {
            GameId = 1,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
            OwnerUserId = "owner1"
        };

        var result = await _service.CreateAsync(dto);

        result.Should().NotBeNull();
        result.GameId.Should().Be(1);
        result.OwnerUserId.Should().Be("owner1");
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_When_Game_NotFound()
    {
        var dto = new CreateGameEventDto
        {
            GameId = 999,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
            OwnerUserId = "owner1"
        };

        Func<Task> act = async () => await _service.CreateAsync(dto);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Game not found");
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_When_Start_After_End()
    {
        await SeedGame();

        var dto = new CreateGameEventDto
        {
            GameId = 1,
            StartTime = DateTime.UtcNow.AddHours(3),
            EndTime = DateTime.UtcNow.AddHours(1),
            OwnerUserId = "owner1"
        };

        Func<Task> act = async () => await _service.CreateAsync(dto);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("StartTime must be before EndTime");
    }

    // =========================================
    // DELETE TESTS
    // =========================================

    [Fact]
    public async Task DeleteAsync_Should_Delete_When_Owner()
    {
        await SeedGame();

        var createDto = new CreateGameEventDto
        {
            GameId = 1,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
            OwnerUserId = "owner1"
        };

        var created = await _service.CreateAsync(createDto);

        await _service.DeleteAsync(created.Id, "owner1");

        var entity = await _context.GameEvents.FindAsync(created.Id);
        entity.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_Should_Throw_When_Not_Owner()
    {
        await SeedGame();

        var createDto = new CreateGameEventDto
        {
            GameId = 1,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
            OwnerUserId = "owner1"
        };

        var created = await _service.CreateAsync(createDto);

        Func<Task> act = async () =>
            await _service.DeleteAsync(created.Id, "otherUser");

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    // =========================================
    // PARTICIPANT TESTS
    // =========================================
    // =========================================
    // PARTICIPANT TESTS
    // =========================================

    [Fact]
    public async Task AddParticipantAsync_Should_Add_When_Valid()
    {
        await SeedGame();

        var created = await _service.CreateAsync(new CreateGameEventDto
        {
            GameId = 1,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
            OwnerUserId = "owner1"
        });

        var dto = new RegisterParticipantDto(
            created.Id,
            "user2",
            "user2"
        );

        await _service.AddParticipantAsync(dto);

        var entity = await _context.GameEvents
            .Include(e => e.Participants)
            .FirstAsync();

        entity.Participants.Should().HaveCount(1);
    }
    [Fact]
    public async Task AddParticipantAsync_Should_Throw_When_Max_Reached()
    {
        await SeedGame(); // Boardgame Min=2 Max=4

        var created = await _service.CreateAsync(new CreateGameEventDto
        {
            GameId = 1,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
            OwnerUserId = "owner1",
            MinNumberOfPlayers = 2,  // MUST respect game rules
            MaxNumberOfPlayers = 2   // Event allows only 2 players
        });

        // First participant
        await _service.AddParticipantAsync(new RegisterParticipantDto(
            created.Id,
            "user1",
            "user1"
        ));

        // Second participant (fills the event)
        await _service.AddParticipantAsync(new RegisterParticipantDto(
            created.Id,
            "user2",
            "user2"
        ));

        // Third participant should fail
        Func<Task> act = async () =>
            await _service.AddParticipantAsync(new RegisterParticipantDto(
                created.Id,
                "user3",
                "user3"
            ));

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Max players reached.");
    }
    [Fact]
    public async Task RemoveParticipantAsync_Should_Remove_When_Exists()
    {
        await SeedGame();

        var created = await _service.CreateAsync(new CreateGameEventDto
        {
            GameId = 1,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
            OwnerUserId = "owner1"
        });

        var dto = new RegisterParticipantDto(
            created.Id,
            "user2",
            "user2"
        );

        await _service.AddParticipantAsync(dto);
        await _service.RemoveParticipantAsync(dto);

        var entity = await _context.GameEvents
            .Include(e => e.Participants)
            .FirstAsync();

        entity.Participants.Should().BeEmpty();
    }
}
