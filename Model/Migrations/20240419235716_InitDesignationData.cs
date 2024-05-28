using Microsoft.EntityFrameworkCore.Migrations;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Net.NetworkInformation;

#nullable disable

namespace Model.Migrations
{
    /// <inheritdoc />
    public partial class InitDesignationData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Designations",
                columns: new[] { "Name", "IsActive", "Created", "Modified" },
                values: new object[,]
                {
                    {
                        "JR. PROCESS ENGR.",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "SR. PIPING ENGR.",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "JR. PIPING ENGR.",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "SR. I&C ENGR.",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "JR. I&C ENGR.",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "JR. ELECTRICAL ENGR.",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "SR. CIVIL & STRUCTURAL ENGR.",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "JR. CIVIL & STRUCTURAL ENGR.",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "PIPING DESIGNER",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "CAD OPERATOR - PIPING",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "CAD OPERATOR - CIVIL",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "CAD OPERATOR - I&E",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "CAD OPERATOR - PROCESS",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "DOCUMENTATION & LIBRARY ASSISTANT",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "JR. PLANNING ENGR.",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "UEPDH ENGINEERING MANAGER",
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "SR. PROCESS ENGR."
                        ,true
                        ,DateTime.Now.ToUniversalTime()
                        ,DateTime.Now.ToUniversalTime()
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
