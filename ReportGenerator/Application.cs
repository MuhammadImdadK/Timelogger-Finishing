// <copyright file="Application.cs" company="Dylan Eddies">
// Coyright (C) Dylan EddiesAll Rights reserved.
// </copyright>

namespace ReportGenerator.EmailReporting;

using Configuration;
using Microsoft.Extensions.Configuration;

/// <summary>
/// The application.
/// </summary>
public class Application
{
    private readonly HandlebarsController controller;
    private readonly IConfigurationRoot configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="Application"/> class.
    /// </summary>
    /// <param name="controller">The handlebars controller.</param>
    /// <param name="configuration">The reporter configuration.</param>
    public Application(HandlebarsController controller, IConfigurationRoot configuration)
    {
        this.controller = controller;
        this.configuration = configuration;
    }

    public string Result { get; private set; }

    /// <summary>
    /// Runs the report generator.
    /// </summary>
    /// <param name="options">The options provided to the generator.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    public async Task Run(GenerationOptions options)
    {
        var gitlab = this.configuration.GetSection("GitLabIntegration").Get<GitLabIntegration>();
        if (options.OutputType == "email" && (Environment.GetEnvironmentVariable("CI_COMMIT_BRANCH")?.ToString().ToLower() ?? gitlab?.EmailOutputBranch.ToLower()) != gitlab?.EmailOutputBranch.ToLower())
        {
            return;
        }

        var result = await this.controller.CreateReport(options);
        this.Result = result;
    }
}
