using Microsoft.AspNetCore.Mvc;
using PiastaNet.API.DTOs;
using PiastaNet.API.Services;

namespace PiastaNet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameEventsController : ControllerBase
    {
        private readonly IGameEventService _service;

        public GameEventsController(IGameEventService service)
        {
            _service = service;
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
    }
}
