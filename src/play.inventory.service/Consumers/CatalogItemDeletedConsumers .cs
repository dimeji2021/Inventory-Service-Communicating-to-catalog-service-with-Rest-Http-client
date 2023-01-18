using MassTransit;
using play.catalog.contracts;
using play.common;
using play.inventory.service.Entities;

namespace play.inventory.service.Consumers
{
    public class CatalogItemDeletedConsumer : IConsumer<CatalogItemDeleted>
    {
        private readonly IRepository<CatalogItem> _repository;
        public CatalogItemDeletedConsumer(IRepository<CatalogItem> repository)
        {
            _repository = repository;
        }
        public async Task Consume(ConsumeContext<CatalogItemDeleted> context)
        {
            var message = context.Message;
            var item = await _repository.GetItemsAsync(message.ItemId);
            if (item is null) return;
            await _repository.RemoveAsync(message.ItemId);
        }
    }
}