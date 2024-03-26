using Avalonia.Collections;
using Avalonia.Media;
using Common.Enums;
using DynamicData;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Model.ModelSql;
using ReactiveUI;
using Service.Interface;
using SukiUI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TimeLoggerView.Views;

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

    public MainViewModel(User currentUser)
    {
        this.CurrentUser = currentUser;
        this.ProjectManagement = new ProjectManagementViewModel();
        this.userManagement = new UserManagementViewModel(this.CurrentUser);
        Signout = ReactiveCommand.Create(PerformSignout);

    }

    public void PerformSignout()
    {
        this.ShouldQuit = false;
        OnSignout?.Invoke(this, EventArgs.Empty);
    }
}

public class ProjectManagementViewModel : ViewModelBase
{
    private bool canModifyBudget;
    private bool isEditing;
    private bool isAddingAttachment;
    private bool isBusy;
    private string busyText = string.Empty;
    private Project? priorState;
    private Project currentProject = new Project()
    {
        ProjectName = "Test Project Name",
        ERFNumber = "ERF-1234",
        Description = @" Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed aliquam, risus quis pulvinar euismod, felis nibh condimentum odio, eu auctor lectus nisi quis nunc. Mauris sagittis ac diam sit amet tempus. Phasellus magna est, venenatis quis venenatis ut, egestas sit amet tellus. Nam nibh velit, faucibus a sapien at, congue auctor arcu. Interdum et malesuada fames ac ante ipsum primis in faucibus. Curabitur a felis non massa imperdiet tempor sit amet ac mauris. Pellentesque aliquam feugiat interdum. Vivamus efficitur lacus in interdum convallis. Cras dignissim at purus quis condimentum. Proin ex ligula, pretium a aliquam sed, sodales quis magna. Nam non enim felis. Sed et risus dignissim, placerat tellus ut, placerat ipsum. Sed fringilla feugiat velit, at sagittis felis. Mauris eget enim metus. Donec lacus eros, rhoncus eu quam vel, placerat tempor magna. Aenean eget fermentum felis.

Aliquam condimentum maximus scelerisque. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Pellentesque eu augue sed mi convallis gravida vel nec orci. Pellentesque varius sed eros vitae ullamcorper. Proin ut euismod justo. Mauris sem felis, rhoncus nec turpis sed, imperdiet dictum felis. Vivamus quis mi mauris. Suspendisse imperdiet, justo eu finibus iaculis, nisi turpis lobortis tortor, vel maximus nisl quam efficitur massa. Quisque tincidunt arcu lacus, ut tempus tellus fermentum ac. Fusce gravida consequat risus, sed aliquet mi scelerisque nec. Nam dictum quam vel fermentum rutrum. Etiam sit amet purus nisi. Praesent vestibulum, libero in sagittis maximus, erat metus laoreet tortor, at mattis libero sapien quis turpis. Phasellus consequat sem eu fermentum placerat. Nam fermentum urna sit amet erat commodo, a pretium dolor convallis. Nullam ultricies, tellus nec suscipit congue, dolor ex fringilla odio, at euismod mi elit ac orci.

Nam volutpat gravida magna, id lobortis nisl auctor ut. Curabitur ultricies felis vel purus faucibus fermentum. Vivamus at erat iaculis, blandit augue a, blandit purus. Nullam at libero sapien. Mauris dapibus purus elit, vitae gravida ligula gravida nec. Fusce nisi purus, aliquam id leo in, mattis rutrum purus. Sed ut nibh pellentesque, laoreet diam ac, tincidunt turpis. ",
        Id = 1,
        CreatedBy = 1,
        Created = DateTime.Now,
        IsActive = true,
        ManhourBudget = 23,
        ApprovalState = RequestStatus.UpdateRequested,
        Drawings = new List<Model.ModelSql.Drawing>()
        {
            new()
            {
                Name = "Name",
                Description = "Description",
            }
        }
    };

    public bool CanModifyBudget { get => this.canModifyBudget; set => this.RaiseAndSetIfChanged(ref this.canModifyBudget, value); }
    public bool IsEditing
    {
        get => this.isEditing;
        set
        {
            this.RaiseAndSetIfChanged(ref this.isEditing, value);
            CanModifyBudget = value && (CurrentProject.ApprovalState == null
            || CurrentProject.ApprovalState == RequestStatus.None
            || CurrentProject.ApprovalState == RequestStatus.UpdateRequested);
        }
    }
    public bool IsAddingAttachment { get => isAddingAttachment; set => this.RaiseAndSetIfChanged(ref isAddingAttachment, value); }
    public Project? PriorState { get => this.priorState; set => this.RaiseAndSetIfChanged(ref this.priorState, value); }
    public Project CurrentProject { get => this.currentProject; set => this.RaiseAndSetIfChanged(ref this.currentProject, value); }

