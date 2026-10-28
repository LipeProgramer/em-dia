using EmDia.Domain.Enums;

// Contratos de entrada e saída da API (spec §3.4). As entidades nunca são expostas diretamente.
// Enums trafegam como texto e nomes em camelCase (configurado na Api).

namespace EmDia.Application.DTOs.Auth
{
    /// <summary>E01 · POST /auth/registrar</summary>
    public record RegistrarRequest(string Nome, string Email, string Senha);

    public record RegistrarResponse(Guid Id, string Nome, string Email);

    /// <summary>E02 · POST /auth/login</summary>
    public record LoginRequest(string Email, string Senha);

    public record UsuarioLogado(Guid Id, string Nome, bool PrecisaCheckup);

    public record LoginResponse(string AccessToken, DateTimeOffset ExpiraEm, UsuarioLogado Usuario);

    /// <summary>E03 · POST /auth/refresh</summary>
    public record RefreshResponse(string AccessToken, DateTimeOffset ExpiraEm);

    /// <summary>Resultado interno do login/refresh: resposta + valor do refresh token para o cookie.</summary>
    public record SessaoEmitida<T>(T Resposta, string RefreshToken, DateTimeOffset RefreshExpiraEm);
}

namespace EmDia.Application.DTOs.Usuarios
{
    /// <summary>E06 · PUT /usuarios/me/preferencias (entrada e saída).</summary>
    public record PreferenciasDto(bool ReceberEmail, short MaxReavisos, short IntervaloReavisoDias, bool ModoFocoPadrao);

    /// <summary>E05 · GET /usuarios/me</summary>
    public record UsuarioResponse(
        Guid Id,
        string Nome,
        string Email,
        DateTimeOffset? UltimoCheckupEm,
        bool PrecisaCheckup,
        PreferenciasDto Preferencias);

    /// <summary>E07 · DELETE /usuarios/me</summary>
    public record ExcluirContaRequest(string Senha);
}

namespace EmDia.Application.DTOs.Catalogo
{
    /// <summary>E08 · GET /categorias</summary>
    public record CategoriaResponse(short Id, string Nome, string Icone, short Ordem);

    /// <summary>E09 · GET /modelos</summary>
    public record ModeloResponse(
        Guid Id,
        string Nome,
        short CategoriaId,
        TipoVencimento TipoVencimento,
        int? IntervaloDias,
        int? IntervaloKm,
        short? GarantiaFabricanteMeses,
        short AntecedenciaDias,
        string? CodigoCheckup,
        IReadOnlyList<string> Passos);

    /// <summary>E10 · GET /checkup/perguntas</summary>
    public record PerguntaResponse(
        string Codigo,
        string Texto,
        string Ajuda,
        short CategoriaId,
        IReadOnlyList<ModeloResponse> Modelos);
}

namespace EmDia.Application.DTOs.Checkup
{
    public record RespostaCheckup(string Codigo, bool Resposta);

    public record VeiculoCheckup(string Ref, string Apelido, TipoVeiculo Tipo, int KmAtual, int MediaKmMes);

    /// <summary>
    /// Item escolhido no check-up. O veículo vem por <see cref="VeiculoRef"/> (cadastrado neste check-up)
    /// ou por <see cref="VeiculoId"/> (já existente, ao refazer o check-up).
    /// </summary>
    public record ItemCheckup(
        Guid ModeloId,
        string? VeiculoRef,
        Guid? VeiculoId,
        DateOnly? DataBase,
        int? KmBase,
        DateOnly? DataVencimento);

    /// <summary>E11 · POST /checkup</summary>
    public record CheckupRequest(
        IReadOnlyList<RespostaCheckup> Respostas,
        IReadOnlyList<VeiculoCheckup>? Veiculos,
        IReadOnlyList<ItemCheckup>? Itens);

    public record ResumoStatus(int Vencidos, int Atencao, int EmDia);

    /// <summary>Situação de uma área; Status nulo = nenhum item cadastrado nela.</summary>
    public record AreaCheckup(short CategoriaId, string Categoria, StatusItem? Status, int QuantidadeItens);

    public record DestaqueCheckup(Guid ItemId, string Nome, short CategoriaId, StatusItem Status, string TextoPrazo);

    public record CheckupResponse(
        Guid CheckupId,
        int ItensCriados,
        ResumoStatus Resumo,
        int AreasEmDia,
        int AreasSemItens,
        IReadOnlyList<AreaCheckup> Areas,
        IReadOnlyList<DestaqueCheckup> Destaques);
}

namespace EmDia.Application.DTOs.Itens
{
    using EmDia.Application.DTOs.Checkup;

