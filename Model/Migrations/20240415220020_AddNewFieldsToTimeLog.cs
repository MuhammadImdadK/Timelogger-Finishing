using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Model.Migrations
{
    /// <inheritdoc />
    public partial class AddNewFieldsToTimeLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsNewTimeLog",
                table: "TimeLogs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVisibleToUser",
                table: "TimeLogs",
                type: "boolean",
                nullable: false,
                defaultValue: true);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsNewTimeLog",
                table: "TimeLogs");

            migrationBuilder.DropColumn(
                name: "IsVisibleToUser",
                table: "TimeLogs");
        }
    }
}
