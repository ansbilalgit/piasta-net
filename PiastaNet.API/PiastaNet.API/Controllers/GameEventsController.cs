using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PiastaNet.API.DTOs;
using PiastaNet.API.Services;

namespace PiastaNet.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GameEventsController : ControllerBase
    {
        private readonly IGameEventService _service;

        public GameEventsController(IGameEventService service)
        {
            _service = service;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll(
    [FromQuery] string? q,
    [FromQuery] int? gameId,
    [FromQuery] string? ownerUserId,
    [FromQuery] DateTime? from,
    [FromQuery] DateTime? to,
    [FromQuery] bool? upcomingOnly,
    [FromQuery] bool? pastOnly,
    [FromQuery] int? minPlayers,
    [FromQuery] int? maxPlayers,
    [FromQuery] bool? hasAvailableSlots,
    [FromQuery] string? sortBy,
    [FromQuery] string? sortDir,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    CancellationToken ct = default)
        {
            var result = await _service.GetAllAsync(
                q,
                gameId,
                ownerUserId,
                from,
                to,
                upcomingOnly,
                pastOnly,
                minPlayers,
                maxPlayers,
                hasAvailableSlots,
                sortBy,
                sortDir,
                page,
                pageSize,
                ct);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateGameEventDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut]
        public async Task<IActionResult> Update(UpdateGameEventDto dto)
        {
            var result = await _service.UpdateAsync(dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] string ownerUserId)
        {
            await _service.DeleteAsync(id, ownerUserId);
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPost("add-participant")]
        public async Task<IActionResult> AddParticipant([FromBody] RegisterParticipantDto dto)
        {
            await _service.AddParticipantAsync(dto);
            return Ok();
        }

        [HttpPost("remove-participant")]
        public async Task<IActionResult> RemoveParticipant([FromBody] RegisterParticipantDto dto)
        {
            await _service.RemoveParticipantAsync(dto);
            return Ok();
        }
    }
}
