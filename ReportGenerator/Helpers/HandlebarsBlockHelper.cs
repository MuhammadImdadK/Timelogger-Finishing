// <copyright file="HandlebarsBlockHelper.cs" company="Dylan Eddies">
// Coyright (C) Dylan EddiesAll Rights reserved.
// </copyright>

namespace ReportGenerator.Helpers;

using Autofac;
using HandlebarsDotNet;

/// <summary>
/// The handle bars block helper class.
/// </summary>
public abstract class HandlebarsBlockHelper : IHandlebarsBlockHelper
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HandlebarsBlockHelper"/> class.
    /// </summary>
    /// <param name="handlebars">The handlebars compiler.</param>
    /// <param name="container">The Autofac Container.</param>
    public HandlebarsBlockHelper(IHandlebars handlebars, IContainer container)
    {
        this.Handlebars = handlebars;
        this.Container = container;
    }

    /// <summary>
    /// Gets the handlebars compiler.
    /// </summary>
    public IHandlebars Handlebars { get; }

    /// <summary>
    /// Gets the autofac container.
    /// </summary>
    protected IContainer Container { get; }

    /// <inheritdoc/>
    public abstract void Execute(EncodedTextWriter writer, BlockHelperOptions options, Context context, Arguments parameters);
}
