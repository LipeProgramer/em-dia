using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EmDia.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CriacaoInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "avisos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<short>(type: "smallint", nullable: false),
                    canal = table.Column<short>(type: "smallint", nullable: false),
                    vencimento_referencia = table.Column<DateOnly>(type: "date", nullable: false),
                    numero = table.Column<short>(type: "smallint", nullable: false),
                    enviado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    sucesso = table.Column<bool>(type: "boolean", nullable: false),
                    erro = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_avisos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "categorias",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false),
                    nome = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    icone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ordem = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categorias", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "checkups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    realizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    respostas = table.Column<string>(type: "jsonb", nullable: false),
                    itens_criados = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_checkups", x => x.id);
                    table.CheckConstraint("ck_checkup_itens", "itens_criados >= 0");
                });

            migrationBuilder.CreateTable(
                name: "modelos_item",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    categoria_id = table.Column<short>(type: "smallint", nullable: false),
                    nome = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    tipo_vencimento = table.Column<short>(type: "smallint", nullable: false),
                    intervalo_dias = table.Column<int>(type: "integer", nullable: true),
                    intervalo_km = table.Column<int>(type: "integer", nullable: true),
                    garantia_fabricante_meses = table.Column<short>(type: "smallint", nullable: true),
                    antecedencia_dias = table.Column<short>(type: "smallint", nullable: false),
                    codigo_checkup = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_modelos_item", x => x.id);
                    table.CheckConstraint("ck_modelo_antecedencia", "antecedencia_dias BETWEEN 0 AND 365");
                    table.CheckConstraint("ck_modelo_garantia", "garantia_fabricante_meses IS NULL OR garantia_fabricante_meses > 0");
                    table.CheckConstraint("ck_modelo_intervalo_dias", "intervalo_dias IS NULL OR intervalo_dias > 0");
                    table.CheckConstraint("ck_modelo_intervalo_km", "intervalo_km IS NULL OR intervalo_km > 0");
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expira_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revogado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    senha_hash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ultimo_checkup_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    falhas_login = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    bloqueado_ate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "veiculos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    apelido = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    tipo = table.Column<short>(type: "smallint", nullable: false),
                    km_atual = table.Column<int>(type: "integer", nullable: false),
                    media_km_mes = table.Column<int>(type: "integer", nullable: false),
                    km_atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_veiculos", x => x.id);
                    table.CheckConstraint("ck_veiculo_km", "km_atual >= 0");
                    table.CheckConstraint("ck_veiculo_media_km", "media_km_mes BETWEEN 1 AND 20000");
                });

            migrationBuilder.CreateTable(
                name: "passos_modelo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    modelo_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem = table.Column<short>(type: "smallint", nullable: false),
                    descricao = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_passos_modelo", x => x.id);
                    table.ForeignKey(
                        name: "FK_passos_modelo_modelos_item_modelo_item_id",
                        column: x => x.modelo_item_id,
                        principalTable: "modelos_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "preferencias_aviso",
                columns: table => new
                {
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    receber_email = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    max_reavisos = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)2),
                    intervalo_reaviso_dias = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)3),
                    modo_foco_padrao = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_preferencias_aviso", x => x.usuario_id);
                    table.CheckConstraint("ck_pref_intervalo", "intervalo_reaviso_dias BETWEEN 1 AND 7");
                    table.CheckConstraint("ck_pref_max_reavisos", "max_reavisos BETWEEN 0 AND 5");
                    table.ForeignKey(
                        name: "FK_preferencias_aviso_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "itens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    categoria_id = table.Column<short>(type: "smallint", nullable: false),
                    modelo_item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    veiculo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    nome = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    tipo_vencimento = table.Column<short>(type: "smallint", nullable: false),
                    data_base = table.Column<DateOnly>(type: "date", nullable: true),
                    data_vencimento = table.Column<DateOnly>(type: "date", nullable: true),
                    intervalo_dias = table.Column<int>(type: "integer", nullable: true),
                    intervalo_km = table.Column<int>(type: "integer", nullable: true),
                    km_base = table.Column<int>(type: "integer", nullable: true),
                    garantia_fabricante_meses = table.Column<short>(type: "smallint", nullable: true),
                    antecedencia_dias = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)30),
                    proximo_vencimento_data = table.Column<DateOnly>(type: "date", nullable: true),
                    proximo_vencimento_km = table.Column<int>(type: "integer", nullable: true),
                    situacao = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1),
                    observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_itens", x => x.id);
                    table.CheckConstraint("ck_item_antecedencia", "antecedencia_dias BETWEEN 0 AND 365");
                    table.CheckConstraint("ck_item_calculado", "tipo_vencimento <> 4 OR (data_base IS NOT NULL AND garantia_fabricante_meses IS NOT NULL)");
                    table.CheckConstraint("ck_item_data_unica", "tipo_vencimento <> 1 OR data_vencimento IS NOT NULL");
                    table.CheckConstraint("ck_item_garantia", "garantia_fabricante_meses IS NULL OR garantia_fabricante_meses > 0");
                    table.CheckConstraint("ck_item_intervalo_dias", "intervalo_dias IS NULL OR intervalo_dias > 0");
                    table.CheckConstraint("ck_item_intervalo_km", "intervalo_km IS NULL OR intervalo_km > 0");
                    table.CheckConstraint("ck_item_km_base", "km_base IS NULL OR km_base >= 0");
                    table.CheckConstraint("ck_item_recorrente", "tipo_vencimento <> 2 OR (data_base IS NOT NULL AND intervalo_dias IS NOT NULL)");
                    table.CheckConstraint("ck_item_uso_tempo", "tipo_vencimento <> 3 OR (veiculo_id IS NOT NULL AND data_base IS NOT NULL AND km_base IS NOT NULL AND intervalo_km IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_itens_modelos_item_modelo_item_id",
                        column: x => x.modelo_item_id,
                        principalTable: "modelos_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_itens_veiculos_veiculo_id",
                        column: x => x.veiculo_id,
                        principalTable: "veiculos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "historicos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    concluido_em = table.Column<DateOnly>(type: "date", nullable: false),
                    km_na_conclusao = table.Column<int>(type: "integer", nullable: true),
                    valor_gasto = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    vencimento_anterior = table.Column<DateOnly>(type: "date", nullable: true),
                    proximo_vencimento = table.Column<DateOnly>(type: "date", nullable: true),
                    observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_historicos", x => x.id);
                    table.CheckConstraint("ck_historico_km", "km_na_conclusao IS NULL OR km_na_conclusao >= 0");
                    table.CheckConstraint("ck_historico_valor", "valor_gasto IS NULL OR valor_gasto >= 0");
                    table.ForeignKey(
                        name: "FK_historicos_itens_item_id",
                        column: x => x.item_id,
                        principalTable: "itens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "passos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem = table.Column<short>(type: "smallint", nullable: false),
                    descricao = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    concluido = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    concluido_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_passos", x => x.id);
                    table.ForeignKey(
                        name: "FK_passos_itens_item_id",
                        column: x => x.item_id,
                        principalTable: "itens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "categorias",
                columns: new[] { "id", "icone", "nome", "ordem" },
                values: new object[,]
                {
                    { (short)1, "car", "Veículo", (short)1 },
                    { (short)2, "id-card", "Documentos", (short)2 },
                    { (short)3, "home", "Casa", (short)3 },
                    { (short)4, "heart", "Saúde", (short)4 },
                    { (short)5, "dollar", "Finanças", (short)5 },
                    { (short)6, "shield-check", "Garantias", (short)6 }
                });

            migrationBuilder.InsertData(
                table: "modelos_item",
                columns: new[] { "id", "antecedencia_dias", "ativo", "categoria_id", "codigo_checkup", "garantia_fabricante_meses", "intervalo_dias", "intervalo_km", "nome", "tipo_vencimento" },
                values: new object[,]
                {
                    { new Guid("a7000000-0000-0000-0000-000000000001"), (short)30, true, (short)1, "TEM_CARRO", null, 365, 10000, "Troca de óleo", (short)3 },
                    { new Guid("a7000000-0000-0000-0000-000000000002"), (short)30, true, (short)1, "TEM_CARRO", null, 365, null, "IPVA", (short)2 },
                    { new Guid("a7000000-0000-0000-0000-000000000003"), (short)45, true, (short)2, "TEM_CNH", null, null, null, "CNH", (short)1 },
                    { new Guid("a7000000-0000-0000-0000-000000000004"), (short)15, true, (short)3, "TEM_PURIFICADOR", null, 180, null, "Filtro do purificador", (short)2 },
                    { new Guid("a7000000-0000-0000-0000-000000000005"), (short)30, true, (short)4, "TEM_PET", null, 365, null, "Vacina do pet", (short)2 },
                    { new Guid("a7000000-0000-0000-0000-000000000006"), (short)15, true, (short)6, "COMPROU_ELETRONICO", (short)12, null, null, "Garantia de eletrônico", (short)4 }
                });

            migrationBuilder.InsertData(
                table: "passos_modelo",
                columns: new[] { "id", "descricao", "modelo_item_id", "ordem" },
                values: new object[,]
                {
                    { new Guid("b1000000-0000-0000-0000-000000000001"), "Agendar oficina", new Guid("a7000000-0000-0000-0000-000000000001"), (short)1 },
                    { new Guid("b1000000-0000-0000-0000-000000000002"), "Levar o veículo", new Guid("a7000000-0000-0000-0000-000000000001"), (short)2 },
                    { new Guid("b1000000-0000-0000-0000-000000000003"), "Consultar valor", new Guid("a7000000-0000-0000-0000-000000000002"), (short)1 },
                    { new Guid("b1000000-0000-0000-0000-000000000004"), "Pagar", new Guid("a7000000-0000-0000-0000-000000000002"), (short)2 },
                    { new Guid("b1000000-0000-0000-0000-000000000005"), "Agendar exame médico", new Guid("a7000000-0000-0000-0000-000000000003"), (short)1 },
                    { new Guid("b1000000-0000-0000-0000-000000000006"), "Pagar taxa", new Guid("a7000000-0000-0000-0000-000000000003"), (short)2 },
                    { new Guid("b1000000-0000-0000-0000-000000000007"), "Ir ao Detran", new Guid("a7000000-0000-0000-0000-000000000003"), (short)3 },
                    { new Guid("b1000000-0000-0000-0000-000000000008"), "Comprar refil", new Guid("a7000000-0000-0000-0000-000000000004"), (short)1 },
                    { new Guid("b1000000-0000-0000-0000-000000000009"), "Trocar", new Guid("a7000000-0000-0000-0000-000000000004"), (short)2 },
                    { new Guid("b1000000-0000-0000-0000-000000000010"), "Agendar veterinário", new Guid("a7000000-0000-0000-0000-000000000005"), (short)1 },
                    { new Guid("b1000000-0000-0000-0000-000000000011"), "Testar o produto", new Guid("a7000000-0000-0000-0000-000000000006"), (short)1 },
                    { new Guid("b1000000-0000-0000-0000-000000000012"), "Separar a nota fiscal", new Guid("a7000000-0000-0000-0000-000000000006"), (short)2 }
                });

            migrationBuilder.CreateIndex(
                name: "ix_avisos_item_vencimento",
                table: "avisos",
                columns: new[] { "item_id", "vencimento_referencia" });

            migrationBuilder.CreateIndex(
                name: "ix_avisos_uk",
                table: "avisos",
                columns: new[] { "item_id", "vencimento_referencia", "numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_categorias_nome",
                table: "categorias",
                column: "nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_checkups_usuario",
                table: "checkups",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_historicos_item",
                table: "historicos",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_itens_modelo_item_id",
                table: "itens",
                column: "modelo_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_itens_radar",
                table: "itens",
                columns: new[] { "usuario_id", "situacao", "proximo_vencimento_data" });

            migrationBuilder.CreateIndex(
                name: "ix_itens_usuario",
                table: "itens",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_itens_veiculo",
                table: "itens",
                column: "veiculo_id");

            migrationBuilder.CreateIndex(
                name: "ix_modelos_categoria",
                table: "modelos_item",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "ix_passos_item_ordem",
                table: "passos",
                columns: new[] { "item_id", "ordem" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_passos_modelo_ordem",
                table: "passos_modelo",
                columns: new[] { "modelo_item_id", "ordem" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_hash",
                table: "refresh_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_usuario",
                table: "refresh_tokens",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_email",
                table: "usuarios",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_veiculos_usuario",
                table: "veiculos",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_veiculos_usuario_apelido",
                table: "veiculos",
                columns: new[] { "usuario_id", "apelido" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "avisos");

            migrationBuilder.DropTable(
                name: "categorias");

            migrationBuilder.DropTable(
                name: "checkups");

            migrationBuilder.DropTable(
                name: "historicos");

            migrationBuilder.DropTable(
                name: "passos");

            migrationBuilder.DropTable(
                name: "passos_modelo");

            migrationBuilder.DropTable(
                name: "preferencias_aviso");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "itens");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "modelos_item");

            migrationBuilder.DropTable(
                name: "veiculos");
        }
    }
}
