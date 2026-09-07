using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WorkShop.API.Models
{
    [Index(nameof(Matricula), IsUnique = true)]
    public class Veiculo
    {
        public int Id { get; set; }

        [Required]
        public string Matricula { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int Ano { get; set; }
        public string ClienteId { get; set; } = string.Empty;

        public ICollection<OrdemReparacao> OrdensReparacao { get; set; } = new List<OrdemReparacao>();
    }
}