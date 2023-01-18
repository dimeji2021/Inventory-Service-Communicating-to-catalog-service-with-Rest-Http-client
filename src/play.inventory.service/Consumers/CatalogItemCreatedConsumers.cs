using MassTransit;
using play.catalog.contracts;
using play.common;
using play.inventory.service.Entities;

namespace play.inventory.service.Consumers
{
    public class CatalogItemCreatedConsumer : IConsumer<CatalogItemCreated>
    {
        private readonly IRepository<CatalogItem> _repository;
        public CatalogItemCreatedConsumer(IRepository<CatalogItem> repository)
        {
            _repository = repository;
        }
        public async Task Consume(ConsumeContext<CatalogItemCreated> context)
        {
            var message = context.Message;
            var item = await _repository.GetItemsAsync(message.ItemId);
            if (item is not null) return;
            item = new CatalogItem()
            {
                Id = message.ItemId,
                Name = message.Name,
                Description = message.Description
            };
            await _repository.CreateAsync(item);
        }
    }
}