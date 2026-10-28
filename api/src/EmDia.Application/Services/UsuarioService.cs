using EmDia.Application.DTOs.Usuarios;
using EmDia.Application.Interfaces;
using EmDia.Application.Interfaces.Repositories;
using EmDia.Domain.Entities;
using EmDia.Domain.Exceptions;
using FluentValidation;

namespace EmDia.Application.Services;

/// <summary>Conta e preferências (E05–E07).</summary>
public class UsuarioService(
    IUsuarioRepository usuarios,
    IUnitOfWork uow,
    IPasswordHasher hasher,
    IRelogio relogio,
    IValidator<PreferenciasDto> validadorPreferencias,
    IValidator<ExcluirContaRequest> validadorExclusao)
{
    /// <summary>E05: dados da conta e preferências. Nunca devolve o hash da senha.</summary>
    public async Task<UsuarioResponse> ObterAsync(Guid usuarioId, CancellationToken ct)
        => ParaResposta(await ObterUsuarioAsync(usuarioId, ct));

    /// <summary>E06: altera as preferências de aviso; valem para os próximos ciclos (RN09).</summary>
    public async Task<PreferenciasDto> AtualizarPreferenciasAsync(Guid usuarioId, PreferenciasDto req, CancellationToken ct)
    {
        await validadorPreferencias.ValidateAndThrowAsync(req, ct);
        var usuario = await ObterUsuarioAsync(usuarioId, ct);

        usuario.Preferencias!.Atualizar(req.ReceberEmail, req.MaxReavisos, req.IntervaloReavisoDias,
            req.ModoFocoPadrao, relogio.Agora);
        await uow.SalvarAsync(ct);

        return ParaDto(usuario.Preferencias);
    }

    /// <summary>E07 (RF18 · LGPD): exclui a conta e todos os dados, após confirmar a senha.</summary>
    public async Task ExcluirContaAsync(Guid usuarioId, ExcluirContaRequest req, CancellationToken ct)
    {
        await validadorExclusao.ValidateAndThrowAsync(req, ct);
        var usuario = await ObterUsuarioAsync(usuarioId, ct);

        if (!hasher.Verificar(req.Senha, usuario.SenhaHash))
            throw new DomainException(CodigosErro.SenhaIncorreta, "A senha de confirmação está incorreta.");

        await usuarios.ExcluirComDadosAsync(usuario, ct);
        await uow.SalvarAsync(ct);
    }

    private async Task<Usuario> ObterUsuarioAsync(Guid usuarioId, CancellationToken ct) =>
        await usuarios.ObterPorIdAsync(usuarioId, ct)
        ?? throw new NotFoundException("USUARIO_NAO_ENCONTRADO", "Não encontramos sua conta.");

    private static UsuarioResponse ParaResposta(Usuario u) =>
        new(u.Id, u.Nome, u.Email, u.UltimoCheckupEm, u.PrecisaCheckup, ParaDto(u.Preferencias!));

    private static PreferenciasDto ParaDto(PreferenciasAviso p) =>
        new(p.ReceberEmail, p.MaxReavisos, p.IntervaloReavisoDias, p.ModoFocoPadrao);
}
