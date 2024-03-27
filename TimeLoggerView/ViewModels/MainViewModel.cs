using Avalonia.Collections;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.Extensions.DependencyInjection;
using Model.ModelSql;
using ReactiveUI;
using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Windows.Input;

namespace TimeLoggerView.ViewModels;

public class MainViewModel : ViewModelBase
{
    private UserManagementViewModel userManagement;
    private ProjectManagementViewModel projectManagement;
    private TimesheetViewModel timesheetManagement;

    public bool ShouldQuit { get; private set; } = true;
    public User CurrentUser { get; set; }

    public UserManagementViewModel UserManagement { get => this.userManagement; set => this.RaiseAndSetIfChanged(ref this.userManagement, value); }
    public ProjectManagementViewModel ProjectManagement { get => this.projectManagement; set => this.RaiseAndSetIfChanged(ref this.projectManagement, value); }

    public TimesheetViewModel TimesheetManagement { get => this.timesheetManagement; set => this.RaiseAndSetIfChanged(ref this.timesheetManagement, value); }

    public EventHandler OnSignout;

    public ICommand Signout { get; }

    public MainViewModel()
    {
        this.CurrentUser = App.CurrentUser;
        this.ProjectManagement = new ProjectManagementViewModel();
        this.userManagement = new UserManagementViewModel();
        Signout = ReactiveCommand.Create(PerformSignout);

    }

    public void PerformSignout()
    {
        this.ShouldQuit = false;
        OnSignout?.Invoke(this, EventArgs.Empty);
    }
}

public class TimesheetViewModel : ViewModelBase
{

}

public class ReportsViewModel : ViewModelBase
{

}

public class RequestsViewModel : ViewModelBase
{

}

