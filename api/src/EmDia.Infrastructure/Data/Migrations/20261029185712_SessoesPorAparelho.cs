using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmDia.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SessoesPorAparelho : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "familia_id",
                table: "refresh_tokens",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // Tokens que já existiam: cada um vira a própria sessão (as sessões abertas continuam valendo)
            migrationBuilder.Sql("UPDATE refresh_tokens SET familia_id = id;");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "substituido_em",
                table: "refresh_tokens",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_familia",
                table: "refresh_tokens",
                column: "familia_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_refresh_tokens_familia",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "familia_id",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "substituido_em",
                table: "refresh_tokens");
        }
    }
}
