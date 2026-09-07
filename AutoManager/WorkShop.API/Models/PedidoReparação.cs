namespace WorkShop.API.Models
{
    public class PedidoReparacao
    {
        public int Id { get; set; }
        public required string ClienteId { get; set; } 
        public int VeiculoId { get; set; } 

        public required string DescricaoProblema { get; set; }
        public DateTime DataSubmissao { get; set; } = DateTime.UtcNow;

        // Estados possíveis: "Pendente", "Aceite", "Rejeitado"
        public string Estado { get; set; } = "Pendente";

        // Opcional: Justificação caso o admin rejeite o pedido
        public string? ObservacoesAdmin { get; set; }
    }
}
