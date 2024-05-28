// <copyright file="IfConditionHelper.cs" company="Dylan Eddies">
// Coyright (C) Dylan EddiesAll Rights reserved.
// </copyright>

namespace ReportGenerator.Helpers;

using Autofac;
using HandlebarsDotNet;

/// <summary>
/// If (a{string} == b{string});.
/// </summary>
[HandleBarsHelper(Name = "ifCond", Type = HandlebarsHelperType.Block)]
public class IfConditionHelper : HandlebarsBlockHelper
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IfConditionHelper"/> class.
    /// </summary>
    /// <param name="handlebars">Handlebars compiler.</param>
    /// <param name="container">Container.</param>
    public IfConditionHelper(IHandlebars handlebars, IContainer container)
        : base(handlebars, container)
    {
    }

    /// <inheritdoc/>
    public override void Execute(EncodedTextWriter writer, BlockHelperOptions options, Context context, Arguments parameters)
    {
        if (parameters[0].ToString() == parameters[1].ToString())
        {
            options.Template(writer, (object)context);
        }
        else
        {
            options.Inverse(writer, (object)context);
        }
    }
}
