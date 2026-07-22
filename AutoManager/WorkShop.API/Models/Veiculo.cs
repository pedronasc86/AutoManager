using System.Collections.Generic;

namespace WorkShop.API.Models
{
    public class Veiculo
    {
        public int Id { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int Ano { get; set; }
        public string ClienteId { get; set; } = string.Empty;

        public ICollection<OrdemReparacao> OrdensReparacao { get; set; } = new List<OrdemReparacao>();
    }
}