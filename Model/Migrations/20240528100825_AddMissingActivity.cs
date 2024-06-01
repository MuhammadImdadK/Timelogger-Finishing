using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Model.Migrations
{
    public partial class AddMissingActivity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
              table: "ActivityTypes",
              columns: new[] { "Name", "IsDefault", "IsActive", "Created", "Modified" },
              values: new object[,]
              {
                    {
                        "RE-IFI",
                        true,
                        true,
                        DateTime.Now.ToUniversalTime() ,
                        DateTime.Now.ToUniversalTime()
                    }
              });
      }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
