// <copyright file="GenerationOptions.cs" company="Dylan Eddies">
// Coyright (C) Dylan EddiesAll Rights reserved.
// </copyright>

namespace ReportGenerator.EmailReporting;
/// <summary>
/// The options for generating a report.
/// </summary>
public class GenerationOptions
{
    private FileInfo? input;
    private string templateName = string.Empty;

    /// <summary>
    /// Gets or sets the Data Input File.
    /// </summary>
    public FileInfo? Input
    {
        get => this.input;
        set
        {
            if (!value?.Exists ?? true)
            {
                throw new InvalidOperationException($"input:{value} does not exist or was not supplied");
            }

            if (!value?.ToString().ToLower().EndsWith(".json") ?? false)
            {
                throw new InvalidOperationException("Data file must be JSON");
            }

            this.input = value;
        }
    }

    /// <summary>
    /// Gets or sets the output file.
    /// </summary>
    public string Output { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the output type.
    /// </summary>
    public string OutputType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email subject.
    /// </summary>
    public string EmailSubject { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the report template file.
    /// </summary>
    public string TemplateName
    {
        get => this.templateName;
        set
        {
            this.templateName = value;
        }
    }

    public string JobName { get; set; }
}