    public ICommand BeginEditCommand { get; }
    public ICommand SaveEditCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand SubmitBudgetCommand { get; }
    public ICommand MarkProjectAsClosedCommand { get; }
    public ICommand AddAttachmentCommand { get; }
    public ICommand LogTimeCommand { get; }
    public ICommand SubmitAttachmentCommand { get; }
    public ICommand CancelAttachmentCommand { get; }

    public ObservableCollection<Project> Projects { get; set; } = new ObservableCollection<Project>()
    {
        new Project()
        {
            ProjectName = "Test Project Name",
            ERFNumber = "ERF-1234",
            Description = "Description",
            Id = 1,
            CreatedBy = 1,
            Created = DateTime.Now,
            IsActive = true,
            Drawings = new List<Model.ModelSql.Drawing>()
            {
                new()
                {
                    Name = "Name",
                    Description = "Description"
                }
            }
        }
    };

    public ObservableCollection<Model.ModelSql.Drawing> CurrentDrawings { get; set; } = new();

    private bool IsBusy { get => this.isBusy; set => this.RaiseAndSetIfChanged(ref this.isBusy, value); }
    private string BusyText { get => this.busyText; set => this.RaiseAndSetIfChanged(ref this.busyText, value); }

    public bool IsTicketApproved => CurrentProject.ApprovalState == Common.Enums.RequestStatus.Accepted;
    public bool CanSubmit => CurrentProject.ManhourBudget > 0 && (
        CurrentProject.ApprovalState == null
        || CurrentProject.ApprovalState == RequestStatus.None
        || CurrentProject.ApprovalState == RequestStatus.UpdateRequested);

    public string ApprovalState => CurrentProject.ApprovalState switch
    {
        Common.Enums.RequestStatus.Open => "Awaiting Approval",
        Common.Enums.RequestStatus.Accepted => nameof(Common.Enums.RequestStatus.Accepted),
        Common.Enums.RequestStatus.Rejected => nameof(Common.Enums.RequestStatus.Rejected),
        RequestStatus.UpdateRequested => "Update Requested",
        _ => "Not Submitted"
    };

    public Brush ApprovalStateBrush => CurrentProject.ApprovalState switch
    {
        Common.Enums.RequestStatus.Open => new SolidColorBrush(Color.FromRgb(255, 252, 79), 1),
        Common.Enums.RequestStatus.Accepted => new SolidColorBrush(Color.FromRgb(58, 156, 34), 1),
        Common.Enums.RequestStatus.Rejected => new SolidColorBrush(Color.FromRgb(200, 0, 0), 1),
        RequestStatus.UpdateRequested => new SolidColorBrush(Color.FromRgb(222, 133, 55), 1),
        _ => new SolidColorBrush(Color.FromRgb(170, 170, 170), 1)
    };

    public ICommand ViewProjectCommand { get; }


    public ProjectManagementViewModel()
    {
        this.ViewProjectCommand = ReactiveCommand.Create<Project>(ViewProject);
        this.BeginEditCommand = ReactiveCommand.Create(BeginEdit);
        this.SaveEditCommand = ReactiveCommand.Create(SaveEdit);
        this.CancelEditCommand = ReactiveCommand.Create(CancelEdit);
        this.SubmitBudgetCommand = ReactiveCommand.Create(SubmitBudget);
        this.MarkProjectAsClosedCommand = ReactiveCommand.Create(MarkProjectAsClosed);
        this.AddAttachmentCommand = ReactiveCommand.Create(AddAttachment);
        this.SubmitAttachmentCommand = ReactiveCommand.Create(SubmitAttachment);
        this.CancelAttachmentCommand = ReactiveCommand.Create(CancelAttachment);
        this.LogTimeCommand = ReactiveCommand.Create(LogTime);
    }

    private void ViewProject(Project project)
    {
        //this.CurrentProject = project;
        var projectView = new ProjectView();
        projectView.DataContext = this;
        this.CurrentDrawings.Clear();
        if (CurrentProject.Drawings != null)
        {
            this.CurrentDrawings.AddRange(CurrentProject.Drawings);
        }
        SukiHost.ShowDialog(App.WorkspaceInstance, projectView, allowBackgroundClose: true);
    }

    private void BeginEdit()
    {
        this.IsEditing = true;
        this.PriorState = (Project)CurrentProject.Clone();
    }

    private void SaveEdit() {
        this.IsBusy = true;
        this.BusyText = "Saving changes to project";
    }
    private void CancelEdit()
    {
        this.IsEditing = false;

        if (this.PriorState != null)
        {
            this.CurrentProject = this.PriorState;
        }
    }
    private void SubmitBudget() {
        this.IsBusy = true;
        this.BusyText = "Submitting budget proposal";
    }
    private void MarkProjectAsClosed() { }
    private void AddAttachment()
    {
        this.IsAddingAttachment = true;
    }
    private void SubmitAttachment() {
        this.IsBusy = true;
        this.BusyText = "Adding Drawing";
    }
    private void CancelAttachment()
    {
        this.IsAddingAttachment = false;
    }
    private void LogTime() { }
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

