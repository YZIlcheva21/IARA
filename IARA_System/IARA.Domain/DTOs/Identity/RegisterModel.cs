// IARA.Domain/DTOs/Identity/RegisterModel.cs
namespace IARA.Domain.DTOs.Identity
{
    public class RegisterModel
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Name { get; set; } // Добавено за грешка "Cannot resolve symbol 'Name'"
        public string? IDNumber { get; set; } // Добавено за грешка "Cannot resolve symbol 'IDNumber'"
        public string? FisherName { get; set; } // Добавено за грешка "Cannot resolve symbol 'FisherName'"
        public string? FisherIDNumber { get; set; } // Добавено за грешка "Cannot resolve symbol 'FisherIDNumber'"
        public int? FisherId { get; set; } // Добавено за грешка "Cannot resolve symbol 'FisherId'"
        public string? Role { get; set; }
    }
}