using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EmDia.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CheckupPorArea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "modelos_item",
                columns: new[] { "id", "antecedencia_dias", "ativo", "categoria_id", "codigo_checkup", "garantia_fabricante_meses", "intervalo_dias", "intervalo_km", "nome", "tipo_vencimento" },
                values: new object[,]
                {
                    { new Guid("a7000000-0000-0000-0000-000000000011"), (short)30, true, (short)1, "TEM_SEGURO_AUTO", null, 365, null, "Seguro do veículo", (short)2 },
                    { new Guid("a7000000-0000-0000-0000-000000000012"), (short)30, true, (short)2, "TEM_CERTIFICADO", null, null, null, "Certificado digital", (short)1 },
                    { new Guid("a7000000-0000-0000-0000-000000000013"), (short)60, true, (short)3, "MORA_ALUGUEL", null, null, null, "Contrato de aluguel", (short)1 },
                    { new Guid("a7000000-0000-0000-0000-000000000014"), (short)15, true, (short)3, "TEM_AR_CONDICIONADO", null, 180, null, "Limpeza do ar-condicionado", (short)2 },
                    { new Guid("a7000000-0000-0000-0000-000000000015"), (short)15, true, (short)4, "VAI_DENTISTA", null, 180, null, "Dentista", (short)2 },
                    { new Guid("a7000000-0000-0000-0000-000000000016"), (short)30, true, (short)4, "USA_OCULOS", null, 365, null, "Exame de vista", (short)2 },
                    { new Guid("a7000000-0000-0000-0000-000000000017"), (short)30, true, (short)5, "PAGA_IPTU", null, 365, null, "IPTU", (short)2 },
                    { new Guid("a7000000-0000-0000-0000-000000000018"), (short)30, true, (short)5, "TEM_SEGURO_VIDA", null, 365, null, "Seguro de vida", (short)2 },
                    { new Guid("a7000000-0000-0000-0000-000000000019"), (short)15, true, (short)6, "COMPROU_ELETRODOMESTICO", (short)12, null, null, "Garantia de eletrodoméstico", (short)4 }
                });

            migrationBuilder.InsertData(
                table: "passos_modelo",
                columns: new[] { "id", "descricao", "modelo_item_id", "ordem" },
                values: new object[,]
                {
                    { new Guid("b1000000-0000-0000-0000-000000000020"), "Pedir cotações", new Guid("a7000000-0000-0000-0000-000000000011"), (short)1 },
                    { new Guid("b1000000-0000-0000-0000-000000000021"), "Renovar a apólice", new Guid("a7000000-0000-0000-0000-000000000011"), (short)2 },
                    { new Guid("b1000000-0000-0000-0000-000000000022"), "Agendar a validação", new Guid("a7000000-0000-0000-0000-000000000012"), (short)1 },
                    { new Guid("b1000000-0000-0000-0000-000000000023"), "Emitir o novo certificado", new Guid("a7000000-0000-0000-0000-000000000012"), (short)2 },
                    { new Guid("b1000000-0000-0000-0000-000000000024"), "Conversar com o proprietário", new Guid("a7000000-0000-0000-0000-000000000013"), (short)1 },
                    { new Guid("b1000000-0000-0000-0000-000000000025"), "Assinar a renovação", new Guid("a7000000-0000-0000-0000-000000000013"), (short)2 },
                    { new Guid("b1000000-0000-0000-0000-000000000026"), "Agendar o técnico", new Guid("a7000000-0000-0000-0000-000000000014"), (short)1 },
                    { new Guid("b1000000-0000-0000-0000-000000000027"), "Agendar a consulta", new Guid("a7000000-0000-0000-0000-000000000015"), (short)1 },
                    { new Guid("b1000000-0000-0000-0000-000000000028"), "Agendar o oftalmologista", new Guid("a7000000-0000-0000-0000-000000000016"), (short)1 },
                    { new Guid("b1000000-0000-0000-0000-000000000029"), "Emitir o boleto", new Guid("a7000000-0000-0000-0000-000000000017"), (short)1 },
                    { new Guid("b1000000-0000-0000-0000-000000000030"), "Pagar ou parcelar", new Guid("a7000000-0000-0000-0000-000000000017"), (short)2 },
                    { new Guid("b1000000-0000-0000-0000-000000000031"), "Conferir a cobertura", new Guid("a7000000-0000-0000-0000-000000000018"), (short)1 },
                    { new Guid("b1000000-0000-0000-0000-000000000032"), "Renovar", new Guid("a7000000-0000-0000-0000-000000000018"), (short)2 },
                    { new Guid("b1000000-0000-0000-0000-000000000033"), "Testar o aparelho", new Guid("a7000000-0000-0000-0000-000000000019"), (short)1 },
                    { new Guid("b1000000-0000-0000-0000-000000000034"), "Separar a nota fiscal", new Guid("a7000000-0000-0000-0000-000000000019"), (short)2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "passos_modelo",
                keyColumn: "id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000020"));

            migrationBuilder.DeleteData(
                table: "passos_modelo",
                keyColumn: "id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000021"));

            migrationBuilder.DeleteData(
                table: "passos_modelo",
                keyColumn: "id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000022"));

            migrationBuilder.DeleteData(
                table: "passos_modelo",
                keyColumn: "id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000023"));

            migrationBuilder.DeleteData(
                table: "passos_modelo",
                keyColumn: "id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000024"));

            migrationBuilder.DeleteData(
                table: "passos_modelo",
                keyColumn: "id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000025"));

            migrationBuilder.DeleteData(
                table: "passos_modelo",
                keyColumn: "id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000026"));

            migrationBuilder.DeleteData(
                table: "passos_modelo",
                keyColumn: "id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000027"));

            migrationBuilder.DeleteData(
                table: "passos_modelo",
                keyColumn: "id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000028"));

            migrationBuilder.DeleteData(
                table: "passos_modelo",
                keyColumn: "id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000029"));

            migrationBuilder.DeleteData(
                table: "passos_modelo",
                keyColumn: "id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000030"));

            migrationBuilder.DeleteData(
                table: "passos_modelo",
                keyColumn: "id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000031"));

            migrationBuilder.DeleteData(
                table: "passos_modelo",
                keyColumn: "id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000032"));

            migrationBuilder.DeleteData(
                table: "passos_modelo",
                keyColumn: "id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000033"));

            migrationBuilder.DeleteData(
                table: "passos_modelo",
                keyColumn: "id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000034"));

            migrationBuilder.DeleteData(
                table: "modelos_item",
                keyColumn: "id",
                keyValue: new Guid("a7000000-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "modelos_item",
                keyColumn: "id",
                keyValue: new Guid("a7000000-0000-0000-0000-000000000012"));

            migrationBuilder.DeleteData(
                table: "modelos_item",
                keyColumn: "id",
                keyValue: new Guid("a7000000-0000-0000-0000-000000000013"));

            migrationBuilder.DeleteData(
                table: "modelos_item",
                keyColumn: "id",
                keyValue: new Guid("a7000000-0000-0000-0000-000000000014"));

            migrationBuilder.DeleteData(
                table: "modelos_item",
                keyColumn: "id",
                keyValue: new Guid("a7000000-0000-0000-0000-000000000015"));

            migrationBuilder.DeleteData(
                table: "modelos_item",
                keyColumn: "id",
                keyValue: new Guid("a7000000-0000-0000-0000-000000000016"));

            migrationBuilder.DeleteData(
                table: "modelos_item",
                keyColumn: "id",
                keyValue: new Guid("a7000000-0000-0000-0000-000000000017"));

            migrationBuilder.DeleteData(
                table: "modelos_item",
                keyColumn: "id",
                keyValue: new Guid("a7000000-0000-0000-0000-000000000018"));

            migrationBuilder.DeleteData(
                table: "modelos_item",
                keyColumn: "id",
                keyValue: new Guid("a7000000-0000-0000-0000-000000000019"));
        }
    }
}
