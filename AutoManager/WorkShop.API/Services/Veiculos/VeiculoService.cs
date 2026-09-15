using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using WorkShop.API.Data;
using WorkShop.API.DTOs;
using WorkShop.API.Models;
using WorkShop.API.Services.Auth;

namespace WorkShop.API.Services.Veiculos;

public class VeiculoService : IVeiculoService
{
    private static readonly Regex RegexMatricula = new(
        @"^(?:[A-Z]{2}-\d{2}-\d{2}|\d{2}-[A-Z]{2}-\d{2}|\d{2}-\d{2}-[A-Z]{2}|[A-Z]{2}-\d{2}-[A-Z]{2})$",
        RegexOptions.Compiled);

    private readonly WorkshopContext _contexto;
    private readonly IUserContextService _userContextService;

    public VeiculoService(WorkshopContext contexto, IUserContextService userContextService)
    {
        _contexto = contexto;
        _userContextService = userContextService;
    }

    public async Task<VeiculoServiceResult> ObterTodosAsync()
        => new(200, await CriarQueryResposta().OrderBy(v => v.Id).ToListAsync());

    public async Task<VeiculoServiceResult> CriarAsync(CriarVeiculoDto dto)
    {
        var validacao = await ValidarDadosAsync(dto);
        if (validacao.Erro is not null) return new(400, new { message = validacao.Erro });

        if (await _contexto.Veiculos.AnyAsync(v => v.Matricula.ToUpper() == validacao.Matricula))
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
        _contexto.Veiculos.Add(veiculo);
        await _contexto.SaveChangesAsync();
        return new(201, Mapear(veiculo));
    }

    public async Task<VeiculoServiceResult> ObterPorIdAsync(int id)
    {
        var veiculo = await _contexto.Veiculos.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
        return veiculo is null ? new(404) : new(200, Mapear(veiculo));
    }

    public async Task<VeiculoServiceResult> AtualizarAsync(int id, CriarVeiculoDto dto)
    {
        var validacao = await ValidarDadosAsync(dto);
        if (validacao.Erro is not null) return new(400, new { message = validacao.Erro });

        if (await _contexto.Veiculos.AnyAsync(v => v.Id != id && v.Matricula.ToUpper() == validacao.Matricula))
            return new(400, new { message = "Já existe outro veículo registado com esta matrícula." });

        var veiculo = await _contexto.Veiculos.FindAsync(id);
        if (veiculo is null) return new(404, new { message = "Veículo não encontrado." });

        veiculo.Matricula = validacao.Matricula!;
        veiculo.Marca = dto.Marca.Trim();
        veiculo.Modelo = dto.Modelo.Trim();
        veiculo.Ano = dto.Ano;
        await _contexto.SaveChangesAsync();
        return new(204);
    }

    public async Task<VeiculoServiceResult> ObterMeusAsync()
    {
        var clienteId = _userContextService.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(clienteId))
            return new(401, new { message = "Não foi possível identificar o cliente através do token." });

        return new(200, await CriarQueryResposta().Where(v => v.ClienteId == clienteId).OrderBy(v => v.Id).ToListAsync());
    }

    public async Task<VeiculoServiceResult> EliminarAsync(int id)
    {
        var veiculo = await _contexto.Veiculos.FindAsync(id);
        if (veiculo is null) return new(404, "Veículo não encontrado.");
        _contexto.Veiculos.Remove(veiculo);
        await _contexto.SaveChangesAsync();
        return new(204);
    }

    private IQueryable<RespostaVeiculoDto> CriarQueryResposta() => _contexto.Veiculos.AsNoTracking().Select(v => new RespostaVeiculoDto
    {
        Id = v.Id, Matricula = v.Matricula, Marca = v.Marca, Modelo = v.Modelo, Ano = v.Ano, ClienteId = v.ClienteId
    });

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
