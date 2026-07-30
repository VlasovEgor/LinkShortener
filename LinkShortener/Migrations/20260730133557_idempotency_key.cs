using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkShortener.Migrations
{
    /// <inheritdoc />
    public partial class idempotency_key : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                table: "ShortLinks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShortLinks_IdempotencyKey",
                table: "ShortLinks",
                column: "IdempotencyKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShortLinks_IdempotencyKey",
                table: "ShortLinks");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "ShortLinks");
        }
    }
}
