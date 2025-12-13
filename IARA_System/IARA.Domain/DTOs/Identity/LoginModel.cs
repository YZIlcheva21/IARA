// IARA.Domain/DTOs/Identity/LoginModel.cs
namespace IARA.Domain.DTOs.Identity
{
    public class LoginModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }
}