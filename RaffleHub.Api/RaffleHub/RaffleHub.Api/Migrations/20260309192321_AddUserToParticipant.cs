using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaffleHub.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserToParticipant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Participant",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Participant_UserId",
                table: "Participant",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Participant_AspNetUsers_UserId",
                table: "Participant",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Participant_AspNetUsers_UserId",
                table: "Participant");

            migrationBuilder.DropIndex(
                name: "IX_Participant_UserId",
                table: "Participant");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Participant");
        }
    }
}
