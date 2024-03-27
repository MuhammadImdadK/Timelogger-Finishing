using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using Model.Interface;
using Model.Repository;
using Model.Database;
using Service.Interface;
using Service.Service;
using SukiUI;
using System;
using TimeLoggerView.ViewModels;
using TimeLoggerView.Views;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Model.ModelSql;

namespace TimeLoggerView;

public partial class App : Application
{
    public static IServiceProvider Container { get; private set; }

    public static MainWindow? WorkspaceInstance { get; set; }

    public static User CurrentUser { get; set; } = new();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        var assembly = Assembly.GetExecutingAssembly();
        var location = assembly.Location;
        location = location.Replace($"{assembly.GetName().Name}.dll", string.Empty);
        var configuration = new ConfigurationBuilder()
            .SetBasePath(location)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        var connectionStrings = configuration.GetSection("ConnectionStrings").Get<Dictionary<string, string>>();
        
        if(connectionStrings == null || (!connectionStrings?.ContainsKey("TimeLoggerDatabase") ?? true))
        {
            throw new InvalidOperationException("Configuration does not contain the time logger database.");
        }

        var dbString = connectionStrings["TimeLoggerDatabase"];

        services.AddRepository(dbString);
        
        services.AddSingleton<IConfigurationRoot>(configuration);
        services.AddTransient<IRepository, EntityFrameworkRepository>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITimeLogService, TimeLogService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IAttachmentService, AttachmentService>();
        services.AddScoped<IRequestService, RequestService>();
        
        Container = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var vm = new LoginViewModel((IAuthenticationService)Container.GetService(typeof(IAuthenticationService)));
            var loginPage = new LoginWindow()
            {
                DataContext = vm
            };
            vm.OnLoginSuccessful += OnLoginSuccessful;
            desktop.MainWindow = loginPage; 
            SukiTheme.GetInstance().ChangeBaseTheme(ThemeVariant.Dark);
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Triggers once the login is successful; hide the login screen and open the main window.
    /// </summary>
    /// <param name="sender">the login view model</param>
    /// <param name="user">the user that was logged into.</param>
    private void OnLoginSuccessful(object sender, User user)
    {
        if(ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if(!user.IsActive)
            {
                return;
            }
            desktop.MainWindow?.Hide();
            // The following must be done before the ViewModel is created
            WorkspaceInstance = new MainWindow();
            CurrentUser = user;

            // now we can hook up the model safely
            var vm = new MainViewModel();
            WorkspaceInstance.DataContext = vm;
            WorkspaceInstance.Show();
            vm.OnSignout += OnSignout;
        }
    }

    private void OnSignout(object sender, EventArgs e)
    {
        if(WorkspaceInstance!.DataContext is MainViewModel vm)
        {
            vm.OnSignout -= OnSignout;
            WorkspaceInstance.Close();

            if(ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow?.Show();
            }
        }
    }
}
