using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Model.Migrations
{
    public partial class AddMissingDeliverables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
              table: "DeliverableDrawingTypes",
              columns: new[] { "Name", "IsActive", "Created", "Modified" },
              values: new object[,]
              {
                    {
                        "Rig Layout Plan",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "STANDARD DRAWINGS",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "GRADING LAYOUT PLAN",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "CIVIL BOQ",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "CRS",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "MOM",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "KICK OFF",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Integration meeting",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
              });

      }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
