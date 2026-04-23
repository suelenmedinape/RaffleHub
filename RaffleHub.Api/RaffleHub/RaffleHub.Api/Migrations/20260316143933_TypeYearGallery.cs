using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaffleHub.Api.Migrations
{
    /// <inheritdoc />
    public partial class TypeYearGallery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"Gallery\" ALTER COLUMN \"Year\" TYPE integer USING EXTRACT(YEAR FROM \"Year\")::integer;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"Gallery\" ALTER COLUMN \"Year\" TYPE timestamp with time zone USING make_date(\"Year\", 1, 1);");
        }
    }
}
