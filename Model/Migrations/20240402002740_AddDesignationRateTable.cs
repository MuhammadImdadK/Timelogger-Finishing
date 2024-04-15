using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Model.Migrations
{
    /// <inheritdoc />
    public partial class AddDesignationRateTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseRate",
                table: "Designations");

            migrationBuilder.DropColumn(
                name: "OutsideHourRate",
                table: "Designations");

            migrationBuilder.DropColumn(
                name: "OvertimeRate",
                table: "Designations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "BaseRate",
                table: "Designations",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "OutsideHourRate",
                table: "Designations",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "OvertimeRate",
                table: "Designations",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }
    }
}
