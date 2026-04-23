using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaffleHub.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddImageToRaffle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FolderName",
                table: "Raffle",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Raffle",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FolderName",
                table: "Raffle");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Raffle");
        }
    }
}
