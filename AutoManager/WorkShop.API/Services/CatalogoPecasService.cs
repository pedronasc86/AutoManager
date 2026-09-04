using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WorkShop.API.DTOs;
using System.Text.Json;

namespace WorkShop.API.Services
{
    public interface ICatalogoPecasService
    {
        Task<(bool TemStock, decimal PrecoUnitario, string MensagemErro)> VerificarStockEObterPrecoAsync(string pecaId, int quantidadeDesejada);

        Task<List<RespostaPecaCatalogoDto>> ObterPecasAsync();
    }

    public class CatalogoPecasService : ICatalogoPecasService
    {
        private readonly HttpClient _httpClient;

        public CatalogoPecasService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<RespostaPecaCatalogoDto>> ObterPecasAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<RespostaPecaCatalogoDto>>("api/pecas")
                    ?? new List<RespostaPecaCatalogoDto>();
            }
            catch (Exception ex)
            {
                throw new HttpRequestException(
                    "Não foi possível contactar a PartsCatalog.API.",
                    ex
                );
            }
        }

        public async Task<(bool TemStock, decimal PrecoUnitario, string MensagemErro)>
    VerificarStockEObterPrecoAsync(string pecaId, int quantidadeDesejada)
        {
            // Validação antes de chamar a API externa.
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
                var disponibilidadeResponse = await _httpClient.GetAsync(
                    $"api/pecas/{pecaId}/disponibilidade?quantidade={quantidadeDesejada}");

                if (!disponibilidadeResponse.IsSuccessStatusCode)
                {
                    return (false, 0,
                        "Não foi possível verificar a disponibilidade da peça.");
                }

                var temStock = await disponibilidadeResponse.Content
                    .ReadFromJsonAsync<bool>();

                if (temStock != true)
                {
                    return (false, 0,
                        "Não existe stock suficiente para a peça pedida.");
                }

                var pecaResponse = await _httpClient.GetAsync($"api/pecas/{pecaId}");

                if (!pecaResponse.IsSuccessStatusCode)
                {
                    return (false, 0, "A peça não foi encontrada no catálogo.");
                }

                var peca = await pecaResponse.Content
                    .ReadFromJsonAsync<RespostaPecaCatalogoDto>();

                if (peca is null)
                {
                    return (false, 0, "Não foi possível ler os dados da peça.");
                }

                if (!peca.Ativo)
                {
                    return (false, 0, $"A peça '{peca.Nome}' está inativa.");
                }

                return (true, peca.PrecoUnitario, string.Empty);
            }
            catch (HttpRequestException)
            {
                return (false, 0, "A PartsCatalog.API está indisponível.");
            }
            catch (TaskCanceledException)
            {
                return (false, 0,
                    "A PartsCatalog.API demorou demasiado tempo a responder.");
            }
            catch (JsonException)
            {
                return (false, 0,
                    "A resposta recebida da PartsCatalog.API não é válida.");
            }
        }

        //public async Task<(bool TemStock, decimal PrecoUnitario, string MensagemErro)> VerificarStockEObterPrecoAsync(string pecaId, int quantidadeDesejada)
        //{
        //    if (!Guid.TryParse(pecaId, out _))
        //    {
        //        return (false, 0, "O ID da peça não é válido.");
        //    }

        //    if (quantidadeDesejada <= 0)
        //    {
        //        return (false, 0, "A quantidade da peça deve ser superior a zero.");
        //    }

        //    try
        //    {
        //        var resposta = await _httpClient.GetAsync($"api/pecas/{pecaId}");

        //        if (!resposta.IsSuccessStatusCode)
        //        {
        //            return (false, 0, $"Peça #{pecaId} não foi encontrada no catálogo.");
        //        }

        //        var peca = await resposta.Content.ReadFromJsonAsync<RespostaPecaCatalogoDto>();

        //        if (peca == null)
        //        {
        //            return (false, 0, "Não foi possível ler os dados da peça.");
        //        }

        //        if (!peca.Ativo)
        //        {
        //            return (false, 0, $"A peça '{peca.Nome}' está inativa.");
        //        }

        //        if (peca.StockDisponivel < quantidadeDesejada)
        //        {
        //            return (false, 0,
        //                $"Stock insuficiente para '{peca.Nome}'. Disponível: {peca.StockDisponivel}.");
        //        }

        //        return (true, peca.PrecoUnitario, string.Empty);
        //    }
        //    catch (Exception ex)
        //    {
        //        return (false, 0, $"Não foi possível contactar o catálogo de peças: {ex.Message}");
        //    }
        //}
    }
}