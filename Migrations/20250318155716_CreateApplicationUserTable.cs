using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConferenceDelegateManagement1234122.Migrations
{
    /// <inheritdoc />
    public partial class CreateApplicationUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CCCD",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "CCCD",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(12)",
                oldMaxLength: 12)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "CCCD",
                keyValue: null,
                column: "CCCD",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "CCCD",
                table: "AspNetUsers",
                type: "varchar(12)",
                maxLength: 12,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CCCD",
                table: "AspNetUsers",
                column: "CCCD",
                unique: true,
                filter: "CCCD IS NOT NULL");
        }
    }
}
