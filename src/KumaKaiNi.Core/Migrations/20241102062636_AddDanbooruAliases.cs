using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KumaKaiNi.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddDanbooruAliases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "danbooru_aliases",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    alias = table.Column<string>(type: "text", nullable: false),
                    tag = table.Column<string>(type: "text", nullable: false),
                    inserted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_modified = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_danbooru_aliases", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_danbooru_aliases_alias",
                table: "danbooru_aliases",
                column: "alias",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "danbooru_aliases");
        }
    }
}
