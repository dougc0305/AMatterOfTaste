using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AMatterOfTaste.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "appuser",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    passwordhash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    cookbookslug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    isadmin = table.Column<bool>(type: "boolean", nullable: false),
                    createdbyid = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    createddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modifiedbyid = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    modifieddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_appuser", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "recipecategory",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sortorder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    createdbyid = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    createddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modifiedbyid = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    modifieddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_recipecategory", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "recipe",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    story = table.Column<string>(type: "text", nullable: true),
                    attribution = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    categoryid = table.Column<int>(type: "integer", nullable: false),
                    servings = table.Column<int>(type: "integer", nullable: true),
                    preptimeminutes = table.Column<int>(type: "integer", nullable: true),
                    cooktimeminutes = table.Column<int>(type: "integer", nullable: true),
                    createdbyid = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    createddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modifiedbyid = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    modifieddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_recipe", x => x.id);
                    table.ForeignKey(
                        name: "fk_recipe_recipecategory_categoryid",
                        column: x => x.categoryid,
                        principalTable: "recipecategory",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recipeingredient",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    recipeid = table.Column<int>(type: "integer", nullable: false),
                    sortorder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    quantity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    createdbyid = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    createddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modifiedbyid = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    modifieddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_recipeingredient", x => x.id);
                    table.ForeignKey(
                        name: "fk_recipeingredient_recipe_recipeid",
                        column: x => x.recipeid,
                        principalTable: "recipe",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recipephoto",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    recipeid = table.Column<int>(type: "integer", nullable: false),
                    filename = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    isprimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    sortorder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    createdbyid = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    createddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modifiedbyid = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    modifieddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_recipephoto", x => x.id);
                    table.ForeignKey(
                        name: "fk_recipephoto_recipe_recipeid",
                        column: x => x.recipeid,
                        principalTable: "recipe",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recipestep",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    recipeid = table.Column<int>(type: "integer", nullable: false),
                    stepnumber = table.Column<int>(type: "integer", nullable: false),
                    instruction = table.Column<string>(type: "text", nullable: false),
                    createdbyid = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    createddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modifiedbyid = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    modifieddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_recipestep", x => x.id);
                    table.ForeignKey(
                        name: "fk_recipestep_recipe_recipeid",
                        column: x => x.recipeid,
                        principalTable: "recipe",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "userfavorite",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    userid = table.Column<int>(type: "integer", nullable: false),
                    recipeid = table.Column<int>(type: "integer", nullable: false),
                    createdbyid = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    createddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modifiedbyid = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    modifieddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_userfavorite", x => x.id);
                    table.ForeignKey(
                        name: "fk_userfavorite_appuser_userid",
                        column: x => x.userid,
                        principalTable: "appuser",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_userfavorite_recipe_recipeid",
                        column: x => x.recipeid,
                        principalTable: "recipe",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "appuser",
                columns: new[] { "id", "cookbookslug", "createdbyid", "createddate", "email", "isactive", "isadmin", "modifiedbyid", "modifieddate", "name", "passwordhash" },
                values: new object[] { 1, null, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@amatteroftaste.us", true, true, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cheryl Charbonneau", "$2a$11$AUuv/yUUlaVfZx4zMYpP1OMfZndwAx4vmVRNzAltu4JnSR3bI4SWq" });

            migrationBuilder.InsertData(
                table: "recipecategory",
                columns: new[] { "id", "createdbyid", "createddate", "isactive", "modifiedbyid", "modifieddate", "name" },
                values: new object[] { 1, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Main Course" });

            migrationBuilder.InsertData(
                table: "recipecategory",
                columns: new[] { "id", "createdbyid", "createddate", "isactive", "modifiedbyid", "modifieddate", "name", "sortorder" },
                values: new object[,]
                {
                    { 2, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Dessert", 1 },
                    { 3, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cookies", 2 },
                    { 4, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Breakfast", 3 },
                    { 5, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Side Dishes", 4 },
                    { 6, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Soups", 5 },
                    { 7, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bread", 6 },
                    { 8, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Entertaining", 7 },
                    { 9, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Gifts", 8 },
                    { 10, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Appetizer", 9 },
                    { 11, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Salads", 10 },
                    { 12, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Beverages", 11 },
                    { 13, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Casserole", 12 },
                    { 14, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Dinner", 13 },
                    { 15, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Snacks", 14 },
                    { 16, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Condiments", 15 },
                    { 17, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Drinks", 16 },
                    { 18, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Other", 17 }
                });

            migrationBuilder.CreateIndex(
                name: "ix_appuser_cookbookslug",
                table: "appuser",
                column: "cookbookslug",
                unique: true,
                filter: "cookbookslug IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_appuser_email",
                table: "appuser",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_recipe_categoryid",
                table: "recipe",
                column: "categoryid");

            migrationBuilder.CreateIndex(
                name: "ix_recipeingredient_recipeid",
                table: "recipeingredient",
                column: "recipeid");

            migrationBuilder.CreateIndex(
                name: "ix_recipephoto_recipeid",
                table: "recipephoto",
                column: "recipeid");

            migrationBuilder.CreateIndex(
                name: "ix_recipestep_recipeid",
                table: "recipestep",
                column: "recipeid");

            migrationBuilder.CreateIndex(
                name: "ix_userfavorite_recipeid",
                table: "userfavorite",
                column: "recipeid");

            migrationBuilder.CreateIndex(
                name: "ix_userfavorite_userid_recipeid",
                table: "userfavorite",
                columns: new[] { "userid", "recipeid" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recipeingredient");

            migrationBuilder.DropTable(
                name: "recipephoto");

            migrationBuilder.DropTable(
                name: "recipestep");

            migrationBuilder.DropTable(
                name: "userfavorite");

            migrationBuilder.DropTable(
                name: "appuser");

            migrationBuilder.DropTable(
                name: "recipe");

            migrationBuilder.DropTable(
                name: "recipecategory");
        }
    }
}
