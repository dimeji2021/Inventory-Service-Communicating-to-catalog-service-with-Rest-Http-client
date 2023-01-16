using play.inventory.service.Dtos;

namespace play.inventory.service.Clients
{
    public class CatalogClient
    {
        private readonly HttpClient _httpClient;
        public CatalogClient(HttpClient httpClient)
        {
            this._httpClient = httpClient;
        }
        public async Task<IReadOnlyCollection<CatalogItemDto>> GetCatalogItemAsync()
        {
            return await _httpClient.GetFromJsonAsync<IReadOnlyCollection<CatalogItemDto>>("/api/items");
        }
    }
}