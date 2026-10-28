using EmDia.Domain.Entities;
using EmDia.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EmDia.Infrastructure.Data.Seed;

/// <summary>
/// Dados iniciais criados por migration (spec §2.7).
/// Categorias (6), modelos e passos — usados no check-up e no cadastro rápido.
/// IDs fixos (determinísticos) para que migrations subsequentes não os recrie.
/// </summary>
public static class SeedData
{
    public static void Aplicar(ModelBuilder b)
    {
        SeedCategorias(b);
        SeedModelos(b);
        SeedPassosModelo(b);
    }

    // ── Categorias (spec §2.3) ────────────────────────────────────────────────
    private static void SeedCategorias(ModelBuilder b)
    {
        b.Entity<Categoria>().HasData(
            new Categoria(1, "Veículo",    "car",        1),
            new Categoria(2, "Documentos", "id-card",    2),
            new Categoria(3, "Casa",       "home",       3),
            new Categoria(4, "Saúde",      "heart",      4),
            new Categoria(5, "Finanças",   "dollar",     5),
            new Categoria(6, "Garantias",  "shield-check", 6)
        );
    }

    // ── Modelos (spec §2.7) ───────────────────────────────────────────────────
    // IDs fixos para idempotência das migrations
    private static readonly Guid IdTrocaOleo       = new("a7000000-0000-0000-0000-000000000001");
    private static readonly Guid IdIpva            = new("a7000000-0000-0000-0000-000000000002");
    private static readonly Guid IdCnh             = new("a7000000-0000-0000-0000-000000000003");
    private static readonly Guid IdFiltroPurif     = new("a7000000-0000-0000-0000-000000000004");
    private static readonly Guid IdVacinaPet       = new("a7000000-0000-0000-0000-000000000005");
    private static readonly Guid IdGarantiaEletro  = new("a7000000-0000-0000-0000-000000000006");
    private static readonly Guid IdLicenciamento   = new("a7000000-0000-0000-0000-000000000007");
    private static readonly Guid IdPassaporte      = new("a7000000-0000-0000-0000-000000000008");
    private static readonly Guid IdConsultaRotina  = new("a7000000-0000-0000-0000-000000000009");
    private static readonly Guid IdDeclaracaoIr    = new("a7000000-0000-0000-0000-000000000010");
    // Check-up por área: mais perguntas em cada uma das 6 áreas
    private static readonly Guid IdSeguroVeiculo   = new("a7000000-0000-0000-0000-000000000011");
    private static readonly Guid IdCertificado     = new("a7000000-0000-0000-0000-000000000012");
    private static readonly Guid IdAluguel         = new("a7000000-0000-0000-0000-000000000013");
    private static readonly Guid IdArCondicionado  = new("a7000000-0000-0000-0000-000000000014");
    private static readonly Guid IdDentista        = new("a7000000-0000-0000-0000-000000000015");
    private static readonly Guid IdExameVista      = new("a7000000-0000-0000-0000-000000000016");
    private static readonly Guid IdIptu            = new("a7000000-0000-0000-0000-000000000017");
    private static readonly Guid IdSeguroVida      = new("a7000000-0000-0000-0000-000000000018");
    private static readonly Guid IdGarantiaEletrod = new("a7000000-0000-0000-0000-000000000019");

    /// <summary>Modelos complementares aos exemplos da spec §2.7, para cobrir as 6 áreas e as 8 perguntas.</summary>
    private static object Modelo(Guid id, short categoria, string nome, TipoVencimento tipo, int? dias,
        short antecedencia, string codigo) => new
    {
        Id = id,
        CategoriaId = categoria,
        Nome = nome,
        TipoVencimento = tipo,
        IntervaloDias = dias,
        IntervaloKm = (int?)null,
        GarantiaFabricanteMeses = (short?)null,
        AntecedenciaDias = antecedencia,
        CodigoCheckup = codigo,
        Ativo = true
    };

