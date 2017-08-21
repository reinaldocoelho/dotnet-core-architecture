﻿using AutoMapper;
using Domain.Usuarios;
using Domain.Usuarios.Endereco;
using Infra.Helpers;
using UsuariosAPI.Models.Usuarios;
using UsuariosAPI.Models.Usuarios.Endereco;

namespace UsuariosAPI.Mappings.Profiles
{
    public class UsuariosProfile : Profile
    {
        public UsuariosProfile()
        {
            // Entity -> Model

            CreateMap<Usuario, UsuarioGetModel>()
                // Formatando o Nome Completo do Usuário a partir das propriedades Nome e Sobrenome
                .ForMember(t => t.NomeCompleto, opt => opt.MapFrom(s => $"{s.Nome} {s.Sobrenome}"))
                // Formatando a Idade do usuário a partir da sua Data de Nascimento
                .ForMember(t => t.Idade, opt => opt.MapFrom(s => s.DataNascimento.GetCurrentAge()))
                // Formatando o Sexo do usuário a partir do enum SexoType
                .ForMember(t => t.Sexo, opt => opt.MapFrom(s => (s.Sexo.HasValue) ? s.Sexo.Value.GetDescription() : null));

            CreateMap<UsuarioEndereco, UsuarioEnderecoGetModel>()
                // Formatando o Tipo do endereçoa partir do enum EnderecoType
                .ForMember(t => t.Tipo, opt => opt.MapFrom(s => s.Tipo.GetDescription()));

            // Model -> Entity
        }
    }
}