using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConferenceDelegateManagement1234122.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCCCDUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CCCD",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CCCD",
                table: "AspNetUsers",
                column: "CCCD",
                unique: true,
                filter: "CCCD IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CCCD",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CCCD",
                table: "AspNetUsers",
                column: "CCCD",
                unique: true);
        }
    }
}
