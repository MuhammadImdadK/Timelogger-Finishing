using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Model.Database;
using Model.Interface;
using System;
using System.Threading.Tasks;
using TimeLoggerView;

namespace Pec.ProjectManagement.Ui.Views
{
    public partial class SplashWindow : Window
    {
        private readonly ILogger<SplashWindow> logger;
        public SplashWindow()
        {
            InitializeComponent();

            IRepository repository = App.Container.GetService<IRepository>();

            IConfigurationRoot config = App.Container.GetRequiredService<IConfigurationRoot>();
            ILogger<DbContext> dbLogger = App.Container.GetService<ILogger<DbContext>>();
            repository.RunMigrations(config, dbLogger);
        }

        private void Window_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.StatusText.Text = "Checking database";
            Task.Run(() =>
            {
                try
                {
                    var logger = (ILogger<SplashWindow>)App.Container.GetService<ILogger<SplashWindow>>();

                    logger.LogInformation("Attempting to run database migrations");
                    IRepository repository = App.Container.GetService<IRepository>();
                    IConfigurationRoot config = App.Container.GetRequiredService<IConfigurationRoot>();
                    ILogger<DbContext> dbLogger = App.Container.GetService<ILogger<DbContext>>();

                    repository.RunMigrations(config, dbLogger);
                    logger.LogInformation("Migrations were successful");
                }
                catch (Exception ex)
                {
                    this.logger.LogCritical("Error running migrations: {message} {exception}", ex.Message, ex);
                }
                Dispatcher.UIThread.Invoke(() => this.Close());
            });
        }
    }
}
