using PartsCatalog.API.Data;
using PartsCatalog.API.Models;

using AutoMapper;
using PartsCatalog.API.DTOs;
using PartsCatalog.API.Repositories;

namespace PartsCatalog.API.Services
{
    public class PecaService : IPecaService
    {
        private readonly CatalogDbContext _context;
        private readonly IPecaRepository _repository;
        private readonly IMapper _mapper;

        public PecaService(CatalogDbContext context, IPecaRepository repository, IMapper mapper)
        {
            _context = context;
            _repository = repository;
            _mapper = mapper;
        }

        public IQueryable<Peca> GetPartsQuery()
        {
            return _context.Pecas.Where(p => p.Ativo).AsQueryable();
        }

        public async Task<PecaServiceResult> ObterPorIdAsync(Guid id)
        {
            var peca = await _repository.ObterPorIdAsync(id);
            return peca is null ? new(404, "Peça não encontrada.") : new(200, _mapper.Map<PecaResponse>(peca));
        }

        public async Task<PecaServiceResult> CriarAsync(CriarPecaRequest request)
        {
            var peca = _mapper.Map<Peca>(request);
            peca.Id = Guid.NewGuid();
            peca.Ativo = peca.StockDisponivel > 0;
            await _repository.CriarAsync(peca);
            return new(201, _mapper.Map<PecaResponse>(peca));
        }

        public async Task<PecaServiceResult> AtualizarAsync(Guid id, AtualizarPecaRequest request)
        {
            var peca = await _repository.ObterPorIdAsync(id);
            if (peca is null) return new(404, "Peça não encontrada.");
            _mapper.Map(request, peca);
            peca.Ativo = peca.StockDisponivel > 0;
            await _repository.AtualizarAsync(peca);
            return new(204);
        }

        public async Task<PecaServiceResult> RemoverAsync(Guid id)
        {
            if (await _repository.ObterPorIdAsync(id) is null) return new(404, "Peça não encontrada.");
            await _repository.RemoverAsync(id);
            return new(204);
        }

        public async Task<PecaServiceResult> InativarAsync(Guid id)
            => await _repository.InativarAsync(id) ? new(204) : new(404, "Peça não encontrada.");

        public async Task<PecaServiceResult> AtivarAsync(Guid id)
            => await _repository.AtivarAsync(id) ? new(204) : new(404, "Peça não encontrada.");

        public async Task<PecaServiceResult> VerificarDisponibilidadeAsync(Guid id, int quantidade)
        {
            if (quantidade <= 0) return new(400, "A quantidade deve ser maior que zero.");
            return new(200, await _repository.VerificarDisponibilidadeAsync(id, quantidade));
        }

        public async Task<PecaServiceResult> ObterTodasAdminAsync()
        {
            var pecas = await _repository.ObterTodasAsync();
            return new(200, _mapper.Map<IEnumerable<PecaResponse>>(pecas));
        }
    }
}
