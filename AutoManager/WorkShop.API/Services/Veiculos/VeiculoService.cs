using System.Text.RegularExpressions;
using WorkShop.API.DTOs;
using WorkShop.API.Models;
using WorkShop.API.Repositories;
using WorkShop.API.Services.Auth;

namespace WorkShop.API.Services.Veiculos;

public class VeiculoService : IVeiculoService
{
    private static readonly Regex RegexMatricula = new(
        @"^(?:[A-Z]{2}-\d{2}-\d{2}|\d{2}-[A-Z]{2}-\d{2}|\d{2}-\d{2}-[A-Z]{2}|[A-Z]{2}-\d{2}-[A-Z]{2})$",
        RegexOptions.Compiled);

    private readonly IWorkshopRepository _repository;
    private readonly IUserContextService _userContextService;

    public VeiculoService(IWorkshopRepository repository, IUserContextService userContextService)
    {
        _repository = repository;
        _userContextService = userContextService;
    }

    public async Task<VeiculoServiceResult> ObterTodosAsync()
        => new(200, (await _repository.ObterVeiculosAsync()).Select(Mapear));

    public async Task<VeiculoServiceResult> CriarAsync(CriarVeiculoDto dto)
    {
        var validacao = await ValidarDadosAsync(dto);
        if (validacao.Erro is not null) return new(400, new { message = validacao.Erro });

        if (await _repository.MatriculaExisteAsync(validacao.Matricula!))
            return new(400, new { message = "Já existe um veículo registado com esta matrícula." });

        var clienteId = _userContextService.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(clienteId))
            return new(401, new { message = "Não foi possível identificar o cliente através do token." });

        var veiculo = new Veiculo
        {
            Matricula = validacao.Matricula!,
            Marca = dto.Marca.Trim(),
            Modelo = dto.Modelo.Trim(),
            Ano = dto.Ano,
            ClienteId = clienteId
        };
        await _repository.AdicionarVeiculoAsync(veiculo);
        return new(201, Mapear(veiculo));
    }

    public async Task<VeiculoServiceResult> ObterPorIdAsync(int id)
    {
        var veiculo = await _repository.ObterVeiculoAsync(id, semRastreio: true);
        return veiculo is null ? new(404) : new(200, Mapear(veiculo));
    }

    public async Task<VeiculoServiceResult> AtualizarAsync(int id, CriarVeiculoDto dto)
    {
        var validacao = await ValidarDadosAsync(dto);
        if (validacao.Erro is not null) return new(400, new { message = validacao.Erro });

        if (await _repository.MatriculaExisteAsync(validacao.Matricula!, id))
            return new(400, new { message = "Já existe outro veículo registado com esta matrícula." });

        var veiculo = await _repository.ObterVeiculoAsync(id);
        if (veiculo is null) return new(404, new { message = "Veículo não encontrado." });

        veiculo.Matricula = validacao.Matricula!;
        veiculo.Marca = dto.Marca.Trim();
        veiculo.Modelo = dto.Modelo.Trim();
        veiculo.Ano = dto.Ano;
        await _repository.GuardarAsync();
        return new(204);
    }

    public async Task<VeiculoServiceResult> ObterMeusAsync()
    {
        var clienteId = _userContextService.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(clienteId))
            return new(401, new { message = "Não foi possível identificar o cliente através do token." });

        return new(200, (await _repository.ObterVeiculosAsync(clienteId)).Select(Mapear));
    }

    public async Task<VeiculoServiceResult> EliminarAsync(int id)
    {
        var veiculo = await _repository.ObterVeiculoAsync(id);
        if (veiculo is null) return new(404, "Veículo não encontrado.");
        await _repository.RemoverVeiculoAsync(veiculo);
        return new(204);
    }

    private Task<(string? Matricula, string? Erro)> ValidarDadosAsync(CriarVeiculoDto dto)
    {
        var matricula = dto.Matricula?.Trim().ToUpperInvariant() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(matricula)) return Task.FromResult<(string?, string?)>((null, "A matrícula é obrigatória."));
        if (!RegexMatricula.IsMatch(matricula)) return Task.FromResult<(string?, string?)>((null, "Formato de matrícula inválido. Formatos aceites: 00-AA-00, AA-00-AA, 00-00-AA ou AA-00-00."));
        if (string.IsNullOrWhiteSpace(dto.Marca)) return Task.FromResult<(string?, string?)>((null, "A marca é obrigatória."));
        if (string.IsNullOrWhiteSpace(dto.Modelo)) return Task.FromResult<(string?, string?)>((null, "O modelo é obrigatório."));
        if (dto.Ano < 1900 || dto.Ano > DateTime.UtcNow.Year) return Task.FromResult<(string?, string?)>((null, $"O ano do veículo tem de estar compreendido entre 1900 e {DateTime.UtcNow.Year}."));
        return Task.FromResult<(string?, string?)>((matricula, null));
    }

    private static RespostaVeiculoDto Mapear(Veiculo veiculo) => new()
    {
        Id = veiculo.Id, Matricula = veiculo.Matricula, Marca = veiculo.Marca, Modelo = veiculo.Modelo, Ano = veiculo.Ano, ClienteId = veiculo.ClienteId
    };
}
