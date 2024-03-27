using Avalonia.Collections;
using Avalonia.Media;
using Avalonia.Threading;
using Common.Enums;
using DynamicData;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.Extensions.DependencyInjection;
using Model.ModelSql;
using ReactiveUI;
using Service.Interface;
using SukiUI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TimeLoggerView.Views;
using TimeLoggerView.Views.Projects;
using TimeLoggerView.Views.Timesheet;

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

public class ProjectManagementViewModel : ModuleViewModel
{
    private const int ErfOffset = 10001;
    private bool canModifyBudget;
    private bool isEditing;
    private bool isAddingAttachment;

    private Model.ModelSql.Drawing currentAttachment = new();
    private Project? priorState;
    private Project currentProject = new Project();
    private User? modifiedByUser;
    private User? createdByUser;
    private readonly IProjectService projectService;
    private readonly IAttachmentService attachmentService;
    private readonly IUserService userService;

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
    public Model.ModelSql.Drawing CurrentAttachment { get => this.currentAttachment; set => this.RaiseAndSetIfChanged(ref this.currentAttachment, value); }

    public ICommand CreateProjectCommand { get; }
    public ICommand SubmitCreateProjectCommand { get; }

    public ICommand BeginEditCommand { get; }
    public ICommand SaveEditCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand SubmitBudgetCommand { get; }
    public ICommand MarkProjectAsClosedCommand { get; }
    public ICommand SubmitMarkProjectAsClosedCommand { get; }
    public ICommand CancelMarkProjectAsClosedCommand { get; }

    public ICommand AddAttachmentCommand { get; }
    public ICommand LogTimeCommand { get; }
    public ICommand SubmitAttachmentCommand { get; }
    public ICommand CancelAttachmentCommand { get; }
    public ICommand CloseDialogCommand { get; }

    public ICommand CloseProjectViewCommand { get; }

    public ObservableCollection<Project> Projects { get; set; } = new ObservableCollection<Project>();

    public ObservableCollection<Model.ModelSql.Drawing> CurrentDrawings { get; set; } = new();



