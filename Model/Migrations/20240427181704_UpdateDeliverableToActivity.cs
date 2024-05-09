using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Model.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDeliverableToActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Drawing");

            migrationBuilder.AddColumn<int>(
                name: "ActivityTypeId",
                table: "Drawing",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ActivityTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    ModifiedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityTypes_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ActivityTypes_Users_ModifiedBy",
                        column: x => x.ModifiedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Drawing_ActivityTypeId",
                table: "Drawing",
                column: "ActivityTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityTypes_CreatedBy",
                table: "ActivityTypes",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityTypes_ModifiedBy",
                table: "ActivityTypes",
                column: "ModifiedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Drawing_ActivityTypes_ActivityTypeId",
                table: "Drawing",
                column: "ActivityTypeId",
                principalTable: "ActivityTypes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drawing_ActivityTypes_ActivityTypeId",
                table: "Drawing");

            migrationBuilder.DropTable(
                name: "ActivityTypes");

            migrationBuilder.DropIndex(
                name: "IX_Drawing_ActivityTypeId",
                table: "Drawing");

            migrationBuilder.DropColumn(
                name: "ActivityTypeId",
                table: "Drawing");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Drawing",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
