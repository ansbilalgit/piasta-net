using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PiastaNet.API.Services;

namespace PiastaNet.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {

        private readonly IItemsService _items;

        public ItemsController(IItemsService items) => _items = items;
        
        [HttpGet]
        public async Task<IActionResult> GetAll(
       [FromQuery] string? q,
       [FromQuery] string? type,
       [FromQuery] string? category,
       [FromQuery] string? sortBy,
       [FromQuery] string? sortDir,
       [FromQuery] int page = 1,
       [FromQuery] int pageSize = 20,
       CancellationToken ct = default)
        {
            var result = await _items.GetAllAsync(q, type, category, sortBy, sortDir, page, pageSize, ct);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var item = await _items.GetByIdAsync(id, ct);
            return item is null ? NotFound() : Ok(item);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var ok = await _items.DeleteAsync(id, ct);
            return ok ? NoContent() : NotFound();
        }
    }
}