    /// <summary>Formato resumido usado no Radar (E12) e na lista de itens (E13).</summary>
    public record ItemResumoResponse(
        Guid Id,
        string Nome,
        short CategoriaId,
        TipoVencimento TipoVencimento,
        StatusItem Status,
        AnelRadar Anel,
        int? DiasRestantes,
        int? KmRestantes,
        DateOnly? DataLimite,
        string TextoPrazo,
        double ProgressoPassos);

    /// <summary>E12 · GET /radar</summary>
    public record RadarResponse(ResumoStatus Resumo, bool ModoFoco, IReadOnlyList<ItemResumoResponse> Itens);

    public record VeiculoResumo(Guid Id, string Apelido);

    public record PassoResponse(Guid Id, short Ordem, string Descricao, bool Concluido);

    public record HistoricoResumo(Guid Id, DateOnly ConcluidoEm, int? Km, decimal? ValorGasto);

    /// <summary>E14 · GET /itens/{id}</summary>
    public record ItemDetalheResponse(
        Guid Id,
        string Nome,
        short CategoriaId,
        Guid? ModeloItemId,
        TipoVencimento TipoVencimento,
        SituacaoItem Situacao,
        StatusItem Status,
        AnelRadar Anel,
        string TextoPrazo,
        int? DiasRestantes,
        int? KmRestantes,
        DateOnly? DataLimite,
        DateOnly? ProximoVencimentoData,
        int? ProximoVencimentoKm,
        DateOnly? DataBase,
        DateOnly? DataVencimento,
        int? IntervaloDias,
        int? IntervaloKm,
        int? KmBase,
        short? GarantiaFabricanteMeses,
        short AntecedenciaDias,
        string? Observacao,
        VeiculoResumo? Veiculo,
        IReadOnlyList<PassoResponse> Passos,
        double ProgressoPassos,
        IReadOnlyList<HistoricoResumo> Historico);

    /// <summary>E15 · POST /itens e E16 · PUT /itens/{id}</summary>
    public record SalvarItemRequest(
        string Nome,
        short CategoriaId,
        Guid? ModeloId,
        TipoVencimento TipoVencimento,
        DateOnly? DataBase,
        DateOnly? DataVencimento,
        int? IntervaloDias,
        int? IntervaloKm,
        int? KmBase,
        short? GarantiaFabricanteMeses,
        Guid? VeiculoId,
        short AntecedenciaDias,
        string? Observacao,
        IReadOnlyList<string>? Passos);

    /// <summary>E18 · POST /itens/{id}/concluir</summary>
    public record ConcluirRequest(
        DateOnly? DataConclusao,
        int? Km,
        decimal? ValorGasto,
        string? Observacao,
        bool ConfirmarPassosPendentes,
        DateOnly? NovaDataVencimento);

    public record ConcluirResponse(ItemDetalheResponse Item, Guid HistoricoId, string Mensagem);

    /// <summary>E19 · PATCH /itens/{id}/passos/{passoId}</summary>
    public record MarcarPassoRequest(bool? Concluido);

    public record MarcarPassoResponse(Guid PassoId, bool Concluido, double ProgressoPassos);
}

namespace EmDia.Application.DTOs.Veiculos
{
    /// <summary>E21 · POST /veiculos</summary>
    public record CriarVeiculoRequest(string Apelido, TipoVeiculo Tipo, int KmAtual, int MediaKmMes);

    /// <summary>E22 · PUT /veiculos/{id}</summary>
    public record EditarVeiculoRequest(string Apelido, TipoVeiculo Tipo, int MediaKmMes);

    /// <summary>E23 · PATCH /veiculos/{id}/km</summary>
    public record AtualizarKmRequest(int KmAtual);

    public record ItemAfetado(Guid Id, string Nome, StatusItem Status);

    public record AtualizarKmResponse(int KmAtual, IReadOnlyList<ItemAfetado> ItensAfetados);

    /// <summary>E20 · GET /veiculos</summary>
    public record VeiculoResponse(
        Guid Id,
        string Apelido,
        TipoVeiculo Tipo,
        int KmAtual,
        int MediaKmMes,
        DateTimeOffset KmAtualizadoEm,
        int QuantidadeItens);
}

namespace EmDia.Application.DTOs.Historico
{
    /// <summary>E25 · GET /historico</summary>
    public record HistoricoResponse(
        Guid Id,
        Guid ItemId,
        string ItemNome,
        DateOnly ConcluidoEm,
        int? Km,
        decimal? ValorGasto,
        string? Observacao,
        DateOnly? VencimentoAnterior,
        DateOnly? ProximoVencimento);
}
