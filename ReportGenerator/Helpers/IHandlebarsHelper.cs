// <copyright file="IHandlebarsHelper.cs" company="Dylan Eddies">
// Coyright (C) Dylan EddiesAll Rights reserved.
// </copyright>

namespace ReportGenerator.Helpers;

using HandlebarsDotNet;

/// <summary>
/// The handlebars helper interface.
/// </summary>
public interface IHandlebarsHelper
{
    /// <summary>
    /// Executes the helper class.
    /// </summary>
    /// <param name="writer">the handlebars writer.</param>
    /// <param name="context">the data contet.</param>
    /// <param name="parameters">the parameters passed to the helper.</param>
    public void Execute(EncodedTextWriter writer, Context context, Arguments parameters);
}
