// <copyright file="ConsoleContainer.cs" company="Dylan Eddies">
// Coyright (C) Dylan EddiesAll Rights reserved.
// </copyright>

namespace ReportGenerator.EmailReporting;

using Autofac;

/// <summary>
/// The console container.
/// </summary>
public class ConsoleContainer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleContainer"/> class.
    /// </summary>
    /// <param name="container">The Autofac container.</param>
    public ConsoleContainer(IContainer container)
    {
        this.Container = container;
    }

    /// <summary>
    /// Gets or sets the container.
    /// </summary>
    public IContainer Container { get; set; }
}
