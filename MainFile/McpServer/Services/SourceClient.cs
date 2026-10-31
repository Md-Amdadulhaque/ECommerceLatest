using System.Text.Json;

namespace MCP_Server.Services
{
    // Calls the E_commerce API. Returns parsed JSON, plain text, or an error object,
    // so a tool result is always something the LLM can read.
    public class SourceClient
    {
        private readonly HttpClient _httpClient;
        public SourceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<object?> GetAsync(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);
            return await ReadResultAsync(response);
        }

        public async Task<object?> PostAsync(string endpoint, object body)
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, body);
            return await ReadResultAsync(response);
        }

        private static async Task<object?> ReadResultAsync(HttpResponseMessage response)
        {
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return new { error = $"API returned {(int)response.StatusCode}", details = body };

            if (string.IsNullOrWhiteSpace(body))
                return new { success = true };

            try
            {
                return JsonSerializer.Deserialize<JsonElement>(body);
            }
            catch (JsonException)
            {
                return body;
            }
        }
    }
}
