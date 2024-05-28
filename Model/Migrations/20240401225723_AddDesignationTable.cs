using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Model.Migrations
{
    /// <inheritdoc />
    public partial class AddDesignationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DesignationID",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Designations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    BaseRate = table.Column<float>(type: "real", nullable: false),
                    OvertimeRate = table.Column<float>(type: "real", nullable: false),
                    OutsideHourRate = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Designations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_DesignationID",
                table: "Users",
                column: "DesignationID");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Designations_DesignationID",
                table: "Users",
                column: "DesignationID",
                principalTable: "Designations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Designations_DesignationID",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Designations");

            migrationBuilder.DropIndex(
                name: "IX_Users_DesignationID",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DesignationID",
                table: "Users");
        }
    }
}
