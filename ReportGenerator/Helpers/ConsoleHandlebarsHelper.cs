// <copyright file="ConsoleHandlebarsHelper.cs" company="Dylan Eddies">
// Coyright (C) Dylan EddiesAll Rights reserved.
// </copyright>

namespace ReportGenerator.Helpers;

using Autofac;
using HandlebarsDotNet;

/// <summary>
/// The Handlebars Helper class.
/// </summary>
public abstract class ConsoleHandlebarsHelper : IHandlebarsHelper
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleHandlebarsHelper"/> class.
    /// </summary>
    /// <param name="container">The container.</param>
    public ConsoleHandlebarsHelper(IContainer container)
    {
        this.Container = container;
    }

    /// <summary>
    /// Gets the autofac container.
    /// </summary>
    protected IContainer Container { get; }

    /// <inheritdoc/>
    public abstract void Execute(EncodedTextWriter writer, Context context, Arguments parameters);
}
