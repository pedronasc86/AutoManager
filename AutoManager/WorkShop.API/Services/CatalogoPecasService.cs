using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WorkShop.API.DTOs;

namespace WorkShop.API.Services
{
    public interface ICatalogoPecasService
    {
        Task<(bool TemStock, decimal PrecoUnitario, string MensagemErro)> VerificarStockEObterPrecoAsync(string pecaId, int quantidadeDesejada);
    }

    public class CatalogoPecasService : ICatalogoPecasService
    {
        private readonly HttpClient _httpClient;

        public CatalogoPecasService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(bool TemStock, decimal PrecoUnitario, string MensagemErro)> VerificarStockEObterPrecoAsync(string pecaId, int quantidadeDesejada)
        {
            if (!Guid.TryParse(pecaId, out _))
            {
                return (false, 0, "O ID da peça não é válido.");
            }

            if (quantidadeDesejada <= 0)
            {
                return (false, 0, "A quantidade da peça deve ser superior a zero.");
            }

            try
            {
                var resposta = await _httpClient.GetAsync($"api/pecas/{pecaId}");

                if (!resposta.IsSuccessStatusCode)
                {
                    return (false, 0, $"Peça #{pecaId} não foi encontrada no catálogo.");
                }

                var peca = await resposta.Content.ReadFromJsonAsync<RespostaPecaCatalogoDto>();

                if (peca == null)
                {
                    return (false, 0, "Não foi possível ler os dados da peça.");
                }

                if (!peca.Ativo)
                {
                    return (false, 0, $"A peça '{peca.Nome}' está inativa.");
                }

                if (peca.StockDisponivel < quantidadeDesejada)
                {
                    return (false, 0,
                        $"Stock insuficiente para '{peca.Nome}'. Disponível: {peca.StockDisponivel}.");
                }

                return (true, peca.PrecoUnitario, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, 0, $"Não foi possível contactar o catálogo de peças: {ex.Message}");
            }
        }
    }
}