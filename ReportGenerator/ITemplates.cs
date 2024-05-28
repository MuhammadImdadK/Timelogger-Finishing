// <copyright file="ITemplates.cs" company="Dylan Eddies">
// Coyright (C) Dylan EddiesAll Rights reserved.
// </copyright>

namespace ReportGenerator.EmailReporting;

/// <summary>
/// Interface for the template manager.
/// </summary>
public interface ITemplates
{
    /// <summary>
    /// Gets the text for the requested template.
    /// </summary>
    /// <param name="templateName">The requested template.</param>
    /// <returns>The template text.</returns>
    Task<string> GetTemplateContent(string templateName);

    /// <summary>
    /// Gets a list of available templates.
    /// </summary>
    /// <returns>The list of all available templates.</returns>
    IEnumerable<string> ListTemplates();
}