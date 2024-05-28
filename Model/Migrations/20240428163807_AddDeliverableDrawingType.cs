using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Model.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliverableDrawingType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeliverableDrawingTypeID",
                table: "TimeLogs",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DeliverableDrawingTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    ModifiedBy = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliverableDrawingTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliverableDrawingTypes_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeliverableDrawingTypes_Users_ModifiedBy",
                        column: x => x.ModifiedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TimeLogs_DeliverableDrawingTypeID",
                table: "TimeLogs",
                column: "DeliverableDrawingTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_DeliverableDrawingTypes_CreatedBy",
                table: "DeliverableDrawingTypes",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DeliverableDrawingTypes_ModifiedBy",
                table: "DeliverableDrawingTypes",
                column: "ModifiedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeLogs_DeliverableDrawingTypes_DeliverableDrawingTypeID",
                table: "TimeLogs",
                column: "DeliverableDrawingTypeID",
                principalTable: "DeliverableDrawingTypes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeLogs_DeliverableDrawingTypes_DeliverableDrawingTypeID",
                table: "TimeLogs");

            migrationBuilder.DropTable(
                name: "DeliverableDrawingTypes");

            migrationBuilder.DropIndex(
                name: "IX_TimeLogs_DeliverableDrawingTypeID",
                table: "TimeLogs");

            migrationBuilder.DropColumn(
                name: "DeliverableDrawingTypeID",
                table: "TimeLogs");
        }
    }
}
