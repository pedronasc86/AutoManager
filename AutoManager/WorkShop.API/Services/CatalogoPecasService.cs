using System.Net.Http.Json;
using WorkShop.API.DTOs;

namespace WorkShop.API.Services
{
    public class CatalogoPecasService
    {
        private readonly HttpClient _httpClient;

        public CatalogoPecasService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Consulta a PartsCatalog.API para verificar peça e stock
        public async Task<(bool TemStock, decimal PrecoUnitario, string MensagemErro)> VerificarStockEObterPrecoAsync(string pecaId, int quantidadeDesejada)
        {
            try
            {
                // Faz o pedido GET à API de Catálogo do teu colega
                var resposta = await _httpClient.GetAsync($"api/pecas/{pecaId}");

                if (!resposta.IsSuccessStatusCode)
                {
                    return (false, 0, $"Peça #{pecaId} não foi encontrada no Catálogo.");
                }

                var peca = await resposta.Content.ReadFromJsonAsync<RespostaPecaCatalogoDto>();

                if (peca == null)
                {
                    return (false, 0, $"Erro ao ler dados da peça #{pecaId}.");
                }

                if (peca.Stock < quantidadeDesejada)
                {
                    return (false, 0, $"Stock insuficiente para a peça '{peca.Nome}'. Disponível: {peca.Stock}, Solicitado: {quantidadeDesejada}.");
                }

                return (true, peca.Preco, string.Empty);
            }
            catch (Exception ex)
            {
                // Caso a API do colega esteja desligada durante os teus testes locais
                return (false, 0, $"Erro de comunicação com PartsCatalog.API: {ex.Message}");
            }
        }
    }
}