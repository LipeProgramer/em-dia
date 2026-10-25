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
            new { Id = new Guid("b1000000-0000-0000-0000-000000000012"), ModeloItemId = IdGarantiaEletro, Ordem = (short)2, Descricao = "Separar a nota fiscal" }
        );
    }
}
