// <copyright file="Templates.cs" company="Dylan Eddies">
// Coyright (C) Dylan EddiesAll Rights reserved.
// </copyright>

namespace ReportGenerator.EmailReporting;

using System.Reflection;
using Autofac;

/// <summary>
/// Manages available templates for the template generator.
/// </summary>
public class Templates : ITemplates
{
    private readonly string[] templates;
    private readonly string templateLocation;

    /// <summary>
    /// Initializes a new instance of the <see cref="Templates"/> class.
    /// </summary>
    public Templates()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var location = assembly.Location;
        location = location.Replace($"{assembly.GetName().Name}.dll", string.Empty);
        this.templateLocation = Path.Join(location, "Templates");

        this.templates = Directory.GetFiles(this.templateLocation).Where(itm => itm.ToLower().EndsWith(".hbs")).ToArray();
    }

    /// <inheritdoc/>
    public async Task<string> GetTemplateContent(string templateName)
    {
        return await File.ReadAllTextAsync(Path.Join(this.templateLocation, $"{templateName}.hbs"));
    }

    /// <inheritdoc/>
    public IEnumerable<string> ListTemplates()
    {
        var items = new List<string>();
        foreach (var template in this.templates)
        {
            items.Add(
                template.Replace(this.templateLocation, string.Empty)
                    .Replace(".hbs", string.Empty)
                    .Replace(Path.PathSeparator.ToString(), string.Empty)
                    .Replace("\\", string.Empty));
        }

        return items;
    }
}
