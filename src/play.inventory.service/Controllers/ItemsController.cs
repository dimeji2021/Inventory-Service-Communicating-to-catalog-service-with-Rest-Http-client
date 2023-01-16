using Microsoft.AspNetCore.Mvc;
using play.common;
using play.inventory.service.Clients;
using play.inventory.service.Dtos;
using play.inventory.service.Entities;

namespace play.inventory.service.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository<InventoryItem> _itemsRepository;
        private readonly CatalogClient _catalogClient;
        public ItemsController(IRepository<InventoryItem> _itemsRepository, CatalogClient _catalogClient)
        {
            this._itemsRepository = _itemsRepository;
            this._catalogClient = _catalogClient;
        }
        [HttpGet]
        public async Task<IActionResult> GetAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest();
            }
            var catalogItems = await _catalogClient.GetCatalogItemAsync();
            var inventoryItemEntities = await _itemsRepository.GetAllItemsAsync(item => item.UserId == userId);
            var inventoryItemDto = inventoryItemEntities.Select(inventoryItem =>
            {
                var catalogItem = catalogItems.Single(catalogItem=> catalogItem.Id == inventoryItem.CatalogItemId);
                return inventoryItem.AsDto(catalogItem.Name,catalogItem.Description);
            });
            return Ok(inventoryItemDto);
        }
        [HttpPost]
        public async Task<IActionResult> PostAsync(GrantItemDto grantItemDto)
        {
            var inventoryItem = await _itemsRepository.GetItemsAsync(item =>
            item.UserId == grantItemDto.UserId && item.CatalogItemId == grantItemDto.CatalogItemId);// check if the user already owns the item
            if (inventoryItem == null)
            {
                inventoryItem = new InventoryItem
                {
                    CatalogItemId = grantItemDto.CatalogItemId,
                    UserId = grantItemDto.UserId,
                    Quantity = grantItemDto.Quantity,
                    AcquiredDate = DateTimeOffset.UtcNow
                };
                await _itemsRepository.CreateAsync(inventoryItem);
            }
            else
            {
                inventoryItem.Quantity += grantItemDto.Quantity;
                await _itemsRepository.UpdateAsync(inventoryItem);
            }
            return Ok();
        }
    }
}
