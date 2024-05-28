// <copyright file="Program.cs" company="Dylan Eddies">
// Coyright (C) Dylan EddiesAll Rights reserved.
// </copyright>

namespace ReportGenerator.EmailReporting;

using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Reflection;
using Autofac;
using Helpers;
using Output;
using HandlebarsDotNet;
using Microsoft.Extensions.Configuration;

/// <summary>
/// The main program class.
/// </summary>
public class Program
{
    /// <summary>
    /// Creates an IoC container.
    /// </summary>
    /// <returns>The Container.</returns>
    public static IContainer CompositionRoot()
    {
        var container = CreateContainer();
        container.RegisterInstance<ConsoleContainer>(new ConsoleContainer(CreateContainer().Build())).SingleInstance();
        return container.Build();
    }

    /// <summary>
    /// The application's main entrypoint.
    /// </summary>
    /// <param name="args">Arguments passed through from the commandline.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public static async Task<int> Main(string[] args)
    {
        Option inputOption = new Option<FileInfo>(name: "--input", description: "Input file - data")
        {
            IsRequired = true,
        };

        inputOption.AddAlias("-i");

        Option outputOption = new Option<string>(name: "--output", description: "Output file - result")
        {
            IsRequired = true,
        };

        outputOption.AddAlias("-o");

        Option outputTypeOption = new Option<string>(name: "--output-type", description: "Output type")
        {
            IsRequired = true,
        };

        outputTypeOption.AddAlias("-y");
        outputTypeOption.SetDefaultValue("file");

        Option emailSubjectOption = new Option<string>(name: "--email-subject", description: "Output Email Subject")
        {
            IsRequired = false,
        };

        Option templateName = new Option<string>(name: "--template-name", description: "Template to generate");
        templateName.AddAlias("-t");
        templateName.IsRequired = true;

        var rootCommand = new RootCommand("Template Report Generator");

        var generateCommand = new Command("generate", "Generate a handlebars tempalte")
        {
            inputOption,
            outputOption,
            outputTypeOption,
            templateName,
            emailSubjectOption,
        };

        generateCommand.AddAlias("g");
        generateCommand.AddAlias("gen");
        generateCommand.Handler = CommandHandler.Create(CompositionRoot().Resolve<Application>().Run);
        rootCommand.AddCommand(generateCommand);

        var listCommand = new Command("list", "List all available templates");
        listCommand.AddAlias("ls");
        listCommand.Handler = CommandHandler.Create(() =>
        {
            foreach (var item in CompositionRoot().Resolve<ITemplates>().ListTemplates())
            {
                Console.WriteLine("Available Templates: ");
                Console.WriteLine($"  {item}");
            }
        });
        rootCommand.AddCommand(listCommand);
        return await rootCommand.InvokeAsync(args);
    }

    private static ContainerBuilder CreateContainer()
    {
        var container = new ContainerBuilder();
        container.RegisterInstance(RegisterConfig());
        container.RegisterInstance<Templates>(new Templates()).As<ITemplates>().SingleInstance();
        container.RegisterInstance(Handlebars.Create()).SingleInstance();
        container.RegisterType<FileOutput>().As<IOutput>().SingleInstance().WithParameter("type", "file");
        container.RegisterType<EmailOutput>().As<IOutput>().SingleInstance().WithParameter("type", "email");
        container.RegisterType<Templates>().As<ITemplates>().SingleInstance();
        container.RegisterType<HandlebarsController>().SingleInstance();
        container.RegisterType<Application>();
        return container;
    }

    private static IConfigurationRoot RegisterConfig()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var location = assembly.Location;
        location = location.Replace($"{assembly.GetName().Name}.dll", string.Empty);
        var builder = new ConfigurationBuilder()
            .SetBasePath(location)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        return builder.Build();
    }
}