    public bool IsTicketApproved => CurrentProject.IsActive &&  CurrentProject.ApprovalState == Common.Enums.RequestStatus.Accepted;
    public bool CanSubmit => CurrentProject.IsActive &&  CurrentProject.ManhourBudget > 0 && (
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
    public User? ModifiedByUser { get => modifiedByUser;  set => this.RaiseAndSetIfChanged(ref modifiedByUser, value); }
    public User? CreatedByUser { get => createdByUser;  set => this.RaiseAndSetIfChanged(ref createdByUser,value); }

    public ProjectManagementViewModel()
    {
        this.projectService = (IProjectService)App.Container.GetService(typeof(IProjectService));
        this.attachmentService = (IAttachmentService)App.Container.GetService(typeof(IAttachmentService));
        this.userService = (IUserService)App.Container.GetService(typeof(IUserService));
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
        this.CloseDialogCommand = ReactiveCommand.Create(CloseDialog);
        this.SubmitMarkProjectAsClosedCommand = ReactiveCommand.Create(SubmitMarkProjectAsClosed);
        this.CancelMarkProjectAsClosedCommand = ReactiveCommand.Create(CancelMarkProjectAsClosed);
        this.CreateProjectCommand = ReactiveCommand.Create(CreateProject);
        this.SubmitCreateProjectCommand = ReactiveCommand.Create(SubmitCreateProject);
        this.CloseProjectViewCommand = ReactiveCommand.Create(CloseProjectView);
        this.LoadProjects();
    }

    private void LoadProjects()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            this.IsBusy = true;
            this.BusyText = "Loading Projects";
        });
        Task.Run(() =>
        {
            var projects = this.projectService.GetProjects();
            Dispatcher.UIThread.Invoke(() =>
            {
                this.IsBusy = false;
                lock (this.Projects)
                {
                    this.Projects.Clear();
                    this.Projects.AddRange(projects);
                }
            });
        });
    }

    private void ViewProject(Project? project = null)
    {
        this.IsBusy = true;
        this.BusyText = "Loading Project Details";

        if (project != null)
        {
            this.CurrentProject = project;
        }
        else
        {
            project = this.CurrentProject;
        }

        Dispatcher.UIThread.Invoke(() =>
        {
            var projectView = new ProjectView();
            projectView.DataContext = this;
            this.CurrentDrawings.Clear();
            SukiHost.ShowDialog(App.WorkspaceInstance, projectView, allowBackgroundClose: false);
        });

        Task.Run(() =>
        {
            if (CurrentProject.Id != null)
            {
                var drawings = this.attachmentService.GetDrawingsByProjectId(CurrentProject.Id ?? 0);
                this.CurrentDrawings.AddRange(drawings);
            }
            if (CurrentProject.ModifiedBy != null)
            {
                this.ModifiedByUser = this.userService.GetUserById(CurrentProject.ModifiedBy ?? 0);
            }
            if (CurrentProject.CreatedBy != null)
            {
                this.CreatedByUser = this.userService.GetUserById(CurrentProject.CreatedBy ?? 0);
            }
            this.IsBusy = false;
            this.CurrentProject = project;
        });
    }

    private void BeginEdit()
    {
        this.IsEditing = true;
        this.PriorState = (Project)CurrentProject.Clone();
    }

    private void SaveEdit()
    {
        Task.Run(() =>
        {
            this.IsBusy = true;
            this.BusyText = "Saving changes to project";

            this.CurrentProject.Drawings?.Clear();
            this.CurrentProject.ModifiedByUser = null;
            this.CurrentProject.CreatedByUser = null;
            this.CurrentProject.ModifiedBy = App.CurrentUser.Id;
            this.CurrentProject.Modified = DateTime.UtcNow;

            var result = this.projectService.UpdateProject(this.CurrentProject);

            if (result)
            {
                this.IsEditing = false;
                ViewProject();
            }
            else
            {
                this.IsBusy = false;
                this.CreateToast("Failed to update project", "Failed to update the project. Please try again later");
            }
        });
    }
    private void CancelEdit()
    {
        this.IsEditing = false;

        if (this.PriorState != null)
        {
            this.CurrentProject = this.PriorState;
        }
    }
    private void SubmitBudget()
    {
        this.IsBusy = true;
        this.BusyText = "Submitting budget proposal";

    }
    private void MarkProjectAsClosed()
    {
        var view = new ConfirmMarkCloseProjectView()
        {
            DataContext = this
        };
        SukiHost.ShowDialog(App.WorkspaceInstance, view, allowBackgroundClose: false);
    }
    private void SubmitMarkProjectAsClosed()
    {
        Task.Run(() =>
        {
            this.CloseDialog();
            this.ViewProject();
            this.IsBusy = true;
            this.BusyText = "Marking project as closed";
            this.CurrentProject.Drawings?.Clear();
            this.CurrentProject.ModifiedByUser = null;
            this.CurrentProject.CreatedByUser = null;
            this.CurrentProject.ModifiedBy = App.CurrentUser.Id;
            this.CurrentProject.Modified = DateTime.UtcNow;
            this.CurrentProject.IsActive = false;

            var result = this.projectService.UpdateProject(this.CurrentProject);

            if (result)
            {
                this.IsEditing = false;
                this.CreateToast("Project closed", "The project was successfully marked as closed");

                LoadProjects();
                ViewProject();
            }
            else
            {
                this.IsBusy = false;
                this.CreateToast("Failed to update project", "Failed to update the project. Please try again later");
            }
        });
    }
    private void CancelMarkProjectAsClosed()
    {
        this.CloseDialog();
        this.ViewProject();
    }
    private void AddAttachment()
    {
        this.IsAddingAttachment = true;
        this.CurrentAttachment = new();
    }
    private void SubmitAttachment()
    {
        Task.Run(() =>
        {
            this.IsBusy = true;
            this.BusyText = "Adding Drawing";

            this.CurrentAttachment.ModifiedByUser = null;
            this.CurrentAttachment.CreatedByUser = null;
            this.CurrentAttachment.CreatedBy = App.CurrentUser.Id;
            this.CurrentAttachment.Created = DateTime.UtcNow;
            this.CurrentAttachment.ModifiedBy = App.CurrentUser.Id;
            this.CurrentAttachment.Modified = DateTime.UtcNow;

            var result = this.attachmentService.InsertAttachment(this.CurrentAttachment);

            if (result)
            {
                this.IsEditing = false;
                this.IsAddingAttachment = false;
                this.CurrentAttachment = new();
                ViewProject();
            }
            else
            {
                this.IsBusy = false;
                this.CreateToast("Failed to update project", "Failed to update the project. Please try again later");
            }
        });
    }
    private void CancelAttachment()
    {
        this.IsAddingAttachment = false;
        this.CurrentAttachment = new();
    }
    private void LogTime() {
        if (TimeLoggerWindow.Instance == null)
        {
            var window = new TimeLoggerWindow();
            window.Show();
        }
    }

    private void CreateProject()
    {
        this.CurrentProject = new();
        var view = new CreateProjectView()
        {
            DataContext = this
        };
        SukiHost.ShowDialog(App.WorkspaceInstance, view, allowBackgroundClose: false);
    }
    private void SubmitCreateProject()
    {
        Task.Run(() =>
        {
            this.IsBusy = true;
            this.BusyText = "Creating Project";

            CurrentProject.Created = DateTime.UtcNow;
            CurrentProject.CreatedBy = App.CurrentUser.Id;
            CurrentProject.Modified = DateTime.UtcNow;
            CurrentProject.ModifiedBy = App.CurrentUser.Id;
            CurrentProject.IsActive = true;
            CurrentProject.ApprovalState = RequestStatus.None;
            var erf = (this.projectService.GetLatestProjectId() ?? 0) + ErfOffset;
            CurrentProject.ERFNumber = erf.ToString();
            var result = this.projectService.InsertProject(CurrentProject);

            Thread.Sleep(1000);

            if (result)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    this.CloseDialog();
                    this.CreateToast("Successfully created project", $"Project '{CurrentProject.ProjectName}' was created");
                    this.LoadProjects();
                });
            }
            else
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    this.CreateToast("Project Creation Failed", "Project Creation ran into an error, please try again.");
                });
            }
        });
    }

    public void CloseProjectView()
    {
        this.CloseDialog();
        Task.Run(() =>
        {
            Thread.Sleep(500);
            this.LoadProjects();
        });
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

