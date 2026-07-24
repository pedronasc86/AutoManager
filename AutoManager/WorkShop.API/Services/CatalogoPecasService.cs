using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
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

        // Ajustado para receber int (ou string, conforme o teu DTO)
        public async Task<(bool TemStock, decimal PrecoUnitario, string MensagemErro)> VerificarStockEObterPrecoAsync(int pecaId, int quantidadeDesejada)
        {
            // =========================================================================
            // MOCK TEMPORÁRIO (Garante que a tua API avança sem depender da dele)
            // =========================================================================
            return await Task.FromResult((true, 25.50m, string.Empty));

            /* 
            CÓDIGO REAL (Basta apagar a linha de cima quando ele entregar a API dele):

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
            */
        }
    }
}