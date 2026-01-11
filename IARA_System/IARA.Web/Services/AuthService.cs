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
                // Login endpoint doesn't require authentication, so use HttpClient directly
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
                else
                {
                    // Try to read error message
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Login failed: {response.StatusCode} - {errorContent}");
                }
                return false;
            }
            catch
            {
                throw; // Re-throw to show error message
            }
        }

        public async Task<bool> RegisterAsync(string username, string email, string password, string confirmPassword, string? firstName = null, string? lastName = null)
        {
            try
            {
                var registerModel = new
                {
                    Username = username,
                    Email = email,
                    Password = password,
                    ConfirmPassword = confirmPassword,
                    FirstName = firstName,
                    LastName = lastName
                };

                var response = await _httpClient.PostAsJsonAsync("api/Auth/register", registerModel);
                
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Registration failed: {response.StatusCode} - {errorContent}");
                }
            }
            catch
            {
                throw; // Re-throw to show error message
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

        public async Task<string?> GetTokenAsync()
        {
            if (string.IsNullOrEmpty(_token))
            {
                try
                {
                    var tokenFromStorage = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
                    // Only set token if it's not null, empty, or whitespace
                    if (!string.IsNullOrWhiteSpace(tokenFromStorage))
                    {
                        _token = tokenFromStorage;
                    }
                    else
                    {
                        // Clear invalid token from storage
                        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
                        _token = null;
                    }
                }
                catch
                {
                    // If we can't access storage, clear token
                    _token = null;
                }
            }
            // Return null if token is empty or whitespace
            return string.IsNullOrWhiteSpace(_token) ? null : _token;
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
