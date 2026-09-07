namespace WorkShop.API.DTOs
{
    public class PecaResponse
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string ReferenciaPeca { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Compatibilidade { get; set; } = string.Empty;
        public decimal PrecoUnitario { get; set; }
        public int StockDisponivel { get; set; }
        public bool Ativo { get; set; }
    }
}
