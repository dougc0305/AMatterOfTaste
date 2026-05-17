using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AMatterOfTaste.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddVisitCounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "viewcount",
                table: "recipe",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "sitevisit",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    visitdate = table.Column<DateOnly>(type: "date", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false),
                    createdbyid = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    createddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modifiedbyid = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    modifieddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sitevisit", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_sitevisit_visitdate",
                table: "sitevisit",
                column: "visitdate",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sitevisit");

            migrationBuilder.DropColumn(
                name: "viewcount",
                table: "recipe");
        }
    }
}
