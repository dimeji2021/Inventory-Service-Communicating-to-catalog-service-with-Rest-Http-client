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
        private readonly IRepository<InventoryItem> _inventoryItemsRepository;
        private readonly IRepository<CatalogItem> _catalogItemsRepository;
        public ItemsController(IRepository<InventoryItem> inventoryItemsRepository, IRepository<CatalogItem> catalogItemsRepository)
        {
            _inventoryItemsRepository = inventoryItemsRepository;
            _catalogItemsRepository = catalogItemsRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync(Guid userId)
        {
            /*1. Get all the items for the user.
            2. Get the IDs of all the items.
            3. Get all the catalog items.
            4. Map the inventory items to the catalog items. which catalogitem is contain in this inventory
            5. Return the mapped items.

            */

            if (userId == Guid.Empty)
            {
                return BadRequest();
            }
            var inventoryItemEntities = await _inventoryItemsRepository.GetAllItemsAsync(item => item.UserId == userId);
            var ItemIds = inventoryItemEntities.Select(item => item.CatalogItemId); // Get the ID of all items
            var catalogItemEntities = await _catalogItemsRepository.GetAllItemsAsync(item => ItemIds.Contains(item.Id));
            var inventoryItemDto = inventoryItemEntities.Select(inventoryItem =>
            {
                var catalogItem = catalogItemEntities.Single(catalogItem => catalogItem.Id == inventoryItem.CatalogItemId);
                return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
            });
            return Ok(inventoryItemDto);
        }
        [HttpPost]
        public async Task<IActionResult> PostAsync(GrantItemDto grantItemDto)
        {
            var inventoryItem = await _inventoryItemsRepository.GetItemsAsync(item =>
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
                await _inventoryItemsRepository.CreateAsync(inventoryItem);
            }
            else
            {
                inventoryItem.Quantity += grantItemDto.Quantity;
                await _inventoryItemsRepository.UpdateAsync(inventoryItem);
            }
            return Ok();
        }
    }
}
