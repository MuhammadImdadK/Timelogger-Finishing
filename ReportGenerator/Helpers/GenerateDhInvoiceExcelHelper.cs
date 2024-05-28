// <copyright file="BpgRunInfo.cs" company="Dylan Eddies">
// Coyright (C) Dylan EddiesAll Rights reserved.
// </copyright>

namespace ReportGenerator.EmailReporting.Helpers;

using System;
using System.ComponentModel;
using System.Drawing;
using System.Net.WebSockets;
using System.Reflection;
using System.Text.Json;
using System.Web;
using Autofac;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office.Word;
using HandlebarsDotNet;
using Model.ModelSql;
using ReportGenerator.Helpers;

public static class HelperUtils
{
    public static string GetEnumDescription(Enum enumObj)
    {
        FieldInfo fieldInfo = enumObj.GetType().GetField(enumObj.ToString());
        object[] attribArray = fieldInfo.GetCustomAttributes(false);

        if (attribArray.Length == 0)
            return enumObj.ToString();
        else
        {
            DescriptionAttribute attrib = null;

            foreach (var att in attribArray)
            {
                if (att is DescriptionAttribute)
                    attrib = att as DescriptionAttribute;
            }

            if (attrib != null)
                return attrib.Description;

            return enumObj.ToString();
        }
    }
}

[HandleBarsHelper(Name = "teamName")]
public class TeamNameHelper : ConsoleHandlebarsHelper
{
    public TeamNameHelper(Autofac.IContainer container) : base(container)
    {
    }

    public override void Execute(EncodedTextWriter writer, Context context, Arguments parameters)
    {
        var doc = JsonSerializer.Deserialize<ReportData>(HandlebarsController.JsonDocument);
        writer.Write($"<b style=\"color: #fefefe;\">{HelperUtils.GetEnumDescription(doc.TeamType)}</b>", false);
    }
}

public class WorkLogValues
{
    public int ProjectId { get; set; }
    public double InHours { get; set; }
    public double OutOfHours { get; set; }
    public double Overtime { get; set; }
    public double CostInHours { get; set; }
    public double CostOutOfHours { get; set; }
    public double CostOvertime { get; set; }
}


[HandleBarsHelper(Name = "genUepdhInvoiceXl")]
public class GenerateUEPDHInvoiceExcelHelper : ConsoleHandlebarsHelper
{
    private const string CacheDataFolder = "InformaticaSystems";

    public GenerateUEPDHInvoiceExcelHelper(Autofac.IContainer container) : base(container)
    {
    }

    public override void Execute(EncodedTextWriter writer, Context context, Arguments parameters)
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        if (!Directory.Exists(Path.Join(appData, CacheDataFolder)))
        {
            Directory.CreateDirectory(Path.Combine(appData, CacheDataFolder));
        }

