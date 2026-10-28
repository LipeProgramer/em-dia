using EmDia.Application.DTOs.Checkup;
using EmDia.Application.Interfaces;
using EmDia.Application.Interfaces.Repositories;
using EmDia.Domain.Entities;
using EmDia.Domain.Enums;
using EmDia.Domain.Exceptions;
using EmDia.Domain.Services;
using FluentValidation;
using FluentValidation.Results;

namespace EmDia.Application.Services;

/// <summary>E11: registra o check-up e cria veículos e itens de uma vez (RN10, RN11, RN01, RN02, RN14).</summary>
public class CheckupService(
    IUsuarioRepository usuarios,
    ICatalogoRepository catalogo,
    IVeiculoRepository veiculos,
    IItemRepository itens,
    ICheckupRepository checkups,
    IUnitOfWork uow,
    IRelogio relogio,
    IValidator<CheckupRequest> validador)
{
    public async Task<CheckupResponse> RealizarAsync(Guid usuarioId, CheckupRequest req, CancellationToken ct)
    {
        await validador.ValidateAndThrowAsync(req, ct);
        var agora = relogio.Agora;
        var hoje = relogio.Hoje;

        var usuario = await usuarios.ObterPorIdAsync(usuarioId, ct)
            ?? throw new NotFoundException("USUARIO_NAO_ENCONTRADO", "Não encontramos sua conta.");
        var modelos = (await catalogo.ListarModelosAtivosAsync(null, ct)).ToDictionary(m => m.Id);
        var categorias = await catalogo.ListarCategoriasAsync(ct);
        var veiculosExistentes = (await veiculos.ListarPorUsuarioAsync(usuarioId, ct)).ToDictionary(v => v.Id);
        var itensDoUsuario = (await itens.ListarPorUsuarioAsync(usuarioId, ct)).ToList();

        // ── Validações que dependem do catálogo ──────────────────────────────
        var codigosValidos = modelos.Values.Select(m => m.CodigoCheckup).OfType<string>().ToHashSet();
        var respostasSim = new HashSet<string>();
        var erros = new List<ValidationFailure>();

        for (int i = 0; i < req.Respostas.Count; i++)
        {
            var r = req.Respostas[i];
            if (!codigosValidos.Contains(r.Codigo))
                erros.Add(new($"respostas[{i}].codigo", "Pergunta inexistente."));
            else if (r.Resposta)
                respostasSim.Add(r.Codigo);
        }

        var refsVeiculos = (req.Veiculos ?? []).Select(v => v.Ref).ToList();
        if (refsVeiculos.Count != refsVeiculos.Distinct().Count())
            erros.Add(new("veiculos", "Use uma referência diferente para cada veículo."));

        var itensReq = req.Itens ?? [];
        for (int i = 0; i < itensReq.Count; i++)
        {
            var it = itensReq[i];
            if (!modelos.TryGetValue(it.ModeloId, out var modelo))
            {
                erros.Add(new($"itens[{i}].modeloId", "Modelo inexistente."));
                continue;
            }
            if (modelo.CodigoCheckup is null || !respostasSim.Contains(modelo.CodigoCheckup))
                erros.Add(new($"itens[{i}].modeloId", "Este item não está ligado a uma resposta \"sim\"."));
            if (it.VeiculoRef is not null && !refsVeiculos.Contains(it.VeiculoRef))
                erros.Add(new($"itens[{i}].veiculoRef", "Veículo não encontrado neste check-up."));
            if (it.VeiculoId is { } vid && !veiculosExistentes.ContainsKey(vid))
                erros.Add(new($"itens[{i}].veiculoId", "Veículo não encontrado."));

            switch (modelo.TipoVencimento)
            {
                case TipoVencimento.DataUnica when it.DataVencimento is null:
                    erros.Add(new($"itens[{i}].dataVencimento", "Informe a data de vencimento."));
                    break;
                case TipoVencimento.RecorrentePorTempo or TipoVencimento.Calculado when it.DataBase is null:
                    erros.Add(new($"itens[{i}].dataBase", "Informe a data da última vez."));
                    break;
                case TipoVencimento.UsoOuTempo:
                    if (it.DataBase is null) erros.Add(new($"itens[{i}].dataBase", "Informe a data da última vez."));
                    if (it.KmBase is null) erros.Add(new($"itens[{i}].kmBase", "Informe a km da última vez."));
                    break;
            }
        }

        if (erros.Count > 0)
            throw new ValidationException(erros);

        // 422: item sem o veículo exigido pelo tipo
        foreach (var it in itensReq)
            if (modelos[it.ModeloId].ExigeVeiculo && it.VeiculoRef is null && it.VeiculoId is null)
                throw new DomainException(CodigosErro.VeiculoObrigatorio,
                    $"O item \"{modelos[it.ModeloId].Nome}\" precisa de um veículo.");

        // ── Criação (tudo em uma transação: um único SaveChanges) ────────────
        var veiculosPorRef = new Dictionary<string, Veiculo>();
        foreach (var v in req.Veiculos ?? [])
        {
            if (veiculosPorRef.Values.Any(x => x.Apelido.Equals(v.Apelido.Trim(), StringComparison.OrdinalIgnoreCase)))
                throw new ConflictException(CodigosErro.ApelidoDuplicado, $"Há dois veículos chamados \"{v.Apelido}\".");

            // Ao refazer o check-up, um veículo com o mesmo apelido é reaproveitado (não duplica — RN10)
            var existente = veiculosExistentes.Values.FirstOrDefault(x =>
                x.Apelido.Equals(v.Apelido.Trim(), StringComparison.OrdinalIgnoreCase));
            if (existente is not null)
            {
                veiculosPorRef[v.Ref] = existente;
                continue;
            }

            var veiculo = new Veiculo(usuarioId, v.Apelido, v.Tipo, v.KmAtual, v.MediaKmMes, agora);
            await veiculos.AdicionarAsync(veiculo, ct);
            veiculosPorRef[v.Ref] = veiculo;
        }

        var veiculoPorId = veiculosExistentes.Values.Concat(veiculosPorRef.Values).DistinctBy(v => v.Id).ToDictionary(v => v.Id);
        int criados = 0;

        foreach (var it in itensReq)
        {
            var modelo = modelos[it.ModeloId];
            var veiculo = it.VeiculoRef is not null ? veiculosPorRef[it.VeiculoRef]
                        : it.VeiculoId is { } id ? veiculoPorId[id]
                        : null;

            // RN10: não duplica item do mesmo modelo (e mesmo veículo) já ativo
            if (CheckupDomain.ItemJaExiste(itensDoUsuario, modelo.Id, veiculo?.Id))
                continue;

            if (modelo.TipoVencimento == TipoVencimento.UsoOuTempo && veiculo is not null && it.KmBase > veiculo.KmAtual)
                throw new CampoInvalidoException("kmBase",
                    $"A km da última troca não pode ser maior que a km atual ({Domain.Formatos.Km(veiculo.KmAtual)}).");

            var item = new Item(
                usuarioId,
                modelo.CategoriaId,
                modelo.Nome,
                modelo.TipoVencimento,
                modelo.TipoVencimento == TipoVencimento.DataUnica ? null : it.DataBase,
                modelo.TipoVencimento == TipoVencimento.DataUnica ? it.DataVencimento : null,
                modelo.IntervaloDias,
                modelo.TipoVencimento == TipoVencimento.UsoOuTempo ? modelo.IntervaloKm : null,
                modelo.TipoVencimento == TipoVencimento.UsoOuTempo ? it.KmBase : null,
                modelo.TipoVencimento == TipoVencimento.Calculado ? modelo.GarantiaFabricanteMeses : null,
                modelo.AntecedenciaDias,
                modelo.Id,
                veiculo?.Id,
                null,
                agora);

            // RN14: passos sugeridos do modelo são copiados
            foreach (var passo in modelo.Passos.OrderBy(p => p.Ordem))
                item.AdicionarPasso(passo.Descricao);

            await itens.AdicionarAsync(item, ct);
            itensDoUsuario.Add(item);
            criados++;
        }

        var checkup = new Checkup(usuarioId, req.Respostas.ToDictionary(r => r.Codigo, r => r.Resposta),
            (short)criados, agora);
        await checkups.AdicionarAsync(checkup, ct);
        usuario.RegistrarCheckup(agora);

        await uow.SalvarAsync(ct);

        // ── RN11: resultado por status e por área ────────────────────────────
        Veiculo? VeiculoDo(Item i) => i.VeiculoId is { } id ? veiculoPorId.GetValueOrDefault(id) : null;
        var resultado = RadarDomain.GerarResultado(itensDoUsuario, categorias.Select(c => c.Id), hoje, VeiculoDo);
        var nomes = categorias.ToDictionary(c => c.Id, c => c.Nome);

        var destaques = RadarDomain.OrdenarPorUrgencia(itensDoUsuario.Where(i => !i.Encerrado), hoje, VeiculoDo)
            .Where(i => i.CalcularStatus(hoje, VeiculoDo(i)) != StatusItem.EmDia)
            .Take(RadarDomain.ItensModoFoco)
            .Select(i => new DestaqueCheckup(i.Id, i.Nome, i.CategoriaId, i.CalcularStatus(hoje, VeiculoDo(i)), i.TextoPrazo(hoje, VeiculoDo(i))))
            .ToList();

        return new CheckupResponse(
            checkup.Id,
            criados,
            new ResumoStatus(resultado.Vencidos, resultado.Atencao, resultado.EmDia),
            resultado.AreasEmDia,
            resultado.Areas.Select(a => new AreaCheckup(a.CategoriaId, nomes[a.CategoriaId], a.Status, a.QuantidadeItens)).ToList(),
            destaques);
    }
}
