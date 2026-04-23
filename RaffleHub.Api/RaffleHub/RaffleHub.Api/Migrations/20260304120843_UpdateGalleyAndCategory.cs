using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaffleHub.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGalleyAndCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CategoriesGalleries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriesGalleries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Gallery",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    FolderName = table.Column<string>(type: "text", nullable: false),
                    NameImage = table.Column<string>(type: "text", nullable: false),
                    DescriptionImage = table.Column<string>(type: "text", nullable: false),
                    Year = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gallery", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Gallery_CategoriesGalleries_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "CategoriesGalleries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Gallery_CategoryId",
                table: "Gallery",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Gallery");

            migrationBuilder.DropTable(
                name: "CategoriesGalleries");
        }
    }
}
