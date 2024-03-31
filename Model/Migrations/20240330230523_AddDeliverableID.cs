using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Model.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliverableID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeliverableID",
                table: "TimeLogs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeliverableID",
                table: "Requests",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimeLogs_DeliverableID",
                table: "TimeLogs",
                column: "DeliverableID");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_DeliverableID",
                table: "Requests",
                column: "DeliverableID");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Drawing_DeliverableID",
                table: "Requests",
                column: "DeliverableID",
                principalTable: "Drawing",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeLogs_Drawing_DeliverableID",
                table: "TimeLogs",
                column: "DeliverableID",
                principalTable: "Drawing",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Drawing_DeliverableID",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeLogs_Drawing_DeliverableID",
                table: "TimeLogs");

            migrationBuilder.DropIndex(
                name: "IX_TimeLogs_DeliverableID",
                table: "TimeLogs");

            migrationBuilder.DropIndex(
                name: "IX_Requests_DeliverableID",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "DeliverableID",
                table: "TimeLogs");

            migrationBuilder.DropColumn(
                name: "DeliverableID",
                table: "Requests");
        }
    }
}
