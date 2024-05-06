using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KumaKaiNi.Core.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIntId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_telegram_allowlists",
                table: "telegram_allowlists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_quotes",
                table: "quotes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_gpt_responses",
                table: "gpt_responses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_error_logs",
                table: "error_logs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_danbooru_blocklists",
                table: "danbooru_blocklists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_custom_commands",
                table: "custom_commands");

            migrationBuilder.DropPrimaryKey(
                name: "PK_chat_logs",
                table: "chat_logs");

            migrationBuilder.DropColumn(
                name: "id",
                table: "telegram_allowlists");

            migrationBuilder.DropColumn(
                name: "id",
                table: "quotes");

            migrationBuilder.DropColumn(
                name: "id",
                table: "gpt_responses");

            migrationBuilder.DropColumn(
                name: "id",
                table: "error_logs");

            migrationBuilder.DropColumn(
                name: "id",
                table: "danbooru_blocklists");

            migrationBuilder.DropColumn(
                name: "id",
                table: "custom_commands");

            migrationBuilder.DropColumn(
                name: "id",
                table: "chat_logs");

            migrationBuilder.AddPrimaryKey(
                name: "PK_telegram_allowlists",
                table: "telegram_allowlists",
                column: "next_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_quotes",
                table: "quotes",
                column: "next_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_gpt_responses",
                table: "gpt_responses",
                column: "next_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_error_logs",
                table: "error_logs",
                column: "next_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_danbooru_blocklists",
                table: "danbooru_blocklists",
                column: "next_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_custom_commands",
                table: "custom_commands",
                column: "next_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_chat_logs",
                table: "chat_logs",
                column: "next_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_telegram_allowlists",
                table: "telegram_allowlists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_quotes",
                table: "quotes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_gpt_responses",
                table: "gpt_responses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_error_logs",
                table: "error_logs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_danbooru_blocklists",
                table: "danbooru_blocklists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_custom_commands",
                table: "custom_commands");

            migrationBuilder.DropPrimaryKey(
                name: "PK_chat_logs",
                table: "chat_logs");

            migrationBuilder.AddColumn<int>(
                name: "id",
                table: "telegram_allowlists",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "id",
                table: "quotes",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "id",
                table: "gpt_responses",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "id",
                table: "error_logs",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "id",
                table: "danbooru_blocklists",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "id",
                table: "custom_commands",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "id",
                table: "chat_logs",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_telegram_allowlists",
                table: "telegram_allowlists",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_quotes",
                table: "quotes",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_gpt_responses",
                table: "gpt_responses",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_error_logs",
                table: "error_logs",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_danbooru_blocklists",
                table: "danbooru_blocklists",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_custom_commands",
                table: "custom_commands",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_chat_logs",
                table: "chat_logs",
                column: "id");
        }
    }
}
