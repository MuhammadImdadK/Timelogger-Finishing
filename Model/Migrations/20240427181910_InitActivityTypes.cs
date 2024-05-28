using Microsoft.EntityFrameworkCore.Migrations;
using Model.ModelSql;
using NLog.Layouts;
using static System.Formats.Asn1.AsnWriter;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Reflection.Emit;
using System.Runtime.Intrinsics.X86;

#nullable disable

namespace Model.Migrations
{
    /// <inheritdoc />
    public partial class InitActivityTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ActivityTypes",
                columns: new[] { "Name", "IsDefault", "IsActive", "Created", "Modified" },
                values: new object[,]
                {
                    {
                        "IFI",
                        true,
                        true,
                        DateTime.Now.ToUniversalTime() , 
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "IFA",
                        true,
                        true,
                        DateTime.Now.ToUniversalTime(), 
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "IDC",
                        true,
                        true,
                        DateTime.Now.ToUniversalTime(), 
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "ISSUED FOR EDR",
                        true,
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "ISSUED FOR HAZOP",
                        true,
                        true,
                        DateTime.Now.ToUniversalTime(),
                        DateTime.Now.ToUniversalTime()
                    },
                    {
                        "AFC",
                        true,
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
