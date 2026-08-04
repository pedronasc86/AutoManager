namespace WorkShop.API.Models
{
    public class PecaAplicadaOrdem
    {
        public int Id { get; set; }

        public int OrdemReparacaoId { get; set; }
        public OrdemReparacao OrdemReparacao { get; set; } = null!;

        public Guid PecaId { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
    }
}