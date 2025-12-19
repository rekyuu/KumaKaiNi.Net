using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KumaKaiNi.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddMarkov : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "markov",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    source_system = table.Column<int>(type: "integer", nullable: false),
                    channel_id = table.Column<string>(type: "text", nullable: true),
                    previous_words = table.Column<string>(type: "text", nullable: false),
                    next_word = table.Column<string>(type: "text", nullable: false),
                    count = table.Column<long>(type: "bigint", nullable: false),
                    can_start = table.Column<bool>(type: "boolean", nullable: false),
                    inserted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_modified = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_markov", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_markov_source_system_channel_id_can_start",
                table: "markov",
                columns: new[] { "source_system", "channel_id", "can_start" });

            migrationBuilder.CreateIndex(
                name: "IX_markov_source_system_channel_id_previous_words",
                table: "markov",
                columns: new[] { "source_system", "channel_id", "previous_words" });

            migrationBuilder.CreateIndex(
                name: "IX_markov_source_system_channel_id_previous_words_next_word",
                table: "markov",
                columns: new[] { "source_system", "channel_id", "previous_words", "next_word" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "markov");
        }
    }
}
