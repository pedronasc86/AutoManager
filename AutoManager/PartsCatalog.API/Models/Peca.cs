using System.ComponentModel.DataAnnotations;

namespace PartsCatalog.API.Models
{
    public class Peca
    {
        public Guid Id { get; set; }
        public string ReferenciaPeca { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Compatibilidade { get; set; } = string.Empty;
        public decimal PrecoUnitario { get; set; }
        public int StockDisponivel { get; set; }
        public bool Ativo { get; set; } = true;
    }
}
