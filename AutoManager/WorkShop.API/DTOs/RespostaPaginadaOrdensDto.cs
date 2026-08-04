namespace WorkShop.API.DTOs
{
    public class RespostaPaginadaOrdensDto
    {
        public List<RespostaOrdemReparacaoDto> Itens { get; set; } = new();

        public int PaginaAtual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalItens { get; set; }

        // Estatísticas gerais do dashboard
        public int TotalOrdens { get; set; }
        public int TotalEmCurso { get; set; }
        public int TotalConcluidas { get; set; }
    }
}