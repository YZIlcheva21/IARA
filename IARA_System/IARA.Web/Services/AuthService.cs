using System.Net.Http.Json;
using Microsoft.JSInterop;

namespace IARA.Web.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        private string? _token;

        public AuthService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public bool IsAuthenticated => !string.IsNullOrEmpty(_token);

        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Auth/login", new { Username = username, Password = password });
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (result != null && !string.IsNullOrEmpty(result.Token))
                    {
                        _token = result.Token;
                        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", _token);
                        _httpClient.DefaultRequestHeaders.Authorization = 
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task InitializeAsync()
        {
            try
            {
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
                if (!string.IsNullOrEmpty(token))
                {
                    _token = token;
                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
                }
            }
            catch
            {
                // Ignore errors during initialization
            }
        }

        public async Task LogoutAsync()
        {
            _token = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
        }

        private class LoginResponse
        {
            public string Token { get; set; } = string.Empty;
            public DateTime Expiration { get; set; }
            public string Username { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string[] Roles { get; set; } = Array.Empty<string>();
        }
    }
}
