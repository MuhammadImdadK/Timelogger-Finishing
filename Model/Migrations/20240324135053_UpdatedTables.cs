using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Model.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "TimeLogs",
                newName: "StartDateTime");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "TimeLogs",
                newName: "EndDateTime");

            migrationBuilder.AddColumn<int>(
                name: "DisciplineType",
                table: "TimeLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DrawingType",
                table: "TimeLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ScopeType",
                table: "TimeLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TeamType",
                table: "TimeLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ERFNumber",
                table: "Projects",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Drawing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    ProjectID = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    ModifiedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drawing", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Drawing_Projects_ProjectID",
                        column: x => x.ProjectID,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Drawing_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Drawing_Users_ModifiedBy",
                        column: x => x.ModifiedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Drawing_CreatedBy",
                table: "Drawing",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Drawing_ModifiedBy",
                table: "Drawing",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Drawing_ProjectID",
                table: "Drawing",
                column: "ProjectID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Drawing");

            migrationBuilder.DropColumn(
                name: "DisciplineType",
                table: "TimeLogs");

            migrationBuilder.DropColumn(
                name: "DrawingType",
                table: "TimeLogs");

            migrationBuilder.DropColumn(
                name: "ScopeType",
                table: "TimeLogs");

            migrationBuilder.DropColumn(
                name: "TeamType",
                table: "TimeLogs");

            migrationBuilder.DropColumn(
                name: "ERFNumber",
                table: "Projects");

            migrationBuilder.RenameColumn(
                name: "StartDateTime",
                table: "TimeLogs",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "EndDateTime",
                table: "TimeLogs",
                newName: "EndTime");
        }
    }
}
