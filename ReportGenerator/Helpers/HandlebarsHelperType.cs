// <copyright file="HandlebarsHelperType.cs" company="Dylan Eddies">
// Coyright (C) Dylan EddiesAll Rights reserved.
// </copyright>

namespace ReportGenerator.Helpers;

/// <summary>
/// The handlebars helper type.
/// </summary>
public enum HandlebarsHelperType
{
    /// <summary>
    /// The default option.
    /// </summary>.
    Helper,

    /// <summary>
    /// Used when a helper can contain additional text.
    /// </summary>
    Block,
}
