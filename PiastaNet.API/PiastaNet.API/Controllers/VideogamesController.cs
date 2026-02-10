using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PiastaNet.API.DTOs;
using PiastaNet.API.Services;

namespace PiastaNet.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VideogamesController : ControllerBase
    {
        private readonly ILibraryTypeService _svc;
        public VideogamesController(ILibraryTypeService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Create(VideogameCreateDto dto, CancellationToken ct)
        {
            try
            {
                var item = await _svc.CreateVideogameAsync(dto, ct);
                return Created($"/api/items/{item.Id}", item);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, VideogameUpdateDto dto, CancellationToken ct)
        {
            try
            {
                var item = await _svc.UpdateVideogameAsync(id, dto, ct);
                return item is null ? NotFound() : Ok(item);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
