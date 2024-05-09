// <copyright file="FileOutput.cs" company="Dylan Eddies">
// Coyright (C) Dylan EddiesAll Rights reserved.
// </copyright>

namespace ReportGenerator.EmailReporting.Output;

/// <summary>
/// File Output class.
/// </summary>
public class FileOutput : IOutput
{
    /// <inheritdoc/>
    public void ProduceOutput(string location, string contents, params string[] parameters)
    {
        var locationInfo = new FileInfo(location);
        Directory.CreateDirectory(locationInfo.DirectoryName ?? throw new InvalidOperationException("Directory name should not be null!"));
        File.WriteAllText(locationInfo.FullName, contents);
    }
}
