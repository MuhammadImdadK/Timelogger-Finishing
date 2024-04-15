using Avalonia.Collections;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.Extensions.DependencyInjection;
using Model.ModelSql;
using ReactiveUI;
using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Windows.Input;
using TimeLoggerView.Views.Timesheet;

namespace TimeLoggerView.ViewModels;

public class MainViewModel : ModuleViewModel
{
    private UserManagementViewModel userManagement;
    private ProjectManagementViewModel projectManagement;
    private RequestsViewModel requestsViewModel;
    private ReportsViewModel reportsViewModel;

    public bool ShouldQuit { get; private set; } = true;
    public User CurrentUser { get; set; }

    public UserManagementViewModel UserManagement { get => this.userManagement; set => this.RaiseAndSetIfChanged(ref this.userManagement, value); }
    public ProjectManagementViewModel ProjectManagement { get => this.projectManagement; set => this.RaiseAndSetIfChanged(ref this.projectManagement, value); }

    public static TimesheetViewModel TimesheetManagement { get; set; }
    public TimesheetViewModel TimesheetModel { get => timesheetModel; set => this.RaiseAndSetIfChanged(ref timesheetModel, value); }
    public RequestsViewModel RequestsViewModel { get => this.requestsViewModel; set => this.RaiseAndSetIfChanged(ref this.requestsViewModel, value); }
    public ReportsViewModel ReportsViewModel { get => this.reportsViewModel; set => this.RaiseAndSetIfChanged(ref this.reportsViewModel, value); }
    
    public EventHandler OnSignout;
    private TimesheetViewModel timesheetModel;
    private bool isAdminUser;

    public ICommand Signout { get; }
    public bool IsAdminUser { get => isAdminUser; set => isAdminUser = value; }

    public MainViewModel()
    {
        this.CurrentUser = App.CurrentUser;
        this.ProjectManagement = new ProjectManagementViewModel();
        this.UserManagement = new UserManagementViewModel();
        this.RequestsViewModel = new RequestsViewModel();
        TimesheetManagement = new TimesheetViewModel();
        this.TimesheetModel = new TimesheetViewModel();
        this.ReportsViewModel = new ReportsViewModel();
        Signout = ReactiveCommand.Create(PerformSignout);
        this.IsAdminUser = this.CurrentUser.RoleID == 1;
    }

    public void PerformSignout()
    {
        if (TimeLoggerWindow.Instance == null)
        {
            this.ShouldQuit = false;
            OnSignout?.Invoke(this, EventArgs.Empty);
        } else
        {
            this.CreateToast("You have unsaved changes", "The time logger is still open. Please review changes before signing out.");
        }
    }
}
