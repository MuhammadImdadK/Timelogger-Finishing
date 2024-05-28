// <copyright file="SmtpConfiguration.cs" company="Dylan Eddies">
// Coyright (C) Dylan EddiesAll Rights reserved.
// </copyright>

namespace ReportGenerator.EmailReporting.Configuration;

/// <summary>
/// The smtp configuration class. for reading configuration from appsettings.
/// </summary>
public class SmtpConfiguration
{
    /// <summary>
    /// Gets or sets the SMTP server host.
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP server port.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether SSL is enabled or not.
    /// </summary>
    public bool EnableSsl { get; set; }

    /// <summary>
    /// Gets or sets the email account to send the email with.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password of the account.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the recipients of the email.
    /// </summary>
    public string Recipients { get; set; } = string.Empty;
    public string SendAs { get; set; } = string.Empty;
}
