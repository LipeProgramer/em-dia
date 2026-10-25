using EmDia.Application.DTOs.Preferencias;
using EmDia.Application.Interfaces;
using EmDia.Application.Interfaces.Repositories;
using EmDia.Domain.Exceptions;

namespace EmDia.Application.Services;

/// <summary>
/// E27: GET /api/v1/usuarios/me/preferencias
/// E28: PUT /api/v1/usuarios/me/preferencias
/// </summary>
public class PreferenciasService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IRelogio _relogio;

    public PreferenciasService(IUsuarioRepository usuarios, IRelogio relogio)
    {
        _usuarios = usuarios;
        _relogio  = relogio;
    }

    /// <summary>E27 – obtém preferências do usuário logado.</summary>
    public async Task<PreferenciasResponse> ObterAsync(Guid usuarioId, CancellationToken ct)
    {
        var usuario = await _usuarios.ObterPorIdAsync(usuarioId, ct)
            ?? throw new NotFoundException("USUARIO_NAO_ENCONTRADO", "Usuário não encontrado.");

        var pref = usuario.Preferencias
            ?? throw new NotFoundException("PREFERENCIAS_NAO_ENCONTRADAS", "Preferências não encontradas.");

        return ToResponse(pref);
    }

    /// <summary>E28 – atualiza preferências.</summary>
    public async Task<PreferenciasResponse> AtualizarAsync(
        Guid usuarioId, AtualizarPreferenciasRequest req, CancellationToken ct)
    {
        var agora = _relogio.Agora;

        var usuario = await _usuarios.ObterPorIdAsync(usuarioId, ct)
            ?? throw new NotFoundException("USUARIO_NAO_ENCONTRADO", "Usuário não encontrado.");

        var pref = usuario.Preferencias
            ?? throw new NotFoundException("PREFERENCIAS_NAO_ENCONTRADAS", "Preferências não encontradas.");

        pref.Atualizar(
            req.ReceberEmail,
            req.MaxReavisos,
            req.IntervaloReavisoDias,
            req.ModoFocoPadrao,
            agora);

        await _usuarios.SalvarAsync(ct);

        return ToResponse(pref);
    }

    private static PreferenciasResponse ToResponse(Domain.Entities.PreferenciasAviso p) => new(
        p.ReceberEmail,
        p.MaxReavisos,
        p.IntervaloReavisoDias,
        p.ModoFocoPadrao,
        p.AtualizadoEm);
}
