// <copyright file="BpgRunInfo.cs" company="Dylan Eddies">
// Coyright (C) Dylan EddiesAll Rights reserved.
// </copyright>

namespace ReportGenerator.EmailReporting.Helpers;

using System;
using System.Drawing;
using System.Net.WebSockets;
using System.Text.Json;
using System.Web;
using Autofac;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using HandlebarsDotNet;
using Model.ModelSql;
using ReportGenerator.Helpers;


[HandleBarsHelper(Name = "formShare")]
public class EmailShareHelper : ConsoleHandlebarsHelper
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BpgRunInfo"/> class.
    /// </summary>
    /// <param name="container">the container.</param>
    public EmailShareHelper(IContainer container)
    : base(container)
    {
    }

    /// <inheritdoc/>
    public override void Execute(EncodedTextWriter writer, Context context, Arguments parameters)
    {
        var doc = HandlebarsController.JsonDocument;

        //    writer.Write(
        //        $@"
        //        <table>
        //            <tr>
        //                <td>
        //                    <b>You have been invited to fill out form:</b>
        //                </td>
        //                <td>
        //                    {HttpUtility.HtmlEncode(share.FormId)} - {HttpUtility.HtmlEncode(share.FormName)}
        //                </td>
        //            </tr>
        //            <tr>
        //                <td>
        //                    <b>Please click the link  to access the form</b>
        //                </td>
        //                <td>
        //                    <a href=""{share.FormUrl}"">Access your form</a>
        //                </td>
        //            </tr>
        //        </table>
        //        <hr />
        //        ",
        //        false);
    }
}
