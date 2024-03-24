using Avalonia.Collections;
using Model.ModelSql;
using ReactiveUI;
using Service.Interface;
using System;
using System.Linq;
using System.Windows.Input;

namespace TimeLoggerView.ViewModels;

public class MainViewModel : ViewModelBase
{
    private UserManagementViewModel userManagement;
    public bool ShouldQuit { get; private set; } = true;
    public User CurrentUser { get; set; }

    public UserManagementViewModel UserManagement { get => this.userManagement; set => this.RaiseAndSetIfChanged(ref this.userManagement, value); }

    public EventHandler OnSignout;

    public ICommand Signout { get; }

    public MainViewModel()
    {
        this.userManagement = new UserManagementViewModel(CurrentUser);
        Signout = ReactiveCommand.Create(PerformSignout);

    }

    public void PerformSignout()
    {
        this.ShouldQuit = false;
        OnSignout?.Invoke(this, EventArgs.Empty);
    }
}
