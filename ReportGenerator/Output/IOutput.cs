// <copyright file="IOutput.cs" company="Dylan Eddies">
// Coyright (C) Dylan EddiesAll Rights reserved.
// </copyright>

namespace ReportGenerator.EmailReporting.Output;

/// <summary>
/// The output interface.
/// </summary>
public interface IOutput
{
    /// <summary>
    /// Send the report to the specified output type.
    /// </summary>
    /// <param name="location">where the report should be stored.</param>
    /// <param name="contents">the report contents.</param>
    /// <param name="parameters">any additional parameters required.</param>
    void ProduceOutput(string location, string contents, params string[] parameters);
}
