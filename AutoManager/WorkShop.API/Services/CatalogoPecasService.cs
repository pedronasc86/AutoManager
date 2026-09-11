using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using WorkShop.API.DTOs;
using System.Text.Json;
using System.Net.Http.Headers;

namespace WorkShop.API.Services
{
    public interface ICatalogoPecasService
    {
        Task<(bool TemStock, decimal PrecoUnitario, string MensagemErro)> VerificarStockEObterPrecoAsync(string pecaId, int quantidadeDesejada);
        Task<IEnumerable<RespostaPecaCatalogoDto>> ObterTodasAsPecasAdminAsync();
        Task<List<RespostaPecaCatalogoDto>> ObterPecasAsync();
    }

    public class CatalogoPecasService : ICatalogoPecasService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CatalogoPecasService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        // Método auxiliar para criar um HttpRequestMessage com o token do utilizador atual de forma isolada
        private HttpRequestMessage CriarMensagemComToken(HttpMethod metodo, string url, HttpContent? content = null)
        {
            var request = new HttpRequestMessage(metodo, url);

            if (content != null)
            {
                request.Content = content;
            }

            var token = _httpContextAccessor.HttpContext?.Request.Cookies["jwtToken"];
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return request;
        }

        public async Task<List<RespostaPecaCatalogoDto>> ObterPecasAsync()
        {
            try
            {
                var request = CriarMensagemComToken(HttpMethod.Get, "api/pecas");
                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    return new List<RespostaPecaCatalogoDto>();
                }

                return await response.Content.ReadFromJsonAsync<List<RespostaPecaCatalogoDto>>()
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
                var requestDisponibilidade = CriarMensagemComToken(HttpMethod.Get, $"api/pecas/{pecaId}/disponibilidade?quantidade={quantidadeDesejada}");
                var disponibilidadeResponse = await _httpClient.SendAsync(requestDisponibilidade);

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

                var requestPeca = CriarMensagemComToken(HttpMethod.Get, $"api/pecas/{pecaId}");
                var pecaResponse = await _httpClient.SendAsync(requestPeca);

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

        public async Task<IEnumerable<RespostaPecaCatalogoDto>> ObterTodasAsPecasAdminAsync()
        {
            try
            {
                var request = CriarMensagemComToken(HttpMethod.Get, "api/pecas/admin/todas");
                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    return Enumerable.Empty<RespostaPecaCatalogoDto>();
                }

                var pecas = await response.Content.ReadFromJsonAsync<IEnumerable<RespostaPecaCatalogoDto>>();
                return pecas ?? Enumerable.Empty<RespostaPecaCatalogoDto>();
            }
            catch
            {
                return Enumerable.Empty<RespostaPecaCatalogoDto>();
            }
        }

        public async Task AtualizarStockAsync(string pecaId, int quantidadeUsada)
        {
            Console.WriteLine($"[LOG] A atualizar stock para a peça {pecaId}, a reduzir {quantidadeUsada} unidades...");

            try
            {
                var requestGet = CriarMensagemComToken(HttpMethod.Get, $"api/pecas/{pecaId}");
                var responseGet = await _httpClient.SendAsync(requestGet);

                if (!responseGet.IsSuccessStatusCode)
                {
                    var erroGet = await responseGet.Content.ReadAsStringAsync();
                    Console.WriteLine($"[ERRO GET] Status: {responseGet.StatusCode} | Detalhe: {erroGet}");
                    return;
                }

                var peca = await responseGet.Content.ReadFromJsonAsync<PecaResponse>();
                if (peca == null) return;

                int novoStock = peca.StockDisponivel - quantidadeUsada;
                if (novoStock < 0) novoStock = 0;

                var payloadAtualizacao = new
                {
                    nome = peca.Nome,
                    referenciaPeca = peca.ReferenciaPeca,
                    categoria = peca.Categoria,
                    compatibilidade = peca.Compatibilidade,
                    precoUnitario = peca.PrecoUnitario,
                    stockDisponivel = novoStock,
                    ativo = peca.Ativo
                };

                var content = JsonContent.Create(payloadAtualizacao);
                var requestPut = CriarMensagemComToken(HttpMethod.Put, $"api/pecas/{pecaId}", content);
                var responsePut = await _httpClient.SendAsync(requestPut);

                if (!responsePut.IsSuccessStatusCode)
                {
                    var erroPut = await responsePut.Content.ReadAsStringAsync();
                    Console.WriteLine($"[ERRO PUT] Status: {responsePut.StatusCode} | Detalhe: {erroPut}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EXCEÇÃO] {ex.Message}");
            }
        }
    }
}