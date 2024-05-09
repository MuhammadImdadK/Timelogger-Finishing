// <copyright file="GitLabIntegration.cs" company="Dylan Eddies">
// Coyright (C) Dylan EddiesAll Rights reserved.
// </copyright>

namespace ReportGenerator.EmailReporting.Configuration;

/// <summary>
/// The gitlab integration config class.
/// </summary>
public class GitLabIntegration
{
    /// <summary>
    /// Gets or sets the branch that email output should be allowed on.
    /// </summary>
    public string EmailOutputBranch { get; set; } = string.Empty;
}