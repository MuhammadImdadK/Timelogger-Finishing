// <copyright file="EmailOutput.cs" company="Dylan Eddies">
// Coyright (C) Dylan EddiesAll Rights reserved.
// </copyright>

namespace ReportGenerator.EmailReporting.Output;

using System.Net;
using System.Net.Mail;
using Configuration;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Email output class.
/// </summary>
public class EmailOutput : IOutput
{
    private readonly IConfigurationRoot configurationRoot;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailOutput"/> class.
    /// </summary>
    /// <param name="configurationRoot">the configuration.</param>
    public EmailOutput(IConfigurationRoot configurationRoot)
    {
        this.configurationRoot = configurationRoot;
    }

    /// <inheritdoc/>
    public void ProduceOutput(string location, string contents, params string[] parameters)
    {
        var smtpConfig = this.configurationRoot.GetSection("SmtpConfiguration").Get<SmtpConfiguration>();
        var addresses = location.Split(";");

        var smtpClient = new SmtpClient(smtpConfig?.Host ?? throw new InvalidOperationException("SmtpConfiguration should not be null."))
        {
            Port = smtpConfig.Port,
            Credentials = new NetworkCredential(smtpConfig.Username, smtpConfig.Password),
            EnableSsl = smtpConfig.EnableSsl,
        };

        var message = new MailMessage()
        {
            From = new MailAddress(smtpConfig.SendAs),
            Subject = parameters[0],
            Body = contents,
            IsBodyHtml = true,
        };

        foreach (var address in addresses)
        {
            if (!string.IsNullOrWhiteSpace(address?.Trim()))
            {
                message.To.Add(address.Trim());
            }
        }

        try
        {
            smtpClient.Send(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message.ToString());
        }
    }
}