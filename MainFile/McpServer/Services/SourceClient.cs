using System.Text.Json;

namespace MCP_Server.Services
{
    public class SourceClient
    {
        private readonly HttpClient _httpClient;
        public SourceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<T?> GetAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json);
        }

        public async Task<T?> PostAsJsonAsync<T>(string endpoint, JsonContent model)
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, model);
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}
