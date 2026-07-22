using System;
using System.Collections.Generic;

namespace WorkShop.API.DTOs
{
    // DTO para enviar na lista de peças ao criar uma ordem
    public class ItemPecaDto
    {
        public string PecaId { get; set; }
        public int Quantidade { get; set; }
    }

    // DTO do pedido para criar Ordem de Reparação (RF8)
    public class CriarOrdemReparacaoDto
    {
        public string DescricaoProblema { get; set; } = string.Empty;
        public int VeiculoId { get; set; }
        public string ClienteId { get; set; } = string.Empty;
        public decimal CustoMaoDeObra { get; set; }
        public List<ItemPecaDto> Pecas { get; set; } = new List<ItemPecaDto>();
    }

    // DTO do pedido para atualizar Ordem (RF8)
    public class AtualizarOrdemReparacaoDto
    {
        public string? Estado { get; set; } // "Em Curso" ou "Concluída"
        public decimal CustoMaoDeObra { get; set; }
        public decimal CustoPecas { get; set; }
    }

    // DTO para ler os dados que vm da PartsCatalog.API via HttpClient
    public class RespostaPecaCatalogoDto
    {
        public string Id { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public int Stock { get; set; }
    }

    // DTO de resposta da Ordem de Reparação
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