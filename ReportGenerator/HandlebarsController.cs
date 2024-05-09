// <copyright file="HandlebarsController.cs" company="Dylan Eddies">
// Coyright (C) Dylan EddiesAll Rights reserved.
// </copyright>

namespace ReportGenerator.EmailReporting;

using System.Reflection;
using System.Text;
using System.Text.Json;
using Autofac;
using Helpers;
using Output;
using HandlebarsDotNet;
using ReportGenerator.Helpers;

/// <summary>
/// The Handlebars Controller.
/// </summary>
public class HandlebarsController
{
    private readonly IHandlebars handlebars;
    private readonly ITemplates templates;
    private readonly IContainer container;


    /// <summary>
    /// Initializes a new instance of the <see cref="HandlebarsController"/> class, and maps all available helpres to <see cref="IHandlebars"></see>/>.
    /// </summary>
    /// <param name="handlebars">The handlebars compiler.</param>
    /// <param name="templates">The template manager.</param>
    /// <param name="container">The container.</param>
    public HandlebarsController(IHandlebars handlebars, ITemplates templates, ConsoleContainer container)
    {
        this.handlebars = handlebars;
        this.templates = templates;
        this.container = container.Container;

        var types = Assembly.GetExecutingAssembly().GetTypes()
            .Where(itm => (itm.IsAssignableTo(typeof(IHandlebarsHelper)) || itm.IsAssignableTo(typeof(IHandlebarsBlockHelper))) && itm.IsClass && itm.IsPublic && !itm.IsAbstract);

        foreach (var type in types)
        {
            var attrib = Attribute.GetCustomAttribute(type, typeof(HandleBarsHelperAttribute)) as HandleBarsHelperAttribute;

            if (attrib == null)
            {
                throw new InvalidOperationException("Handlebars Controller: Attribute for helper should not be null! Something has gone horribly wrong");
            }

            switch (attrib.Type)
            {
                case HandlebarsHelperType.Helper:
                    var instance = Activator.CreateInstance(type, this.container);

                    if (instance is IHandlebarsHelper helper)
                    {
                        Console.WriteLine($"HelperRegister:{attrib.Name}");
                        this.handlebars.RegisterHelper(attrib.Name, (writer, context, parameters) =>
                        {
                            helper.Execute(writer, context, parameters);
                        });
                        continue;
                    }

                    break;
                case HandlebarsHelperType.Block:
                    var blockInstance = Activator.CreateInstance(type, this.handlebars, this.container);

                    if (blockInstance is IHandlebarsBlockHelper blockHelper)
                    {
                        Console.WriteLine($"BlockHelperRegister:{attrib.Name}");
                        this.handlebars.RegisterHelper(attrib.Name, (writer, options, context, parameters) =>
                        {
                            blockHelper.Execute(writer, options, context, parameters);
                        });
                    }

                    break;
            }
        }
    }

    /// <summary>
    /// Gets or sets the json document.
    /// </summary>
    public static JsonDocument? JsonDocument { get; set; }

    /// <summary>
    /// Generates a report based on the parameters provided.
    /// </summary>
    /// <param name="options">Options for generating a report.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    public async Task<string> CreateReport(GenerationOptions options)
    {
        if (!this.templates.ListTemplates().Any(itm => itm.ToLower() == options.TemplateName.ToLower()))
        {
            throw new InvalidOperationException($"{nameof(options.TemplateName)} Should not be null");
        }

        var templateContent = await this.templates.GetTemplateContent(options.TemplateName);

        var template = this.handlebars.Compile(templateContent);

        if (options.Input == null)
        {
            throw new InvalidOperationException($"{nameof(options.Input)} should not be null");
        }

        var data = JsonSerializer.Deserialize<JsonDocument>(File.ReadAllText(options.Input.FullName));
        JsonDocument = data;
        var result = template(data, options).Replace("\r", string.Empty).Replace("\n", string.Empty).Replace("  ", string.Empty);

        return result;
    }
}
