using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KumaKaiNi.Core.Migrations
{
    /// <inheritdoc />
    public partial class EfCoreRefactorRenames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_logs",
                table: "logs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_errors",
                table: "errors");

            migrationBuilder.RenameTable(
                name: "logs",
                newName: "chat_logs");

            migrationBuilder.RenameTable(
                name: "errors",
                newName: "error_logs");

            migrationBuilder.RenameColumn(
                name: "protocol",
                table: "danbooru_caches",
                newName: "source_system");

            migrationBuilder.RenameColumn(
                name: "protocol",
                table: "chat_logs",
                newName: "source_system");

            migrationBuilder.AddPrimaryKey(
                name: "PK_chat_logs",
                table: "chat_logs",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_error_logs",
                table: "error_logs",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_error_logs",
                table: "error_logs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_chat_logs",
                table: "chat_logs");

            migrationBuilder.RenameTable(
                name: "error_logs",
                newName: "errors");

            migrationBuilder.RenameTable(
                name: "chat_logs",
                newName: "logs");

            migrationBuilder.RenameColumn(
                name: "source_system",
                table: "danbooru_caches",
                newName: "protocol");

            migrationBuilder.RenameColumn(
                name: "source_system",
                table: "logs",
                newName: "protocol");

            migrationBuilder.AddPrimaryKey(
                name: "PK_errors",
                table: "errors",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_logs",
                table: "logs",
                column: "id");
        }
    }
}
