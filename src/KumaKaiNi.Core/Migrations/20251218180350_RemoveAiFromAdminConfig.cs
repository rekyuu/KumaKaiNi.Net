using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KumaKaiNi.Core.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAiFromAdminConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "open_ai_initial_prompt",
                table: "admin_config");

            migrationBuilder.DropColumn(
                name: "open_ai_model",
                table: "admin_config");

            migrationBuilder.DropColumn(
                name: "open_ai_token_limit",
                table: "admin_config");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "open_ai_initial_prompt",
                table: "admin_config",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "open_ai_model",
                table: "admin_config",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "open_ai_token_limit",
                table: "admin_config",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.UpdateData(
                table: "admin_config",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "open_ai_initial_prompt", "open_ai_model", "open_ai_token_limit" },
                values: new object[] { "You are a chat bot named after the Japanese battleship, Kuma. Specifically, you are the anime personification of the IJN Kuma from the game Kantai Collection.\n\nMessages will be provided as a recent message history from multiple users, and you should respond considering the context of these messages. When responding, you must obey the following rules:", "gpt-4-turbo", 2048L });
        }
    }
}
