using AutoMapper;
using PartsCatalog.API.DTOs;
using PartsCatalog.API.Models;

namespace PartsCatalog.API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Mapeia da Entidade Peca para o DTO de Resposta
            CreateMap<Peca, PecaResponse>();

            // Mapeia do DTO de Criação para a Entidade Peca
            CreateMap<CriarPecaRequest, Peca>();

            // Mapeia do DTO de Atualização para a Entidade Peca
            CreateMap<AtualizarPecaRequest, Peca>();
        }
    }
}