    private static void SeedModelos(ModelBuilder b)
    {
        b.Entity<ModeloItem>().HasData(
            // Veículo — Troca de óleo (UsoOuTempo)
            new
            {
                Id = IdTrocaOleo,
                CategoriaId = (short)1,
                Nome = "Troca de óleo",
                TipoVencimento = TipoVencimento.UsoOuTempo,
                IntervaloDias = (int?)365,
                IntervaloKm = (int?)10_000,
                GarantiaFabricanteMeses = (short?)null,
                AntecedenciaDias = (short)30,
                CodigoCheckup = "TEM_CARRO",
                Ativo = true
            },
            // Veículo — IPVA (Recorrente)
            new
            {
                Id = IdIpva,
                CategoriaId = (short)1,
                Nome = "IPVA",
                TipoVencimento = TipoVencimento.RecorrentePorTempo,
                IntervaloDias = (int?)365,
                IntervaloKm = (int?)null,
                GarantiaFabricanteMeses = (short?)null,
                AntecedenciaDias = (short)30,
                CodigoCheckup = "TEM_CARRO",
                Ativo = true
            },
            // Documentos — CNH (DataUnica)
            new
            {
                Id = IdCnh,
                CategoriaId = (short)2,
                Nome = "CNH",
                TipoVencimento = TipoVencimento.DataUnica,
                IntervaloDias = (int?)null,
                IntervaloKm = (int?)null,
                GarantiaFabricanteMeses = (short?)null,
                AntecedenciaDias = (short)45,
                CodigoCheckup = "TEM_CNH",
                Ativo = true
            },
            // Casa — Filtro do purificador (Recorrente)
            new
            {
                Id = IdFiltroPurif,
                CategoriaId = (short)3,
                Nome = "Filtro do purificador",
                TipoVencimento = TipoVencimento.RecorrentePorTempo,
                IntervaloDias = (int?)180,
                IntervaloKm = (int?)null,
                GarantiaFabricanteMeses = (short?)null,
                AntecedenciaDias = (short)15,
                CodigoCheckup = "TEM_PURIFICADOR",
                Ativo = true
            },
            // Saúde — Vacina do pet (Recorrente)
            new
            {
                Id = IdVacinaPet,
                CategoriaId = (short)4,
                Nome = "Vacina do pet",
                TipoVencimento = TipoVencimento.RecorrentePorTempo,
                IntervaloDias = (int?)365,
                IntervaloKm = (int?)null,
                GarantiaFabricanteMeses = (short?)null,
                AntecedenciaDias = (short)30,
                CodigoCheckup = "TEM_PET",
                Ativo = true
            },
            // Garantias — Garantia de eletrônico (Calculado)
            new
            {
                Id = IdGarantiaEletro,
                CategoriaId = (short)6,
                Nome = "Garantia de eletrônico",
                TipoVencimento = TipoVencimento.Calculado,
                IntervaloDias = (int?)null,
                IntervaloKm = (int?)null,
                GarantiaFabricanteMeses = (short?)12,
                AntecedenciaDias = (short)15,
                CodigoCheckup = "COMPROU_ELETRONICO",
                Ativo = true
            },
            Modelo(IdLicenciamento, 1, "Licenciamento", TipoVencimento.RecorrentePorTempo, 365, 30, "TEM_CARRO"),
            Modelo(IdPassaporte, 2, "Passaporte", TipoVencimento.DataUnica, null, 90, "TEM_PASSAPORTE"),
            Modelo(IdConsultaRotina, 4, "Consulta de rotina", TipoVencimento.RecorrentePorTempo, 365, 30, "CONSULTA_ROTINA"),
            Modelo(IdDeclaracaoIr, 5, "Declaração do Imposto de Renda", TipoVencimento.RecorrentePorTempo, 365, 30, "DECLARA_IR"),
            Modelo(IdSeguroVeiculo, 1, "Seguro do veículo", TipoVencimento.RecorrentePorTempo, 365, 30, "TEM_SEGURO_AUTO"),
            Modelo(IdCertificado, 2, "Certificado digital", TipoVencimento.DataUnica, null, 30, "TEM_CERTIFICADO"),
            Modelo(IdAluguel, 3, "Contrato de aluguel", TipoVencimento.DataUnica, null, 60, "MORA_ALUGUEL"),
            Modelo(IdArCondicionado, 3, "Limpeza do ar-condicionado", TipoVencimento.RecorrentePorTempo, 180, 15, "TEM_AR_CONDICIONADO"),
            Modelo(IdDentista, 4, "Dentista", TipoVencimento.RecorrentePorTempo, 180, 15, "VAI_DENTISTA"),
            Modelo(IdExameVista, 4, "Exame de vista", TipoVencimento.RecorrentePorTempo, 365, 30, "USA_OCULOS"),
            Modelo(IdIptu, 5, "IPTU", TipoVencimento.RecorrentePorTempo, 365, 30, "PAGA_IPTU"),
            Modelo(IdSeguroVida, 5, "Seguro de vida", TipoVencimento.RecorrentePorTempo, 365, 30, "TEM_SEGURO_VIDA"),
            new
            {
                Id = IdGarantiaEletrod,
                CategoriaId = (short)6,
                Nome = "Garantia de eletrodoméstico",
                TipoVencimento = TipoVencimento.Calculado,
                IntervaloDias = (int?)null,
                IntervaloKm = (int?)null,
                GarantiaFabricanteMeses = (short?)12,
                AntecedenciaDias = (short)15,
                CodigoCheckup = "COMPROU_ELETRODOMESTICO",
                Ativo = true
            }
        );
    }

