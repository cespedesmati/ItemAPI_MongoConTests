using ItemAPI_MongoConTests.Api.DTOs;
using ItemAPI_MongoConTests.Api.Helpers;
using ItemAPI_MongoConTests.Api.Models;
using ItemAPI_MongoConTests.Api.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ItemAPI_MongoConTests.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {

        private readonly IItemsRepository repository;

        public ItemsController(IItemsRepository repository)
        {
            this.repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetItems(string? name = null)
        {

            var items = (await repository.GetItemsAsync()).Select(item => item.AsDto());
            if (items is null) return NotFound();

            if (!string.IsNullOrWhiteSpace(name))
            {
                items = items.Where(item => item.Name!.Contains(name, StringComparison.OrdinalIgnoreCase));
            }

            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDto>> GetItem(Guid id)
        {
            var items = await repository.GetItemAsync(id);
            if (items is null) return NotFound();
            return items.AsDto();
        }

        [HttpPost]
        public async Task<ActionResult<ItemDto>> CreateItem(CreateItemDto itemDto)
        {
            Item item = new()
            {
                Id = Guid.NewGuid(),
                Name = itemDto.Name,
                Price = itemDto.Price,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await repository.CreateItemAsync(item);

            return CreatedAtAction(nameof(GetItem), new { id = item.Id }, item.AsDto());
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateItem(Guid id, UpdateItemDto itemDto)
        {
            var existingItem = await repository.GetItemAsync(id);
            if (existingItem is null) return NotFound();

            Item updateItem = existingItem with
            {
                Name = itemDto.Name,
                Price = itemDto.Price
            };

            await repository.UpdateItemAsync(updateItem);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteItem(Guid id)
        {

            var existingItem = await repository.GetItemAsync(id);
            if (existingItem is null) return NotFound();

            await repository.DeleteItemAsync(id);
            return NoContent();

        }
    }

}

