namespace WorkShop.API.DTOs
{
    public class CriarPedidoDto
    {
        public int VeiculoId { get; set; }
        public required string DescricaoProblema { get; set; }
    }

    public class PedidoReparacaoResponseDto
    {
        public int Id { get; set; }
        public string ClienteId { get; set; } = string.Empty;
        public int VeiculoId { get; set; }
        public string DescricaoProblema { get; set; } = string.Empty;
        public DateTime DataSubmissao { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? ObservacoesAdmin { get; set; }
    }

    public class AnalisarPedidoDto
    {
        public required string Estado { get; set; } // "Aceite" ou "Rejeitado"
        public string? Observacoes { get; set; }
    }
}