        var outputFile = Path.Join(appData, CacheDataFolder, $"UEPDH_Invoice_{DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss")}.xlsx");
        var doc = JsonSerializer.Deserialize<ReportData>(HandlebarsController.JsonDocument);
        using var wbTemp = new XLWorkbook();
        var ws = wbTemp.Worksheets.Add("Ref Data");
        var ws2 = wbTemp.Worksheets.Add("invoice ueppb");
        wbTemp.SaveAs(outputFile);
        using var wb = new XLWorkbook(outputFile);
        ws = wb.Worksheets.ToList()[0];
        ws2 = wb.Worksheets.ToList()[1];

        ws.SheetView.FreezeColumns(4);

        ws.Cell(4, 1).Value = "DEST INVOICE NO.:";
        ws.Cell(4, 3).FormulaA1 = "=CONCATENATE(\"\",$'invoice ueppb'.C4,\" / \",$'invoice uepapb'.C4,\" / \",$'invoice uepbpb'.C4)";

        ws.Cell(5, 1).Value = "DEST PROJECT NO.:";
        ws.Cell(6, 1).Value = "DEST PROJECT TITLE:";
        ws.Cell(6, 3).Value = "UEP DESIGN HOUSE SERVICES";
        ws.Cell(7, 1).Value = "UEP CONTRACT NO.:";
        ws.Cell(8, 1).Value = "UEP RO NO.:";
        ws.Cell(8, 3).FormulaA1 = "=CONCATENATE($'INVOICE UEPPB'.C8,\" / \",$'INVOICE UEPAPB'.C8,\" / \",$'INVOICE UEPBPB'.C8)";
        ws.Cell(9, 1).Value = "MONTH:";
        ws.Cell(9, 3).Value = doc.Month;
        ws.Cell(10, 1).Value = "Plant:";
        ws.Cell(10, 3).Value = "REFER INVOICES";
        ws.Cell(11, 1).Value = "Plant Code:";
        ws.Cell(11, 3).Value = "REFER INVOICES";


        var headColor = Color.FromArgb(0, 0, 255);
        var headBgColor = Color.FromArgb(192, 192, 192);
        var borderColor = Color.FromArgb(0, 0, 0);
        var designationHeaderCell = ws.Cell(13, 3);

        ws.Cell(14, 5).Value = HelperUtils.GetEnumDescription(doc.TeamType);
        ws.Range("E14:J14").Merge();
        ws.Cell(14, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        this.SetBorder(ws.Cell(14, 5), XLColor.FromColor(borderColor), XLBorderStyleValues.Medium);

        var pad = new[] { 12, 13, 14 };
        for (int i = 0; i < pad.Length; i++)
        {
            for (int x = 0; x < 2; x++)
            {
                ws.Cell(pad[i], x + 1).Value = " ";
            }
        }

        ws.Cell(12, 3).Value = " ";

        var tableHeadCells = new[] { "SR No", "ERF #", "Project Name", "Direct/Indirect Cost", "Cost (CC)", "Manhours", "Total Hrs.", "% Total Hrs.", "Cost", "%age Cost (PCC)" };
        var temp = 1;
        var employeeStart = 7;
        var userCol = new Dictionary<int, int>();

        var add = 0;
        var col = string.Empty;
        var colEnd = string.Empty;
        for (var i = 0; i < tableHeadCells.Length; i++)
        {
            var theadCell = ws.Cell(15, i + 1 + add);
            theadCell.Value = tableHeadCells[i];
            theadCell.Style.Font.FontColor = XLColor.FromColor(headColor);
            theadCell.Style.Font.SetBold(true);
            theadCell.Style.Fill.SetBackgroundColor(XLColor.FromColor(headBgColor));
            var val = tableHeadCells[i];
            if (val == "Manhours")
            {
                col = ws.Column(i + temp).ColumnLetter();
                colEnd = ws.Column(i + temp).ColumnLetter();
                try
                {
                    ws.Range($"{col}15:{colEnd}15").Merge();
                    add = temp - 1;
                }
                catch (Exception ex)
                {
                    // .-.
                }
            }
            this.SetBorder(theadCell, XLColor.FromColor(borderColor));
        }

        var tempProj = 0;
        var projStart = 16;
        var projRow = new Dictionary<int, int>();

        foreach (var proj in doc.Projects)
        {
            var srNoCell = ws.Cell(projStart + tempProj, 1);
            var erfNoCell = ws.Cell(projStart + tempProj, 2);
            var projNameCell = ws.Cell(projStart + tempProj, 3);
            var idCostCell = ws.Cell(projStart + tempProj, 4);
            srNoCell.Style.Fill.SetBackgroundColor(XLColor.FromColor(headBgColor));
            this.SetBorder(srNoCell, XLColor.FromColor(borderColor));
            srNoCell.Value = tempProj + 1;
            erfNoCell.Style.Fill.SetBackgroundColor(XLColor.FromColor(headBgColor));
            this.SetBorder(erfNoCell, XLColor.FromColor(borderColor));
            erfNoCell.Value = proj.ERFNumber;
            this.SetBorder(projNameCell, XLColor.FromColor(borderColor));
            projNameCell.Value = proj.ProjectName;
            this.SetBorder(idCostCell, XLColor.FromColor(borderColor));
            idCostCell.Value = "Direct Cost";

            if (!projRow.ContainsKey(proj.Id ?? 0))
            {
                projRow.Add(proj.Id ?? 0, projStart + tempProj);
            }

            tempProj++;
        }


        foreach (var proj in projRow)
        {
            var projCostCell = ws.Cell(proj.Value, 5);
            projCostCell.Value = 1.0;
            var projCell = ws.Cell(proj.Value, 6);
            var timeLogs = doc.TimeLogs.Where(itm => itm.ProjectID == proj.Key);
            double ttl = 0;

            if (timeLogs.Any())
            {
                var all = timeLogs.Select(itm => itm.Duration.TotalHours).ToList();
                foreach (var t in all)
                {
                    ttl += t;
                }
            }
            projCell.Value = ttl;
            this.SetBorder(projCell, XLColor.FromColor(borderColor));
            this.SetBorder(projCostCell, XLColor.FromColor(borderColor));
        }


        var allTtlHrsCol = ws.Column(4 + add + 3).ColumnLetter();
        var allPercHrsCol = ws.Column(4 + add + 4).ColumnLetter();
        var allCCCostCol = ws.Column(5).ColumnLetter();
        var allCostCol = ws.Column(4 + add + 5).ColumnLetter();
        var minRow = projRow.Min(itm => itm.Value);
        var row = projRow.Max(itm => itm.Value) + 1;

        var allTtlHrsColCell = ws.Cell(row, 4 + add + 3);
        allTtlHrsColCell.Style.Font.FontColor = XLColor.FromColor(headColor);
        this.SetBorder(allTtlHrsColCell, XLColor.FromColor(borderColor));
        var allPercHrsColCell = ws.Cell(row, 4 + add + 4);
        allPercHrsColCell.Style.Font.FontColor = XLColor.FromColor(headColor);
        this.SetBorder(allPercHrsColCell, XLColor.FromColor(borderColor));
        var allCostColCell = ws.Cell(row, 4 + add + 5);
        allCostColCell.Style.Font.FontColor = XLColor.FromColor(headColor);
        this.SetBorder(allCostColCell, XLColor.FromColor(borderColor));
        var allCCCostCell = ws.Cell(row, 5);
        allCCCostCell.Style.Font.FontColor = XLColor.FromColor(headColor);
        this.SetBorder(allCCCostCell, XLColor.FromColor(borderColor));
        allCCCostCell.FormulaA1 = $"=SUM({allCCCostCol}{minRow}:{allCCCostCol}{row - 1})";
        allTtlHrsColCell.FormulaA1 = $"=SUM({allTtlHrsCol}{minRow}:{allTtlHrsCol}{row - 1})";
        allPercHrsColCell.FormulaA1 = $"=SUM({allPercHrsCol}{minRow}:{allPercHrsCol}{row - 1})";
        allCostColCell.FormulaA1 = $"=SUM({allCostCol}{minRow}:{allCostCol}{row - 1})";

        foreach (var proj in projRow)
        {
            var ttlCell = ws.Cell(proj.Value, 4 + add + 3);
            ttlCell.FormulaA1 = $"=SUM({col}{proj.Value}:{colEnd}{proj.Value})";
            this.SetBorder(ttlCell, XLColor.FromColor(borderColor));
            var ttlPercHrCell = ws.Cell(proj.Value, 4 + add + 4);
            ttlPercHrCell.FormulaA1 = $"={allTtlHrsCol}{proj.Value}/{allTtlHrsCol}{row}";
            this.SetBorder(ttlPercHrCell, XLColor.FromColor(borderColor));
            var ttlCostCell = ws.Cell(proj.Value, 4 + add + 5);
            ttlCostCell.FormulaA1 = $"={temp}*{allPercHrsCol}{proj.Value}";
            this.SetBorder(ttlCostCell, XLColor.FromColor(borderColor));
            var percCostCell = ws.Cell(proj.Value, 4 + add + 6);
            percCostCell.FormulaA1 = $"={allCCCostCol}{proj.Value}/{allCCCostCol}{row}";
            this.SetBorder(percCostCell, XLColor.FromColor(borderColor));
        }
        for (var i = 0; i < 4 + add; i++)
        {
            ws.Cell(row, i + 1).Value = " ";
        }

        var notesCell = ws.Cell(row + 4, 1);
        notesCell.Value = "Notes:";
        notesCell.Style.Font.SetBold(true);

        var workHours = Enumerable.Range(1, DateTime.DaysInMonth(doc.StartDate.Year, doc.StartDate.Month))
        .Select(day => new DateTime(doc.StartDate.Year, doc.StartDate.Month, day))
                    .Where(dt => dt.DayOfWeek != DayOfWeek.Sunday &&
                                 dt.DayOfWeek != DayOfWeek.Saturday)
                    .ToList().Count() * 8;
        var workLogs = doc.TimeLogs.Select(itm =>
        {
            var time = (itm.EndDateTime ?? itm.StartDateTime).ToLocalTime();
            var overtimeStart = itm.StartDateTime.ToLocalTime().Date.AddHours(17);
            var overtimeBeforeStart = itm.StartDateTime.ToLocalTime().Date.AddHours(8);
            var user = doc.Users.FirstOrDefault(user => user.Id == itm.UserID);
            var desig = doc.Designations.FirstOrDefault(desig => desig.Id == user.DesignationID);
            var rates = doc.DesignationRates.FirstOrDefault(rates => desig.Id == rates.DesignationID);

            if (itm.StartDateTime < overtimeBeforeStart && (itm.StartDateTime.DayOfWeek != DayOfWeek.Sunday && itm.StartDateTime.DayOfWeek != DayOfWeek.Saturday))
            {
                var timeBefore = (overtimeBeforeStart - itm.StartDateTime).TotalHours;
                var regularTime = itm.Duration.TotalHours - timeBefore;
                return new WorkLogValues()
                {
                    ProjectId = itm.ProjectID,
                    Overtime = timeBefore,
                    CostOvertime = timeBefore * rates.OvertimeRate,
                    InHours = regularTime,
                    CostInHours = regularTime * rates.OvertimeRate,
                };
            }
            else if (itm.StartDateTime > overtimeStart && (itm.StartDateTime.DayOfWeek != DayOfWeek.Sunday && itm.StartDateTime.DayOfWeek != DayOfWeek.Saturday))
            {
                return new WorkLogValues()
                {
                    ProjectId = itm.ProjectID,
                    Overtime = itm.Duration.TotalHours,
                    CostOvertime = itm.Duration.TotalHours * rates.OvertimeRate
                };
            }
            else if (itm.StartDateTime.DayOfWeek == DayOfWeek.Sunday || itm.StartDateTime.DayOfWeek == DayOfWeek.Saturday)
            {
                return new WorkLogValues()
                {
                    ProjectId = itm.ProjectID,
                    OutOfHours = itm.Duration.TotalHours,
                    CostOutOfHours = itm.Duration.TotalHours * rates.OutsideHourRate
                };
            }
            else
            {
                var timeLogValues = new WorkLogValues();
                timeLogValues.ProjectId = itm.ProjectID;

                if (itm.EndDateTime > overtimeStart)
                {
                    var diff = (time - overtimeStart);
                    timeLogValues.Overtime = diff.TotalHours;
                    timeLogValues.CostOvertime = timeLogValues.Overtime * rates.OvertimeRate;
                    timeLogValues.InHours = itm.Duration.TotalHours - diff.TotalHours;
                    timeLogValues.CostInHours = timeLogValues.InHours * rates.BaseRate;
                }
                else
                {
                    timeLogValues.InHours = itm.Duration.TotalHours;
                    timeLogValues.CostInHours = timeLogValues.InHours * rates.BaseRate;
                }
                return timeLogValues;
            }
        });

        var ws2tempProj = 0;
        var ws2projStart = 16;
        var ws2projRow = new Dictionary<int, int>();

        foreach (var proj in doc.Projects)
        {
            var srNoCell = ws2.Cell(ws2projStart + ws2tempProj, 1);
            var erfNoCell = ws2.Cell(ws2projStart + ws2tempProj, 2);
            var projNameCell = ws2.Cell(ws2projStart + ws2tempProj, 3);
            var baseMh = ws2.Cell(ws2projStart + ws2tempProj, 4);
            var baseC = ws2.Cell(ws2projStart + ws2tempProj, 5);
            var otMh = ws2.Cell(ws2projStart + ws2tempProj, 6);
            var otC = ws2.Cell(ws2projStart + ws2tempProj, 7);
            var oohMh = ws2.Cell(ws2projStart + ws2tempProj, 8);
            var oohC = ws2.Cell(ws2projStart + ws2tempProj, 9);
            srNoCell.Style.Fill.SetBackgroundColor(XLColor.FromColor(headBgColor));
            this.SetBorder(srNoCell, XLColor.FromColor(borderColor));
            srNoCell.Value = ws2tempProj + 1;
            erfNoCell.Style.Fill.SetBackgroundColor(XLColor.FromColor(headBgColor));
            this.SetBorder(erfNoCell, XLColor.FromColor(borderColor));
            erfNoCell.Value = proj.ERFNumber;
            this.SetBorder(projNameCell, XLColor.FromColor(borderColor));
            projNameCell.Value = proj.ProjectName;

            baseMh.Style.Font.SetBold(true);
            double baseMhTTl = 0;
            this.SetBorder(baseMh, XLColor.FromColor(borderColor));
            baseC.Style.Font.SetBold(true);
            double baseCTTl = 0;


            this.SetBorder(baseC, XLColor.FromColor(borderColor));
            otMh.Style.Font.SetBold(true);
            double otMhTTl = 0;
            this.SetBorder(otMh, XLColor.FromColor(borderColor));
            otC.Style.Font.SetBold(true);
            double otCTTl = 0;
            this.SetBorder(otC, XLColor.FromColor(borderColor));
            oohMh.Style.Font.SetBold(true);
            double oohMhTTl = 0;
            this.SetBorder(oohMh, XLColor.FromColor(borderColor));
            oohC.Style.Font.SetBold(true);
            double oohCTTl = 0;
            this.SetBorder(oohC, XLColor.FromColor(borderColor));

            foreach (var itm in workLogs)
            {
                if (itm.ProjectId != proj.Id)
                {
                    continue;
                }
                baseMhTTl += itm.InHours;
                baseCTTl += itm.CostInHours;
                otMhTTl += itm.Overtime;
                otCTTl += itm.CostOvertime;
                oohMhTTl += itm.OutOfHours;
                oohCTTl += itm.CostOutOfHours;
            }

            baseMh.Value = baseMhTTl;
            baseC.Value = baseCTTl;
            otMh.Value = otMhTTl;
            otC.Value = otCTTl;
            oohMh.Value = oohMhTTl;
            oohC.Value = oohCTTl;

            if (!projRow.ContainsKey(proj.Id ?? 0))
            {
                projRow.Add(proj.Id ?? 0, ws2projStart + ws2tempProj);
            }

            ws2tempProj++;
        }

        ws2.Cell(4, 1).Value = "DEST INVOICE NO.:";
        ws2.Cell(4, 3).Value = "XXXXXXXXXX";
        ws2.Cell(5, 1).Value = "DEST PROJECT NO.:";
        ws2.Cell(6, 1).Value = "DEST PROJECT TITLE:";
        ws2.Cell(6, 3).Value = "UEP DESIGN HOUSE SERVICES";
        ws2.Cell(7, 1).Value = "UEP CONTRACT NO.:";
        ws2.Cell(8, 1).Value = "UEP RO NO.:";
        ws2.Cell(8, 3).Value = "XXXXXXXXXX";
        ws2.Cell(9, 1).Value = "MONTH:";
        ws2.Cell(9, 3).Value = doc.Month;
        ws2.Cell(10, 1).Value = "Plant:";
        ws2.Cell(10, 3).Value = "UEPPB";
        ws2.Cell(11, 1).Value = "Plant Code:";
        ws2.Cell(11, 3).Value = "1000";

        ws2.Cell(14, 4).Value = HelperUtils.GetEnumDescription(doc.TeamType);
        ws2.Range("D14:E14").Merge();
        ws2.Cell(14, 6).Value = "OVER TIME";
        ws2.Range("F14:G14").Merge();
        ws2.Cell(14, 8).Value = "OUT OF HOURS (SAT,SUN)";
        ws2.Range("H14:I14").Merge();
        this.SetBorder(ws2.Cell(14, 4), XLColor.FromColor(borderColor), XLBorderStyleValues.Double);
        this.SetBorder(ws2.Cell(14, 6), XLColor.FromColor(borderColor), XLBorderStyleValues.Double);
        this.SetBorder(ws2.Cell(14, 8), XLColor.FromColor(borderColor), XLBorderStyleValues.Double);


        var ws2tableHeadCells = new[] { "SR No", "ERF #", "Project Name", "Man Hours", "Cost", "Man Hours", "Cost", "Man Hours", "Cost" };
        for (var i = 0; i < ws2tableHeadCells.Length; i++)
        {
            var theadCell = ws2.Cell(15, i + 1 + add);
            theadCell.Value = ws2tableHeadCells[i];
            theadCell.Style.Font.FontColor = XLColor.FromColor(headColor);
            theadCell.Style.Font.SetBold(true);
            theadCell.Style.Fill.SetBackgroundColor(XLColor.FromColor(headBgColor));
            var val = ws2tableHeadCells[i];
            this.SetBorder(theadCell, XLColor.FromColor(borderColor));
        }
        var tbl = "<table>";

        foreach (var usedRow in ws.RowsUsed())
        {
            tbl += "<tr style=\"border: none; background:none;color: #fefefe;\">";
            foreach (var cell in usedRow.CellsUsed())
            {
                try
                {
                    if (cell.HasFormula)
                    {
                        wb.CalculateMode = ClosedXML.Excel.XLCalculateMode.Auto;
                        tbl += $"<td style=\"border: none; background:none;color: #fefefe;\">{HttpUtility.HtmlEncode(cell.Value)}</td>";

                    }
                    else
                    {
                        tbl += $"<td style=\"border: none; background:none;color: #fefefe;\">{HttpUtility.HtmlEncode(cell.Value)}</td>";
                    }
                    if (cell.Value.ToString() == "Manhours" && cell.IsMerged())
                    {
                        for (var i = 0; i < cell.MergedRange().ColumnCount() - 1; i++)
                        {
                            tbl += "<td></td>";
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }

            tbl += "</tr>";
        }

        tbl += "</table>";

        // save
        wb.SaveAs(outputFile);
        writer.Write($"<b style=\"color: #fefefe;\">Open the full invoice in <a href=\"{new Uri(outputFile)}\">Excel</a></b><br/>{tbl}", false);
    }
    private void SetBorder(IXLCell cell, XLColor borderColor, XLBorderStyleValues borderStyleValues = XLBorderStyleValues.Thin)
    {
        cell.Style.Border.BottomBorder = borderStyleValues;
        cell.Style.Border.BottomBorderColor = borderColor;
        cell.Style.Border.LeftBorder = borderStyleValues;
        cell.Style.Border.LeftBorderColor = borderColor;
        cell.Style.Border.RightBorder = borderStyleValues;
        cell.Style.Border.RightBorderColor = borderColor;
        cell.Style.Border.TopBorder = borderStyleValues;
        cell.Style.Border.TopBorderColor = borderColor;
    }
}


[HandleBarsHelper(Name = "genDhInvoiceXl")]
public class GenerateDhInvoiceExcelHelper : ConsoleHandlebarsHelper
{
    private const string CacheDataFolder = "InformaticaSystems";

    public GenerateDhInvoiceExcelHelper(Autofac.IContainer container) : base(container)
    {
    }

    public override void Execute(EncodedTextWriter writer, Context context, Arguments parameters)
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        if (!Directory.Exists(Path.Join(appData, CacheDataFolder)))
        {
            Directory.CreateDirectory(Path.Combine(appData, CacheDataFolder));
        }

        var outputFile = Path.Join(appData, CacheDataFolder, $"DH_Invoice_{DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss")}.xlsx");
        var doc = JsonSerializer.Deserialize<ReportData>(HandlebarsController.JsonDocument);
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("DH Invoice");
        ws.SheetView.FreezeColumns(3);

        ws.Cell(6, 1).Value = "DEST INVOICE NO.:";
        ws.Cell(7, 1).Value = "DEST PROJECT NO.:";
        ws.Cell(8, 1).Value = "DEST PROJECT TITLE:";
        ws.Cell(8, 3).Value = "UEP DESIGN HOUSE SERVICES";
        ws.Cell(9, 1).Value = "UEP CONTRACT NO.:";
        ws.Cell(10, 1).Value = "UEP RELEASE ORDER NO.:";
        ws.Cell(10, 3).Value = "XXXXXX";
        ws.Cell(11, 1).Value = "MONTH:";
        ws.Cell(11, 3).Value = doc.Month;

        var headColor = Color.FromArgb(0, 0, 255);
        var headBgColor = Color.FromArgb(192, 192, 192);
        var borderColor = Color.FromArgb(0, 0, 0);
        var designationHeaderCell = ws.Cell(13, 3);
        designationHeaderCell.Value = "UEP CORE TEAM DESIGNATION";
        designationHeaderCell.Style.Font.FontColor = XLColor.FromColor(headColor);
        designationHeaderCell.Style.Font.SetBold(true);
        designationHeaderCell.Style.Alignment.SetTextRotation(90);
        this.SetBorder(designationHeaderCell, XLColor.FromColor(borderColor));
        var employeeHeaderCell = ws.Cell(14, 3);
        employeeHeaderCell.Value = "Name / EMP #";
        employeeHeaderCell.Style.Font.FontColor = XLColor.FromColor(headColor);
        employeeHeaderCell.Style.Font.SetBold(true);
        employeeHeaderCell.Style.Alignment.SetTextRotation(90);
        this.SetBorder(employeeHeaderCell, XLColor.FromColor(borderColor));

        var pad = new[] { 12, 13, 14 };
        for (int i = 0; i < pad.Length; i++)
        {
            for (int x = 0; x < 2; x++)
            {
                ws.Cell(pad[i], x + 1).Value = " ";
            }
        }

        ws.Cell(12, 3).Value = " ";

        var tableHeadCells = new[] { "SR No", "ERF #", "Project Name", "Manhours", "Total Hrs.", "% Total Hrs.", "Cost" };
        var temp = 0;
        var employeeStart = 4;
        var userCol = new Dictionary<int, int>();
        foreach (var user in doc.Users)
        {
            var theadNoCell = ws.Cell(12, employeeStart + temp);
            var theadDesigCell = ws.Cell(13, employeeStart + temp);
            var theadEmpCell = ws.Cell(14, employeeStart + temp);
            theadNoCell.Value = temp + 1;
            theadDesigCell.Value = doc.Designations.Where(itm => itm.Id == user.DesignationID).FirstOrDefault()?.Name ?? string.Empty;
            theadEmpCell.Value = $"{user.FirstName} {user.LastName} / {user.EmployeeNumber}";
            this.SetBorder(theadDesigCell, XLColor.FromColor(borderColor));
            this.SetBorder(theadEmpCell, XLColor.FromColor(borderColor));
            theadDesigCell.Style.Alignment.SetTextRotation(90);
            theadEmpCell.Style.Alignment.SetTextRotation(90);
            var empId = user.Id ?? 0;

            if (!userCol.ContainsKey(empId))
            {
                userCol.Add(empId, employeeStart + temp);
            }

            temp++;
        }

        var add = 0;
        var col = string.Empty;
        var colEnd = string.Empty;
        for (var i = 0; i < tableHeadCells.Length; i++)
        {
            var theadCell = ws.Cell(15, i + 1 + add);
            theadCell.Value = tableHeadCells[i];
            theadCell.Style.Font.FontColor = XLColor.FromColor(headColor);
            theadCell.Style.Font.SetBold(true);
            theadCell.Style.Fill.SetBackgroundColor(XLColor.FromColor(headBgColor));
            var val = tableHeadCells[i];
            if (val == "Manhours")
            {
                col = ws.Column(i + 1).ColumnLetter();
                colEnd = ws.Column(i + temp).ColumnLetter();
                try
                {
                    ws.Range($"{col}15:{colEnd}15").Merge();
                    add = temp - 1;
                }
                catch (Exception ex)
                {
                    // .-.
                }
            }
            this.SetBorder(theadCell, XLColor.FromColor(borderColor));
        }

        var tempProj = 0;
        var projStart = 16;
        var projRow = new Dictionary<int, int>();

        foreach (var proj in doc.Projects)
        {
            var srNoCell = ws.Cell(16 + tempProj, 1);
            var erfNoCell = ws.Cell(16 + tempProj, 2);
            var projNameCell = ws.Cell(16 + tempProj, 3);
            srNoCell.Style.Fill.SetBackgroundColor(XLColor.FromColor(headBgColor));
            this.SetBorder(srNoCell, XLColor.FromColor(borderColor));
            srNoCell.Value = tempProj + 1;
            erfNoCell.Style.Fill.SetBackgroundColor(XLColor.FromColor(headBgColor));
            this.SetBorder(erfNoCell, XLColor.FromColor(borderColor));
            erfNoCell.Value = proj.ERFNumber;
            this.SetBorder(projNameCell, XLColor.FromColor(borderColor));
            projNameCell.Value = proj.ProjectName;

            if (!projRow.ContainsKey(proj.Id ?? 0))
            {
                projRow.Add(proj.Id ?? 0, projStart + tempProj);
            }

            tempProj++;
        }

        foreach (var user in userCol)
        {
            foreach (var proj in projRow)
            {
                var projCell = ws.Cell(proj.Value, user.Value);
                var timeLogs = doc.TimeLogs.Where(itm => itm.ProjectID == proj.Key && itm.UserID == user.Key);
                double ttl = 0;

                if (timeLogs.Any())
                {
                    var all = timeLogs.Select(itm => itm.Duration.TotalHours).ToList();
                    foreach (var t in all)
                    {
                        ttl += t;
                    }
                }
                projCell.Value = ttl;
                this.SetBorder(projCell, XLColor.FromColor(borderColor));

            }
        }

        var allTtlHrsCol = ws.Column(4 + add + 1).ColumnLetter();
        var allPercHrsCol = ws.Column(4 + add + 2).ColumnLetter();
        var allCostCol = ws.Column(4 + add + 3).ColumnLetter();
        var minRow = projRow.Min(itm => itm.Value);
        var row = projRow.Max(itm => itm.Value) + 1;

        var allTtlHrsColCell = ws.Cell(row, 4 + add + 1);
        allTtlHrsColCell.Style.Font.FontColor = XLColor.FromColor(headColor);
        this.SetBorder(allTtlHrsColCell, XLColor.FromColor(borderColor));
        var allPercHrsColCell = ws.Cell(row, 4 + add + 2);
        allPercHrsColCell.Style.Font.FontColor = XLColor.FromColor(headColor);
        this.SetBorder(allPercHrsColCell, XLColor.FromColor(borderColor));
        var allCostColCell = ws.Cell(row, 4 + add + 3);
        allCostColCell.Style.Font.FontColor = XLColor.FromColor(headColor);
        this.SetBorder(allCostColCell, XLColor.FromColor(borderColor));
        allTtlHrsColCell.FormulaA1 = $"=SUM({allTtlHrsCol}{minRow}:{allTtlHrsCol}{row - 1})";
        allPercHrsColCell.FormulaA1 = $"=SUM({allPercHrsCol}{minRow}:{allPercHrsCol}{row - 1})";
        allCostColCell.FormulaA1 = $"=SUM({allCostCol}{minRow}:{allCostCol}{row - 1})";

        foreach (var proj in projRow)
        {
            var ttlCell = ws.Cell(proj.Value, 4 + add + 1);
            ttlCell.FormulaA1 = $"=SUM({col}{proj.Value}:{colEnd}{proj.Value})";
            this.SetBorder(ttlCell, XLColor.FromColor(borderColor));
            var ttlPercHrCell = ws.Cell(proj.Value, 4 + add + 2);
            ttlPercHrCell.FormulaA1 = $"={allTtlHrsCol}{proj.Value}/{allTtlHrsCol}{row}";
            this.SetBorder(ttlPercHrCell, XLColor.FromColor(borderColor));
            var ttlCostCell = ws.Cell(proj.Value, 4 + add + 3);
            ttlCostCell.FormulaA1 = $"={temp}*{allPercHrsCol}{proj.Value}";
            this.SetBorder(ttlCostCell, XLColor.FromColor(borderColor));
        }
        for (var i = 0; i < 4 + add; i++)
        {
            ws.Cell(row, i + 1).Value = " ";
        }

        var notesCell = ws.Cell(row + 4, 1);
        notesCell.Value = "Notes:";
        notesCell.Style.Font.SetBold(true);

        var workHours = Enumerable.Range(1, DateTime.DaysInMonth(doc.StartDate.Year, doc.StartDate.Month))
        .Select(day => new DateTime(doc.StartDate.Year, doc.StartDate.Month, day))
                    .Where(dt => dt.DayOfWeek != DayOfWeek.Sunday &&
                                 dt.DayOfWeek != DayOfWeek.Saturday)
                    .ToList().Count() * 8;

        var tbl = "<table>";

        var a = ws.Cell(row + 4 + 3, 1);
        a.Value = "A";
        a.Style.Font.SetBold(true);
        this.SetBorder(a, XLColor.FromColor(borderColor), XLBorderStyleValues.Double);
        a = ws.Cell(row + 4 + 3, 2);
        a.Value = $"Total working hours / person in {doc.Month}";
        a.Style.Font.SetBold(true);
        this.SetBorder(a, XLColor.FromColor(borderColor), XLBorderStyleValues.Double);
        a = ws.Cell(row + 4 + 3, 3);
        a.Value = workHours;
        a.Style.Font.SetBold(true);
        this.SetBorder(a, XLColor.FromColor(borderColor), XLBorderStyleValues.Double);

        var b = ws.Cell(row + 4 + 4, 1);
        b.Value = "B";
        b.Style.Font.SetBold(true);
        this.SetBorder(b, XLColor.FromColor(borderColor), XLBorderStyleValues.Double);
        b = ws.Cell(row + 4 + 4, 2);
        b.Value = $"Total available hrs for {temp} persons in {doc.Month}";
        b.Style.Font.SetBold(true);
        this.SetBorder(b, XLColor.FromColor(borderColor), XLBorderStyleValues.Double);
        b = ws.Cell(row + 4 + 4, 3);
        b.Value = workHours * temp;
        b.Style.Font.SetBold(true);
        this.SetBorder(b, XLColor.FromColor(borderColor), XLBorderStyleValues.Double);

        var c = ws.Cell(row + 4 + 5, 1);
        c.Value = "C";
        c.Style.Font.SetBold(true);
        this.SetBorder(c, XLColor.FromColor(borderColor), XLBorderStyleValues.Double);
        c = ws.Cell(row + 4 + 5, 2);
        c.Value = $"Actual Hours Consumed in {doc.Month}";
        c.Style.Font.SetBold(true);
        this.SetBorder(c, XLColor.FromColor(borderColor), XLBorderStyleValues.Double);
        c = ws.Cell(row + 4 + 5, 3);
        c.FormulaA1 = $"={allTtlHrsCol}{row}";
        c.Style.Font.SetBold(true);
        this.SetBorder(c, XLColor.FromColor(borderColor), XLBorderStyleValues.Double);

        var d = ws.Cell(row + 4 + 6, 1);
        d.Value = "D";
        d.Style.Font.SetBold(true);
        this.SetBorder(d, XLColor.FromColor(borderColor), XLBorderStyleValues.Double);
        d = ws.Cell(row + 4 + 6, 2);
        d.Value = $"Per month charges as per contract (Rs)";
        d.Style.Font.SetBold(true);
        this.SetBorder(d, XLColor.FromColor(borderColor), XLBorderStyleValues.Double);
        d = ws.Cell(row + 4 + 6, 3);
        d.Value = temp;
        d.Style.Font.SetBold(true);
        this.SetBorder(d, XLColor.FromColor(borderColor), XLBorderStyleValues.Double);

        foreach (var usedRow in ws.RowsUsed())
        {
            tbl += "<tr style=\"border: none; background:none;color: #fefefe;\">";
            foreach (var cell in usedRow.CellsUsed())
            {
                try
                {
                    if (cell.HasFormula)
                    {
                        wb.CalculateMode = ClosedXML.Excel.XLCalculateMode.Auto;
                        tbl += $"<td style=\"border: none; background:none;color: #fefefe;\">{HttpUtility.HtmlEncode(cell.Value)}</td>";

                    }
                    else
                    {
                        tbl += $"<td style=\"border: none; background:none;color: #fefefe;\">{HttpUtility.HtmlEncode(cell.Value)}</td>";
                    }
                    if (cell.Value.ToString() == "Manhours" && cell.IsMerged())
                    {
                        for (var i = 0; i < cell.MergedRange().ColumnCount() - 1; i++)
                        {
                            tbl += "<td></td>";
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }

            tbl += "</tr>";
        }

        tbl += "</table>";

        // save
        wb.SaveAs(outputFile);
        writer.Write($"<b style=\"color: #fefefe;\">Open the full invoice in <a href=\"{new Uri(outputFile)}\">Excel</a></b><br/>{tbl}", false);
    }

    private void SetBorder(IXLCell cell, XLColor borderColor, XLBorderStyleValues borderStyleValues = XLBorderStyleValues.Thin)
    {
        cell.Style.Border.BottomBorder = borderStyleValues;
        cell.Style.Border.BottomBorderColor = borderColor;
        cell.Style.Border.LeftBorder = borderStyleValues;
        cell.Style.Border.LeftBorderColor = borderColor;
        cell.Style.Border.RightBorder = borderStyleValues;
        cell.Style.Border.RightBorderColor = borderColor;
        cell.Style.Border.TopBorder = borderStyleValues;
        cell.Style.Border.TopBorderColor = borderColor;
    }
}