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
            try
            {
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
                return (false, 0, $"Erro de comunicação com PartsCatalog.API: {ex.Message}");
            }
        }
    }
}