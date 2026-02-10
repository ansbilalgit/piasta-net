using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PiastaNet.API.DTOs;
using PiastaNet.API.Services;

namespace PiastaNet.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BoardgamesController : ControllerBase
    {
        private readonly ILibraryTypeService _svc;
        public BoardgamesController(ILibraryTypeService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Create(BoardgameCreateDto dto, CancellationToken ct)
        {
            try
            {
                var item = await _svc.CreateBoardgameAsync(dto, ct);
                return Created($"/api/items/{item.Id}", item);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, BoardgameUpdateDto dto, CancellationToken ct)
        {
            try
            {
                var item = await _svc.UpdateBoardgameAsync(id, dto, ct);
                return item is null ? NotFound() : Ok(item);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
