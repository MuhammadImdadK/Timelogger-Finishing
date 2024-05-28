// <copyright file="HandleBarsHelperAttribute.cs" company="Dylan Eddies">
// Coyright (C) Dylan EddiesAll Rights reserved.
// </copyright>

namespace ReportGenerator.Helpers;

/// <summary>
/// Helpers MUST be decorated with this class.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class HandleBarsHelperAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the name of the Helper.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of helper.
    /// </summary>
    public HandlebarsHelperType Type { get; set; } = HandlebarsHelperType.Helper;
}
