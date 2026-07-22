namespace WorkShop.API.DTOs
{
    public class CriarVeiculoDto
    {
        public string Matricula { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int Ano { get; set; }
        public string ClienteId { get; set; } = string.Empty;
    }

    public class RespostaVeiculoDto
    {
        public int Id { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int Ano { get; set; }
        public string ClienteId { get; set; } = string.Empty;
    }
}