using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Model.Migrations
{
    /// <inheritdoc />
    public partial class update_drawings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drawing_Projects_ProjectID",
                table: "Drawing");

            migrationBuilder.DropIndex(
                name: "IX_Drawing_ProjectID",
                table: "Drawing");

            migrationBuilder.DropColumn(
                name: "ProjectID",
                table: "Drawing");

            migrationBuilder.CreateIndex(
                name: "IX_Drawing_ProjectId",
                table: "Drawing",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Drawing_Projects_ProjectId",
                table: "Drawing",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drawing_Projects_ProjectId",
                table: "Drawing");

            migrationBuilder.DropIndex(
                name: "IX_Drawing_ProjectId",
                table: "Drawing");

            migrationBuilder.AddColumn<int>(
                name: "ProjectID",
                table: "Drawing",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Drawing_ProjectID",
                table: "Drawing",
                column: "ProjectID");

            migrationBuilder.AddForeignKey(
                name: "FK_Drawing_Projects_ProjectID",
                table: "Drawing",
                column: "ProjectID",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
