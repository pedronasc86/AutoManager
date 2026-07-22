using System;

namespace WorkShop.API.Models
{
    public class OrdemReparacao
    {
        public int Id { get; set; }
        public DateTime DataEntrada { get; set; } = DateTime.UtcNow;
        public DateTime? DataConclusao { get; set; }
        public string DescricaoProblema { get; set; } = string.Empty;
        public string Estado { get; set; } = "Em Curso"; // "Em Curso" ou "Concluída" (RF8)

        public decimal CustoMaoDeObra { get; set; }
        public decimal CustoPecas { get; set; }
        public decimal ValorTotal => CustoMaoDeObra + CustoPecas;

        public int VeiculoId { get; set; }
        public Veiculo? Veiculo { get; set; }

        public string ClienteId { get; set; } = string.Empty; // RF9
    }
}