using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EmDia.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AjustesEntrega2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "modelos_item",
                columns: new[] { "id", "antecedencia_dias", "ativo", "categoria_id", "codigo_checkup", "garantia_fabricante_meses", "intervalo_dias", "intervalo_km", "nome", "tipo_vencimento" },
                values: new object[,]
                {
                    { new Guid("a7000000-0000-0000-0000-000000000007"), (short)30, true, (short)1, "TEM_CARRO", null, 365, null, "Licenciamento", (short)2 },
                    { new Guid("a7000000-0000-0000-0000-000000000008"), (short)90, true, (short)2, "TEM_PASSAPORTE", null, null, null, "Passaporte", (short)1 },
                    { new Guid("a7000000-0000-0000-0000-000000000009"), (short)30, true, (short)4, "CONSULTA_ROTINA", null, 365, null, "Consulta de rotina", (short)2 },
                    { new Guid("a7000000-0000-0000-0000-000000000010"), (short)30, true, (short)5, "DECLARA_IR", null, 365, null, "Declaração do Imposto de Renda", (short)2 }
                });

            migrationBuilder.InsertData(
                table: "passos_modelo",
                columns: new[] { "id", "descricao", "modelo_item_id", "ordem" },
                values: new object[,]
                {
                    { new Guid("b1000000-0000-0000-0000-000000000013"), "Pagar a taxa", new Guid("a7000000-0000-0000-0000-000000000007"), (short)1 },
                    { new Guid("b1000000-0000-0000-0000-000000000014"), "Baixar o documento digital", new Guid("a7000000-0000-0000-0000-000000000007"), (short)2 },
                    { new Guid("b1000000-0000-0000-0000-000000000015"), "Agendar na Polícia Federal", new Guid("a7000000-0000-0000-0000-000000000008"), (short)1 },
                    { new Guid("b1000000-0000-0000-0000-000000000016"), "Pagar a taxa", new Guid("a7000000-0000-0000-0000-000000000008"), (short)2 },
                    { new Guid("b1000000-0000-0000-0000-000000000017"), "Agendar a consulta", new Guid("a7000000-0000-0000-0000-000000000009"), (short)1 },
                    { new Guid("b1000000-0000-0000-0000-000000000018"), "Juntar os comprovantes", new Guid("a7000000-0000-0000-0000-000000000010"), (short)1 },
                    { new Guid("b1000000-0000-0000-0000-000000000019"), "Enviar a declaração", new Guid("a7000000-0000-0000-0000-000000000010"), (short)2 }
                });

            migrationBuilder.CreateIndex(
                name: "ix_modelos_categoria_nome",
                table: "modelos_item",
                columns: new[] { "categoria_id", "nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_itens_categoria_id",
                table: "itens",
                column: "categoria_id");

            migrationBuilder.AddForeignKey(
                name: "FK_avisos_itens_item_id",
                table: "avisos",
                column: "item_id",
                principalTable: "itens",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_checkups_usuarios_usuario_id",
                table: "checkups",
                column: "usuario_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_itens_categorias_categoria_id",
                table: "itens",
                column: "categoria_id",
                principalTable: "categorias",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_itens_usuarios_usuario_id",
                table: "itens",
                column: "usuario_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_modelos_item_categorias_categoria_id",
                table: "modelos_item",
                column: "categoria_id",
                principalTable: "categorias",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_refresh_tokens_usuarios_usuario_id",
                table: "refresh_tokens",
                column: "usuario_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_veiculos_usuarios_usuario_id",
                table: "veiculos",
                column: "usuario_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_avisos_itens_item_id",
                table: "avisos");

            migrationBuilder.DropForeignKey(
                name: "FK_checkups_usuarios_usuario_id",
                table: "checkups");

            migrationBuilder.DropForeignKey(
                name: "FK_itens_categorias_categoria_id",
                table: "itens");

            migrationBuilder.DropForeignKey(
                name: "FK_itens_usuarios_usuario_id",
                table: "itens");

            migrationBuilder.DropForeignKey(
                name: "FK_modelos_item_categorias_categoria_id",
                table: "modelos_item");

            migrationBuilder.DropForeignKey(
                name: "FK_refresh_tokens_usuarios_usuario_id",
                table: "refresh_tokens");

            migrationBuilder.DropForeignKey(
                name: "FK_veiculos_usuarios_usuario_id",
                table: "veiculos");

            migrationBuilder.DropIndex(
                name: "ix_modelos_categoria_nome",
                table: "modelos_item");

            migrationBuilder.DropIndex(
                name: "IX_itens_categoria_id",
                table: "itens");

            migrationBuilder.DeleteData(
                table: "passos_modelo",
                keyColumn: "id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000013"));

            migrationBuilder.DeleteData(
                table: "passos_modelo",
                keyColumn: "id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000014"));

            migrationBuilder.DeleteData(
                table: "passos_modelo",
                keyColumn: "id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000015"));

            migrationBuilder.DeleteData(
                table: "passos_modelo",
                keyColumn: "id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000016"));

            migrationBuilder.DeleteData(
                table: "passos_modelo",
                keyColumn: "id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000017"));

            migrationBuilder.DeleteData(
                table: "passos_modelo",
                keyColumn: "id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000018"));

            migrationBuilder.DeleteData(
                table: "passos_modelo",
                keyColumn: "id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000019"));

            migrationBuilder.DeleteData(
                table: "modelos_item",
                keyColumn: "id",
                keyValue: new Guid("a7000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "modelos_item",
                keyColumn: "id",
                keyValue: new Guid("a7000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "modelos_item",
                keyColumn: "id",
                keyValue: new Guid("a7000000-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "modelos_item",
                keyColumn: "id",
                keyValue: new Guid("a7000000-0000-0000-0000-000000000010"));
        }
    }
}
