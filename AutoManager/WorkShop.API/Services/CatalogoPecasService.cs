using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using WorkShop.API.DTOs;

namespace WorkShop.API.Services
{
    public interface ICatalogoPecasService
    {
        Task<(bool TemStock, decimal PrecoUnitario, string MensagemErro)> VerificarStockEObterPrecoAsync(string pecaId, int quantidadeDesejada);
        Task<IEnumerable<RespostaPecaCatalogoDto>> ObterTodasAsPecasAdminAsync();
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

        private void AdicionarTokenCabecalho()
        {
            var token = _httpContextAccessor.HttpContext?.Request.Cookies["jwtToken"];
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
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
                AdicionarTokenCabecalho();
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

        public async Task<IEnumerable<RespostaPecaCatalogoDto>> ObterTodasAsPecasAdminAsync()
        {
            try
            {
                AdicionarTokenCabecalho();
                var response = await _httpClient.GetAsync("api/pecas/admin/todas");

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
                AdicionarTokenCabecalho();
                var responseGet = await _httpClient.GetAsync($"api/pecas/{pecaId}");
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

                AdicionarTokenCabecalho();
                var responsePut = await _httpClient.PutAsJsonAsync($"api/pecas/{pecaId}", payloadAtualizacao);
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