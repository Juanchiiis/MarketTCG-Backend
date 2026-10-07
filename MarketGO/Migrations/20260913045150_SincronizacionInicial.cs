using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketGO.Migrations
{
    /// <inheritdoc />
    public partial class SincronizacionInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Obligamos a C# a solo agregar las columnas a la tabla que ya existe
            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "Direcciones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodigoPostal",
                table: "Direcciones",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Label",
                table: "Direcciones");

            migrationBuilder.DropColumn(
                name: "CodigoPostal",
                table: "Direcciones");
        }
    }
}
