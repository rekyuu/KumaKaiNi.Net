using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KumaKaiNi.Core.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDanbooruCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "danbooru_caches");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "danbooru_caches",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    channel_id = table.Column<long>(type: "bigint", nullable: false),
                    expires = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    file_url = table.Column<string>(type: "text", nullable: false),
                    inserted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_modified = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    source_system = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_danbooru_caches", x => x.id);
                });
        }
    }
}
