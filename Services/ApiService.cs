using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RaceDay.MVC.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;

            // Set the Base URL of your Part 2 API
            // Make sure this matches the port your API runs on!
            _httpClient.BaseAddress = new Uri("https://localhost:7006/api/");
        }

        // Helper method to attach the JWT token to requests
        public void AddJwtToken()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // GET request
        public async Task<T?> GetAsync<T>(string endpoint)
        {
            AddJwtToken();
            var response = await _httpClient.GetAsync(endpoint);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            return default;
        }

        // POST request
        public async Task<HttpResponseMessage> PostAsync(string endpoint, object data)
        {
            AddJwtToken();
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _httpClient.PostAsync(endpoint, content);
        }

        // PUT request
        public async Task<HttpResponseMessage> PutAsync(string endpoint, object data)
        {
            AddJwtToken();
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _httpClient.PutAsync(endpoint, content);
        }

        // DELETE request
        public async Task<HttpResponseMessage> DeleteAsync(string endpoint)
        {
            AddJwtToken();
            return await _httpClient.DeleteAsync(endpoint);
        }
    }
}