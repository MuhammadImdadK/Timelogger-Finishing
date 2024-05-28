// <copyright file="IHandlebarsBlockHelper.cs" company="Dylan Eddies">
// Coyright (C) Dylan EddiesAll Rights reserved.
// </copyright>

namespace ReportGenerator.Helpers;

using HandlebarsDotNet;

/// <summary>
/// The Handlebars Block Helper Interface.
/// </summary>
public interface IHandlebarsBlockHelper
{
    /// <summary>
    /// Executes the helper class.
    /// </summary>
    /// <param name="writer">the handlebars writer.</param>
    /// <param name="options">the options passed through from the template.</param>
    /// <param name="context">the data contet.</param>
    /// <param name="parameters">the parameters passed to the helper.</param>
    public void Execute(EncodedTextWriter writer, BlockHelperOptions options, Context context, Arguments parameters);
}