    // ── Passos dos modelos (spec §2.7) ────────────────────────────────────────
    private static void SeedPassosModelo(ModelBuilder b)
    {
        b.Entity<PassoModelo>().HasData(
            // Troca de óleo
            new { Id = new Guid("b1000000-0000-0000-0000-000000000001"), ModeloItemId = IdTrocaOleo,      Ordem = (short)1, Descricao = "Agendar oficina" },
            new { Id = new Guid("b1000000-0000-0000-0000-000000000002"), ModeloItemId = IdTrocaOleo,      Ordem = (short)2, Descricao = "Levar o veículo" },
            // IPVA
            new { Id = new Guid("b1000000-0000-0000-0000-000000000003"), ModeloItemId = IdIpva,           Ordem = (short)1, Descricao = "Consultar valor" },
            new { Id = new Guid("b1000000-0000-0000-0000-000000000004"), ModeloItemId = IdIpva,           Ordem = (short)2, Descricao = "Pagar" },
            // CNH
            new { Id = new Guid("b1000000-0000-0000-0000-000000000005"), ModeloItemId = IdCnh,            Ordem = (short)1, Descricao = "Agendar exame médico" },
            new { Id = new Guid("b1000000-0000-0000-0000-000000000006"), ModeloItemId = IdCnh,            Ordem = (short)2, Descricao = "Pagar taxa" },
            new { Id = new Guid("b1000000-0000-0000-0000-000000000007"), ModeloItemId = IdCnh,            Ordem = (short)3, Descricao = "Ir ao Detran" },
            // Filtro do purificador
            new { Id = new Guid("b1000000-0000-0000-0000-000000000008"), ModeloItemId = IdFiltroPurif,    Ordem = (short)1, Descricao = "Comprar refil" },
            new { Id = new Guid("b1000000-0000-0000-0000-000000000009"), ModeloItemId = IdFiltroPurif,    Ordem = (short)2, Descricao = "Trocar" },
            // Vacina do pet
            new { Id = new Guid("b1000000-0000-0000-0000-000000000010"), ModeloItemId = IdVacinaPet,      Ordem = (short)1, Descricao = "Agendar veterinário" },
            // Garantia de eletrônico
            new { Id = new Guid("b1000000-0000-0000-0000-000000000011"), ModeloItemId = IdGarantiaEletro, Ordem = (short)1, Descricao = "Testar o produto" },
            new { Id = new Guid("b1000000-0000-0000-0000-000000000012"), ModeloItemId = IdGarantiaEletro, Ordem = (short)2, Descricao = "Separar a nota fiscal" },
            // Modelos complementares
            new { Id = new Guid("b1000000-0000-0000-0000-000000000013"), ModeloItemId = IdLicenciamento,  Ordem = (short)1, Descricao = "Pagar a taxa" },
            new { Id = new Guid("b1000000-0000-0000-0000-000000000014"), ModeloItemId = IdLicenciamento,  Ordem = (short)2, Descricao = "Baixar o documento digital" },
            new { Id = new Guid("b1000000-0000-0000-0000-000000000015"), ModeloItemId = IdPassaporte,     Ordem = (short)1, Descricao = "Agendar na Polícia Federal" },
            new { Id = new Guid("b1000000-0000-0000-0000-000000000016"), ModeloItemId = IdPassaporte,     Ordem = (short)2, Descricao = "Pagar a taxa" },
            new { Id = new Guid("b1000000-0000-0000-0000-000000000017"), ModeloItemId = IdConsultaRotina, Ordem = (short)1, Descricao = "Agendar a consulta" },
            new { Id = new Guid("b1000000-0000-0000-0000-000000000018"), ModeloItemId = IdDeclaracaoIr,   Ordem = (short)1, Descricao = "Juntar os comprovantes" },
            new { Id = new Guid("b1000000-0000-0000-0000-000000000019"), ModeloItemId = IdDeclaracaoIr,   Ordem = (short)2, Descricao = "Enviar a declaração" },
            // Modelos do check-up por área
            new { Id = new Guid("b1000000-0000-0000-0000-000000000020"), ModeloItemId = IdSeguroVeiculo, Ordem = (short)1, Descricao = "Pedir cotações" },
            new { Id = new Guid("b1000000-0000-0000-0000-000000000021"), ModeloItemId = IdSeguroVeiculo, Ordem = (short)2, Descricao = "Renovar a apólice" },
            new { Id = new Guid("b1000000-0000-0000-0000-000000000022"), ModeloItemId = IdCertificado, Ordem = (short)1, Descricao = "Agendar a validação" },
            new { Id = new Guid("b1000000-0000-0000-0000-000000000023"), ModeloItemId = IdCertificado, Ordem = (short)2, Descricao = "Emitir o novo certificado" },
            new { Id = new Guid("b1000000-0000-0000-0000-000000000024"), ModeloItemId = IdAluguel, Ordem = (short)1, Descricao = "Conversar com o proprietário" },
            new { Id = new Guid("b1000000-0000-0000-0000-000000000025"), ModeloItemId = IdAluguel, Ordem = (short)2, Descricao = "Assinar a renovação" },
            new { Id = new Guid("b1000000-0000-0000-0000-000000000026"), ModeloItemId = IdArCondicionado, Ordem = (short)1, Descricao = "Agendar o técnico" },
            new { Id = new Guid("b1000000-0000-0000-0000-000000000027"), ModeloItemId = IdDentista, Ordem = (short)1, Descricao = "Agendar a consulta" },
            new { Id = new Guid("b1000000-0000-0000-0000-000000000028"), ModeloItemId = IdExameVista, Ordem = (short)1, Descricao = "Agendar o oftalmologista" },
            new { Id = new Guid("b1000000-0000-0000-0000-000000000029"), ModeloItemId = IdIptu, Ordem = (short)1, Descricao = "Emitir o boleto" },
            new { Id = new Guid("b1000000-0000-0000-0000-000000000030"), ModeloItemId = IdIptu, Ordem = (short)2, Descricao = "Pagar ou parcelar" },
            new { Id = new Guid("b1000000-0000-0000-0000-000000000031"), ModeloItemId = IdSeguroVida, Ordem = (short)1, Descricao = "Conferir a cobertura" },
            new { Id = new Guid("b1000000-0000-0000-0000-000000000032"), ModeloItemId = IdSeguroVida, Ordem = (short)2, Descricao = "Renovar" },
            new { Id = new Guid("b1000000-0000-0000-0000-000000000033"), ModeloItemId = IdGarantiaEletrod, Ordem = (short)1, Descricao = "Testar o aparelho" },
            new { Id = new Guid("b1000000-0000-0000-0000-000000000034"), ModeloItemId = IdGarantiaEletrod, Ordem = (short)2, Descricao = "Separar a nota fiscal" }
        );
    }
}
