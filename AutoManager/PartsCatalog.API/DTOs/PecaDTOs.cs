using System.ComponentModel.DataAnnotations;

namespace PartsCatalog.API.DTOs
{
    // O que recebes para CRIAR / ATUALIZAR
    public class CriarPecaRequest
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome não pode exceder 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "A referência é obrigatória.")]
        [StringLength(50, ErrorMessage = "A referência não pode exceder 50 caracteres.")]
        public string ReferenciaPeca { get; set; } = string.Empty;

        [Required(ErrorMessage = "A categoria é obrigatória.")]
        public string Categoria { get; set; } = string.Empty;

        public string Compatibilidade { get; set; } = string.Empty;

        [Range(0.01, 100000.00, ErrorMessage = "O preço deve ser maior que zero.")]
        public decimal PrecoUnitario { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "A quantidade não pode ser negativa.")]
        public int StockDisponivel { get; set; }
    }

    // O que devolves para LEITURA
    public class PecaResponse
    {
        public Guid Id { get; set; } // Ajustado para Guid
        public string Nome { get; set; } = string.Empty;
        public string ReferenciaPeca { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Compatibilidade { get; set; } = string.Empty;
        public decimal PrecoUnitario { get; set; }
        public int StockDisponivel { get; set; }
        public bool Ativo { get; set; }
    }
}