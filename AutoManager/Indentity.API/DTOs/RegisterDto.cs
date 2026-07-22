using System.ComponentModel.DataAnnotations;

namespace Identity.API.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "O formato do email é inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A password é obrigatória.")]
        // Aplicação das Data Annotations / Validações do RF3 (Mínimo 8 caracteres)
        [MinLength(8, ErrorMessage = "A password deve ter pelo menos 8 caracteres.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "A confirmação de password é obrigatória.")]
        [Compare("Password", ErrorMessage = "As passwords não coincidem.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Role/Função do utilizador: ex. "Admin", "Empresa", "Mecanico", "Contabilista"
        [Required(ErrorMessage = "A Role/Função do utilizador é obrigatória.")]
        public string Role { get; set; } = string.Empty;
    }
}
