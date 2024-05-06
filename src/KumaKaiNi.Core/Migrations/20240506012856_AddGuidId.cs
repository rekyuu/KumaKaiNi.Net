using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KumaKaiNi.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddGuidId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "next_id",
                table: "telegram_allowlists",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<Guid>(
                name: "next_id",
                table: "quotes",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<Guid>(
                name: "next_id",
                table: "gpt_responses",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<Guid>(
                name: "next_id",
                table: "error_logs",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<Guid>(
                name: "next_id",
                table: "danbooru_blocklists",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<Guid>(
                name: "next_id",
                table: "custom_commands",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<Guid>(
                name: "next_id",
                table: "chat_logs",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "next_id",
                table: "telegram_allowlists");

            migrationBuilder.DropColumn(
                name: "next_id",
                table: "quotes");

            migrationBuilder.DropColumn(
                name: "next_id",
                table: "gpt_responses");

            migrationBuilder.DropColumn(
                name: "next_id",
                table: "error_logs");

            migrationBuilder.DropColumn(
                name: "next_id",
                table: "danbooru_blocklists");

            migrationBuilder.DropColumn(
                name: "next_id",
                table: "custom_commands");

            migrationBuilder.DropColumn(
                name: "next_id",
                table: "chat_logs");
        }
    }
}
