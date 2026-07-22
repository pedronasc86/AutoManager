using System.ComponentModel.DataAnnotations;

namespace Identity.API.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de email inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A password é obrigatória.")]
        public string Password { get; set; } = string.Empty;
    }
}
