using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace IARA.Web.Services
{
    public class AuthenticatedHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;

        public AuthenticatedHttpClient(HttpClient httpClient, AuthService authService)
        {
            _httpClient = httpClient;
            _authService = authService;
        }

        public async Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            await SetAuthHeader();
            return await _httpClient.GetAsync(requestUri);
        }

        public async Task<T?> GetFromJsonAsync<T>(string requestUri)
        {
            await SetAuthHeader();
            return await System.Net.Http.Json.HttpClientJsonExtensions.GetFromJsonAsync<T>(_httpClient, requestUri);
        }

        public async Task<HttpResponseMessage> PostAsJsonAsync<T>(string requestUri, T value)
        {
            await SetAuthHeader();
            return await System.Net.Http.Json.HttpClientJsonExtensions.PostAsJsonAsync(_httpClient, requestUri, value);
        }

        public async Task<HttpResponseMessage> PutAsJsonAsync<T>(string requestUri, T value)
        {
            await SetAuthHeader();
            return await System.Net.Http.Json.HttpClientJsonExtensions.PutAsJsonAsync(_httpClient, requestUri, value);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string requestUri)
        {
            await SetAuthHeader();
            return await _httpClient.DeleteAsync(requestUri);
        }

        private async Task SetAuthHeader()
        {
            try
            {
                // Clear existing authorization first
                _httpClient.DefaultRequestHeaders.Authorization = null;
                
                var token = await _authService.GetTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new AuthenticationHeaderValue("Bearer", token);
                }
            }
            catch
            {
                // Ignore errors
            }
        }
    }
}
