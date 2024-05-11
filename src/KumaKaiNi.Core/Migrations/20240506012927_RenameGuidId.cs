using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KumaKaiNi.Core.Migrations
{
    /// <inheritdoc />
    public partial class RenameGuidId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "next_id",
                table: "telegram_allowlists",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "next_id",
                table: "quotes",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "next_id",
                table: "gpt_responses",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "next_id",
                table: "error_logs",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "next_id",
                table: "danbooru_blocklists",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "next_id",
                table: "custom_commands",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "next_id",
                table: "chat_logs",
                newName: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "id",
                table: "telegram_allowlists",
                newName: "next_id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "quotes",
                newName: "next_id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "gpt_responses",
                newName: "next_id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "error_logs",
                newName: "next_id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "danbooru_blocklists",
                newName: "next_id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "custom_commands",
                newName: "next_id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "chat_logs",
                newName: "next_id");
        }
    }
}
