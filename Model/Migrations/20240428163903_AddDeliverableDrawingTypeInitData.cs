using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Model.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliverableDrawingTypeInitData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "DeliverableDrawingTypes",
                columns: new[] { "Name", "IsActive", "Created", "Modified" },
                values: new object[,]
                {
                    {
                        "P&ID-1",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "P&ID-2",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "P&ID-3",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "P&ID-4",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "P&ID-5",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "P&ID-6",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "P&ID-7",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "P&ID-8",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Cause & Effect Diagram.",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "HAZOP",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Line List",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Equipment List",
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
                        "Equipment Plot Plan",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Piping Plan-1",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Piping Plan-2",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Piping Plan-3",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Piping Plan-4",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Piping Plan-5",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Piping Plan-6",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Piping Plan-7",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Piping Plan-8",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Piping Isometric",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Mechanical Fabrication Dwg.",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Bill of Materials Level 1",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Bill of Materials Level 2",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Bill of Materials Level 3",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Piping BOQ",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Hazardous Area Layout",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Tubing Layout",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "SSP Drawings",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Compressor Drawings",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Instrument Data Sheets",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Equipment Spec. Sheet",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Cable Layout Plan",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Grounding Plan",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Area Lighting Plan",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Cable Schedule",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "SLD",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "RTU Card Termination",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "PDB / DB Dwg.",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "I&E Scope of work",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "I&E BOQ*",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "I&E BOM",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Foundation Location Plan",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Equipment Foundation Dwgs.",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Pipe Support / Structure Foundation Dwgs.",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Relevant Standard Drawings",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Any Others: (Civil/Structural Dwgs.)",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Civil BOQ",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Civil BOM",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Demolition BOM",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Study Report",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Ladder Dwg.",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "FJB / SJB Dwg.",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Schematic Motor Control Dwg.",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "Others",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    }
                }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
