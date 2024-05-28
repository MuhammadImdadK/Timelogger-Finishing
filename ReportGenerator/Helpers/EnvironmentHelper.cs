// <copyright file="EnvironmentHelper.cs" company="Dylan Eddies">
// Coyright (C) Dylan EddiesAll Rights reserved.
// </copyright>

namespace ReportGenerator.Helpers;

using Autofac;
using HandlebarsDotNet;

/// <summary>
/// Reads an environment variable.
/// </summary>
[HandleBarsHelper(Name = "env")]
public class EnvironmentHelper : ConsoleHandlebarsHelper
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EnvironmentHelper"/> class.
    /// </summary>
    /// <param name="container">The autofac container.</param>
    public EnvironmentHelper(IContainer container)
        : base(container)
    {
    }

    /// <inheritdoc/>
    public override void Execute(EncodedTextWriter writer, Context context, Arguments parameters)
    {
        var envVar = parameters[0].ToString()?.ToUpper()
            ?? throw new InvalidOperationException($"{nameof(EnvironmentHelper)}: Environment variable name not passed to the helper");

        writer.WriteSafeString(Environment.GetEnvironmentVariable(envVar));
    }
}
