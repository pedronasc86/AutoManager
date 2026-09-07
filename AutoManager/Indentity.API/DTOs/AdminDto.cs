using System.ComponentModel.DataAnnotations;

namespace Identity.API.DTOs
{
    public class CriarAdminDto
    {
        [Required(ErrorMessage = "O primeiro nome é obrigatório.")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Introduz um e-mail válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A password é obrigatória.")]
        [MinLength(8, ErrorMessage = "A password deve ter pelo menos 8 caracteres.")]
        public string Password { get; set; } = string.Empty;
    }

    public class AtualizarAdminDto
    {
        [Required(ErrorMessage = "O primeiro nome é obrigatório.")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Introduz um e-mail válido.")]
        public string Email { get; set; } = string.Empty;

        public string? Password { get; set; }
    }
}