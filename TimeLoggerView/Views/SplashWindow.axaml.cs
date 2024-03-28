using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Model.Database;
using Model.Interface;
using System.Threading.Tasks;
using TimeLoggerView;

namespace Pec.ProjectManagement.Ui.Views
{
    public partial class SplashWindow : Window
    {
        public SplashWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.StatusText.Text = "Checking database";
            Task.Run(() =>
            {
                IRepository repository = App.Container.GetService<IRepository>();
                IConfigurationRoot config = App.Container.GetRequiredService<IConfigurationRoot>();

                repository.RunMigrations(config);

                Dispatcher.UIThread.Invoke(() => this.Close());
            });
        }
    }
}
