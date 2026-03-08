

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using PiastaNet.API.Data;
using PiastaNet.API.DTOs;
using PiastaNet.API.Models;
using PiastaNet.API.Services;
using System.Security.Claims;

namespace PiastaNet.Tests;

public class GameEventServiceTests
{
    private readonly AppDbContext _context;
    private readonly GameEventService _service;
    private readonly Mock<IHttpContextAccessor> _mockAccessor;
    private readonly string testUser = "TestUser";
    public GameEventServiceTests()
    {
        _context = TestDbContextFactory.Create(); _mockAccessor = new Mock<IHttpContextAccessor>();

        // Create a fake User identity with the username "TestUser"
        var claims = new[] { new Claim(ClaimTypes.Name, testUser) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        // Setup the mock to return an HttpContext containing our fake user
        var httpContext = new DefaultHttpContext { User = user };
        _mockAccessor.Setup(_ => _.HttpContext).Returns(httpContext);

        // Pass the .Object into your service
        _service = new GameEventService(_context, _mockAccessor.Object);
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
        };

        var result = await _service.CreateAsync(dto);

        result.Should().NotBeNull();
        result.GameId.Should().Be(1);
        result.OwnerUserId.Should().Be(testUser);
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_When_Game_NotFound()
    {
        var dto = new CreateGameEventDto
        {
            GameId = 999,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
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
            EndTime = DateTime.UtcNow.AddHours(2)
        };

        var created = await _service.CreateAsync(createDto);

        await _service.DeleteAsync(created.Id);

        var entity = await _context.GameEvents.FindAsync(created.Id);
        entity.Should().BeNull();
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
            EndTime = DateTime.UtcNow.AddHours(2)
        });

        var dto = new RegisterParticipantDto(
            created.Id,
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
            MinNumberOfPlayers = 2,  // MUST respect game rules
            MaxNumberOfPlayers = 2   // Event allows only 2 players
        });

        // First participant
        await _service.AddParticipantAsync(new RegisterParticipantDto(
            created.Id,
            "user1"
        ));

        // Second participant (fills the event)
        await _service.AddParticipantAsync(new RegisterParticipantDto(
            created.Id,
            "user2"
        ));

        // Third participant should fail
        Func<Task> act = async () =>
            await _service.AddParticipantAsync(new RegisterParticipantDto(
                created.Id,
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
            EndTime = DateTime.UtcNow.AddHours(2)
        });

        var dto = new RegisterParticipantDto(
            created.Id,
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
