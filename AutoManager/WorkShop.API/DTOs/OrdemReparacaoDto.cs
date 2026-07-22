using System;

namespace WorkShop.API.DTOs
{
    public class CriarOrdemReparacaoDto
    {
        public string DescricaoProblema { get; set; } = string.Empty;
        public int VeiculoId { get; set; }
        public string ClienteId { get; set; } = string.Empty;
    }

    public class AtualizarOrdemReparacaoDto
    {
        public string? Estado { get; set; } // "Em Curso" ou "Concluída"
        public decimal CustoMaoDeObra { get; set; }
        public decimal CustoPecas { get; set; }
    }

    public class RespostaOrdemReparacaoDto
    {
        public int Id { get; set; }
        public DateTime DataEntrada { get; set; }
        public DateTime? DataConclusao { get; set; }
        public string DescricaoProblema { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public decimal CustoMaoDeObra { get; set; }
        public decimal CustoPecas { get; set; }
        public decimal ValorTotal { get; set; }
        public int VeiculoId { get; set; }
        public string ClienteId { get; set; } = string.Empty;
    }
}