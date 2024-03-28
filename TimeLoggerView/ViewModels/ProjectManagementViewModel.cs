using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Common.Enums;
using DynamicData;
using Microsoft.EntityFrameworkCore.Storage;
using Model.ModelSql;
using ReactiveUI;
using Service.Interface;
using SukiUI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TimeLoggerView.Views;
using TimeLoggerView.Views.Projects;
using TimeLoggerView.Views.Timesheet;

namespace TimeLoggerView.ViewModels;

public class ProjectManagementViewModel : ModuleViewModel
{
    private const int ErfOffset = 10001;
    private bool canModifyBudget;
    private bool isEditing;
    private bool isAddingAttachment;
    private bool isAddingEndTime;

    private Model.ModelSql.Drawing currentAttachment = new();
    private Request currentRequest = new Request();
    private Project? priorState;
    private Project currentProject = new Project();
    private User? modifiedByUser;
    private User? createdByUser;
    private readonly IProjectService projectService;
    private readonly IAttachmentService attachmentService;
    private readonly IUserService userService;
    private readonly IRequestService requestService;

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
    public bool IsAddingEndTime { get => this.isAddingEndTime; set => this.RaiseAndSetIfChanged(ref isAddingEndTime, value); }
    public Project? PriorState { get => this.priorState; set => this.RaiseAndSetIfChanged(ref this.priorState, value); }
    public Project CurrentProject { get => this.currentProject; set => this.RaiseAndSetIfChanged(ref this.currentProject, value); }
    public Model.ModelSql.Drawing CurrentAttachment { get => this.currentAttachment; set => this.RaiseAndSetIfChanged(ref this.currentAttachment, value); }
    public Request CurrentRequest { get => this.currentRequest; set => this.RaiseAndSetIfChanged(ref this.currentRequest, value); }

    public ICommand CreateProjectCommand { get; }
    public ICommand SubmitCreateProjectCommand { get; }

    public ICommand BeginEditCommand { get; }
    public ICommand SaveEditCommand { get; }
    public ICommand CancelEditCommand { get; }

    public ICommand SubmitBudgetCommand { get; }
    public ICommand CancelBudgetCommand { get; }
    public ICommand PerformSubmitBudgetCommand { get; }

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

    public ObservableCollection<User> AvailableProjectEngineers { get; set; } = new();

    public bool IsTicketApproved => CurrentProject.IsActive && CurrentProject.ApprovalState == Common.Enums.RequestStatus.Accepted;
    public bool CanSubmit => CurrentProject.IsActive && CurrentProject.ManhourBudget > 0 && (
        CurrentProject.ApprovalState == null
        || CurrentProject.ApprovalState == RequestStatus.None
        || CurrentProject.ApprovalState == RequestStatus.UpdateRequested);
    public bool RequestValid => this.CurrentRequest.PlanningEngineer != null && this.CurrentRequest.StartTimeOffset != null;

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
    public User? ModifiedByUser { get => modifiedByUser; set => this.RaiseAndSetIfChanged(ref modifiedByUser, value); }
    public User? CreatedByUser { get => createdByUser; set => this.RaiseAndSetIfChanged(ref createdByUser, value); }

    public ProjectManagementViewModel()
    {
        this.projectService = (IProjectService)App.Container.GetService(typeof(IProjectService));
        this.attachmentService = (IAttachmentService)App.Container.GetService(typeof(IAttachmentService));
        this.userService = (IUserService)App.Container.GetService(typeof(IUserService));
        this.requestService = (IRequestService)App.Container.GetService(typeof(IRequestService));
        this.ViewProjectCommand = ReactiveCommand.Create<Project>(ViewProject);
        this.BeginEditCommand = ReactiveCommand.Create(BeginEdit);
        this.SaveEditCommand = ReactiveCommand.Create(SaveEdit);
        this.CancelEditCommand = ReactiveCommand.Create(CancelEdit);
        this.SubmitBudgetCommand = ReactiveCommand.Create(SubmitBudget);
        this.CancelBudgetCommand = ReactiveCommand.Create(CancelSubmitBudget);
        this.PerformSubmitBudgetCommand = ReactiveCommand.Create(PerformSubmitBudget);
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
        var availableProjectEngineers = this.userService.GetUsers().Where(itm => itm.RoleID == 2);
        Dispatcher.UIThread.Invoke(() =>
        {
            this.AvailableProjectEngineers.Clear();
            this.AvailableProjectEngineers.AddRange(availableProjectEngineers);
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
                Dispatcher.UIThread.Invoke(() =>
                {
                    this.CurrentDrawings.AddRange(drawings);
                });
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
        this.CurrentRequest = new();
        var view = new ProjectRequestView()
        {
            DataContext = this
        };
        SukiHost.ShowDialog(App.WorkspaceInstance, view, allowBackgroundClose: false);
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
            this.CurrentAttachment.ProjectId = this.CurrentProject.Id ?? 0;
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

    private void LogTime()
    {
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
    public void CancelSubmitBudget()
    {
        this.CloseDialog();
        this.ViewProject();
    }
    public void PerformSubmitBudget()
    {
        Task.Run(() =>
        {
            if (!RequestValid)
            {
                this.CreateToast("Invalid proposal", "Make sure you've added a planning engineer");
                return;
            }
            this.IsBusy = true;
            this.BusyText = "Submitting budget proposal";

            CurrentProject.Modified = DateTime.UtcNow;
            CurrentProject.ModifiedBy = App.CurrentUser.Id;
            CurrentProject.ApprovalState = RequestStatus.Open;
            var projectResponse = this.projectService.UpdateProject(CurrentProject);
            if (projectResponse)
            {
                CurrentRequest.Created = DateTime.UtcNow;
                CurrentRequest.CreatedBy = App.CurrentUser.Id;
                CurrentRequest.Modified = DateTime.UtcNow;
                CurrentRequest.ModifiedBy = App.CurrentUser.Id;
                CurrentRequest.IsActive = true;
                CurrentRequest.RequestStatus = RequestStatus.Open;
                CurrentRequest.ProjectID = CurrentProject.Id ?? 0;
                if (this.CurrentRequest.PlanningEngineer != null)
                {
                    CurrentRequest.PlanningEngineerID = this.CurrentRequest.PlanningEngineer.Id;
                    this.CurrentRequest.PlanningEngineer = null;
                }
                if (!this.IsAddingEndTime)
                {
                    CurrentRequest.EndTime = null;
                }
                else if (this.CurrentRequest.EndTimeOffset != null)
                {
                    CurrentRequest.EndTime = ((DateTimeOffset)CurrentRequest.EndTimeOffset).UtcDateTime;
                }
                if (this.CurrentRequest.StartTimeOffset != null)
                {
                    CurrentRequest.StartTime = ((DateTimeOffset)CurrentRequest.StartTimeOffset).UtcDateTime;
                }
                this.CurrentRequest.UserID = App.CurrentUser.Id ?? 0;
                var response = this.requestService.InsertRequest(CurrentRequest);
                if (response)
                {
                    this.IsBusy = false;
                    CreateToast("Budget Proposal Submitted", "Budget proposal was successfully submitted");
                    CloseDialog();
                    ViewProject();
                }
                else
                {
                    this.IsBusy = false;
                    CreateToast("Couldn't create budget proposal", "Proposal creation ran into an error, please try again later.");
                }
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

public class RequestsViewModel : ModuleViewModel
{
    public const int UserRoleId = 3;
    public const int PlanningEngineerRoleId = 2;
    private readonly IUserService userService;
    private readonly IRequestService requestService;
    private readonly IRequestCommentService requestCommentService;
    private readonly ITimeLogService timeLogService;
    private readonly IProjectService projectService;
    private bool isUser;
    private bool isPlanningEngineer;
    private Request currentRequest;
    private RequestStatus updateStatus;
    private string updateStatusComment;

    public bool IsUser { get => this.isUser; set => this.RaiseAndSetIfChanged(ref this.isUser, value); }
    public bool IsPlanningEngineer { get => this.isPlanningEngineer; set => this.RaiseAndSetIfChanged(ref this.isPlanningEngineer, value); }
    public bool RequestValid => this.CurrentRequest.PlanningEngineer != null && this.CurrentRequest.StartTimeOffset != null;

    public ObservableCollection<User> AvailableUsers { get; set; } = new();
    public ObservableCollection<User> AvailablePlanningEngineers { get; set; } = new();
    public ObservableCollection<TimeLog> AvailableTimelogs { get; set; } = new();
    public ObservableCollection<Project> AvailableProjects { get; set; } = new();
    public ObservableCollection<Request> AvailableRequests { get; set; } = new() { new() };

    public List<RequestStatus> AvailableRequestStatuses { get; set; } = new List<RequestStatus>()
    {
        RequestStatus.Accepted,
        RequestStatus.Rejected,
        RequestStatus.UpdateRequested
    };

    public RequestStatus UpdateStatus { get => updateStatus; set => this.RaiseAndSetIfChanged(ref this.updateStatus, value); }
    public string UpdateStatusComment { get => updateStatusComment; set => this.RaiseAndSetIfChanged(ref this.updateStatusComment, value); }
    public Request CurrentRequest { get => currentRequest; set => this.RaiseAndSetIfChanged(ref this.currentRequest, value); }

    public ICommand ReloadRequestsCommand { get; }
    public ICommand CloseDialogCommand { get; }


    public ICommand CreateRequestCommand { get; }
    public ICommand SubmitCreateRequestCommand { get; }
    public ICommand UpdateRequestStatusCommand { get; }
    public ICommand SubmitUpdateRequestStatusCommand { get; }
    public ICommand PostCommentCommand { get; }
    public bool IsAddingEndTime { get; private set; }

    public RequestsViewModel()
    {
        this.userService = (IUserService)App.Container.GetService(typeof(IUserService));
        this.requestService = (IRequestService)App.Container.GetService(typeof(IRequestService));
        this.requestCommentService = (IRequestCommentService)App.Container.GetService(typeof(IRequestCommentService));
        this.timeLogService = (ITimeLogService)App.Container.GetService(typeof(ITimeLogService));
        this.projectService = (IProjectService)App.Container.GetService(typeof(IProjectService));
        this.IsUser = App.CurrentUser.RoleID == UserRoleId;
        this.IsPlanningEngineer = App.CurrentUser.RoleID == PlanningEngineerRoleId || App.CurrentUser.RoleID == 1;
        this.ReloadRequestsCommand = ReactiveCommand.Create(LoadRequests);
        this.CloseDialogCommand = ReactiveCommand.Create(CloseDialog);
        this.CreateRequestCommand = ReactiveCommand.Create(CreateRequest);
        this.SubmitCreateRequestCommand = ReactiveCommand.Create(SubmitCreateRequest);
        this.UpdateRequestStatusCommand = ReactiveCommand.Create<Request>(UpdateRequestStatus);
        this.SubmitUpdateRequestStatusCommand = ReactiveCommand.Create(SubmitUpdateRequestStatus);
        this.PostCommentCommand = ReactiveCommand.Create<Request>(PostComment);
        LoadRequests();
    }

    private void LoadRequests()
    {
        Task.Run(() =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                this.IsBusy = true;
                this.BusyText = "Fetching Requests";
            });
            var availableUsers = this.userService.GetUsers();
            var availableProjects = this.projectService.GetProjects();
            var availableTimelogs = this.timeLogService.GetTimeLogs();
            List<Request> requests = new List<Request>();
            requests = this.requestService.GetRequests();

            if (IsUser)
            {
                requests = requests.Where(itm => itm.UserID == App.CurrentUser.Id).ToList();
                availableTimelogs = availableTimelogs.Where(itm => itm.UserID == App.CurrentUser.Id).ToList();
            }
            Dispatcher.UIThread.Invoke(() =>
            {
                this.AvailableRequests.Clear();
            });
            foreach (Request request in requests)
            {
                request.RequestComments = this.requestCommentService.GetRequestCommentsByRequestId(request.Id ?? 0);
                request.PlanningEngineer = availableUsers.FirstOrDefault(itm => itm.Id == request.PlanningEngineerID) ?? new();
                foreach (var comment in request.RequestComments)
                {
                    comment.User = availableUsers.FirstOrDefault(itm => itm.Id == comment.UserID) ?? new();
                }
                request.User = availableUsers.FirstOrDefault(itm => itm.Id == request.UserID) ?? new();
                request.ModifiedByUser = availableUsers.FirstOrDefault(itm => itm.Id == request.ModifiedBy) ?? new();
                request.CreatedByUser = availableUsers.FirstOrDefault(itm => itm.Id == request.CreatedBy) ?? new();
                request.TimeLog = availableTimelogs.FirstOrDefault(itm => itm.Id == request.TimeLogID);
                if (request.TimeLog != null)
                {
                    request.TimeLog.Project = availableProjects.FirstOrDefault(itm => itm.Id == request.TimeLog.ProjectID) ?? new();
                    request.TimeLog.Project.ModifiedByUser = availableUsers.FirstOrDefault(itm => itm.Id == request.TimeLog.Project.ModifiedBy) ?? new();
                    request.TimeLog.Project.CreatedByUser = availableUsers.FirstOrDefault(itm => itm.Id == request.TimeLog.Project.CreatedBy) ?? new();
                }
                request.Project = availableProjects.FirstOrDefault(itm => itm.Id == request.ProjectID);
                if (request.Project != null)
                {
                    request.Project.ModifiedByUser = availableUsers.FirstOrDefault(itm => itm.Id == request.Project.ModifiedBy) ?? new();
                    request.Project.CreatedByUser = availableUsers.FirstOrDefault(itm => itm.Id == request.Project.CreatedBy) ?? new();
                }
                else if (request.TimeLog?.Project != null)
                {
                    request.Project = request.TimeLog.Project;
                }
            }

            Dispatcher.UIThread.Invoke(() =>
            {
                this.AvailableUsers.Clear();
                this.AvailableUsers.AddRange(availableUsers);
                this.AvailablePlanningEngineers.Clear();
                this.AvailablePlanningEngineers.AddRange(availableUsers.Where(itm => itm.RoleID == PlanningEngineerRoleId));
                this.AvailableTimelogs.Clear();
                this.AvailableTimelogs.AddRange(availableTimelogs);

                this.AvailableProjects.Clear();
                this.AvailableProjects.AddRange(availableProjects);
                this.AvailableRequests.AddRange(requests);
                this.IsBusy = false;
            });
        });
    }

    private void CreateRequest()
    {
        this.CurrentRequest = new();
        var view = new CreateRequestView()
        {
            DataContext = this
        };
        SukiHost.ShowDialog(App.WorkspaceInstance, view, allowBackgroundClose: false);
    }

    private void SubmitCreateRequest()
    {
        Task.Run(() =>
        {
            if (!RequestValid)
            {
                this.CreateToast("Invalid Request", "Make sure you've added a planning engineer");
                return;
            }
            this.IsBusy = true;
            this.BusyText = "Submitting Request";
            CurrentRequest.Created = DateTime.UtcNow;
            CurrentRequest.CreatedBy = App.CurrentUser.Id;
            CurrentRequest.Modified = DateTime.UtcNow;
            CurrentRequest.ModifiedBy = App.CurrentUser.Id;
            CurrentRequest.IsActive = true;
            CurrentRequest.RequestStatus = RequestStatus.Open;
            CurrentRequest.ProjectID = CurrentRequest.Project?.Id ?? 0;
            CurrentRequest.Project = null;
            if (CurrentRequest.TimeLog != null)
            {
                CurrentRequest.TimeLogID = CurrentRequest.TimeLog.Id;
            }
            if (this.CurrentRequest.PlanningEngineer != null)
            {
                CurrentRequest.PlanningEngineerID = this.CurrentRequest.PlanningEngineer.Id;
                this.CurrentRequest.PlanningEngineer = null;
            }
            if (!this.IsAddingEndTime)
            {
                CurrentRequest.EndTime = null;
            }
            else if (this.CurrentRequest.EndTimeOffset != null)
            {
                CurrentRequest.EndTime = ((DateTimeOffset)CurrentRequest.EndTimeOffset).UtcDateTime;
            }
            if (this.CurrentRequest.StartTimeOffset != null)
            {
                CurrentRequest.StartTime = ((DateTimeOffset)CurrentRequest.StartTimeOffset).UtcDateTime;
            }
            this.CurrentRequest.UserID = App.CurrentUser.Id ?? 0;
            var response = this.requestService.InsertRequest(CurrentRequest);
            if (response)
            {
                this.IsBusy = false;
                CreateToast("Request Submitted", "Your request was successfully submitted");
                CloseDialog();
                LoadRequests();
            }
            else
            {
                this.IsBusy = false;
                CreateToast("Couldn't create your request", "Request creation ran into an error, please try again later.");
            }
        });
    }

    private void UpdateRequestStatus(Request request)
    {
        this.CurrentRequest = request;
        var view = new UpdateRequestStatusView()
        {
            DataContext = this
        };
        SukiHost.ShowDialog(App.WorkspaceInstance, view, allowBackgroundClose: true);
    }

    private void SubmitUpdateRequestStatus()
    {
        Task.Run(() =>
        {
            this.IsBusy = true;
            this.BusyText = "Updating Request";
            CurrentRequest.Modified = DateTime.UtcNow;
            CurrentRequest.ModifiedBy = App.CurrentUser.Id;
            CurrentRequest.IsActive = true;
            CurrentRequest.ProjectID = CurrentRequest.Project?.Id ?? 0;

            if (CurrentRequest.TimeLog == null && CurrentRequest.Project != null)
            {
                CurrentRequest.Project.ModifiedBy = App.CurrentUser.Id;
                CurrentRequest.Project.Modified = DateTime.UtcNow;
                CurrentRequest.Project.ApprovalState = CurrentRequest.RequestStatus;
                CurrentRequest.Project.ModifiedByUser = null;
                CurrentRequest.Project.CreatedByUser = null;
                var projectResponse = this.projectService.UpdateProject(CurrentRequest.Project);
                if (!projectResponse)
                {
                    this.CreateToast("Unable to update project status", "Unable to update project status, please try again later");
                    CloseDialog();
                    LoadRequests();
                    return;
                }
            }
            else if (CurrentRequest.TimeLog != null)
            {
                CurrentRequest.TimeLog.CreatedByUser = null;
                CurrentRequest.TimeLog.ModifiedByUser = null;
                CurrentRequest.TimeLog.ModifiedBy = App.CurrentUser.Id;
                CurrentRequest.TimeLog.Modified = DateTime.UtcNow;
                CurrentRequest.TimeLog.StartDateTime = CurrentRequest.StartTime;
                CurrentRequest.TimeLog.EndDateTime = CurrentRequest.EndTime;
                CurrentRequest.TimeLog.ProjectID = CurrentRequest.TimeLog.Project.Id ?? 0;
                CurrentRequest.TimeLog.Project = null;
                if (CurrentRequest.Timestamp != null)
                {
                    CurrentRequest.TimeLog.Duration = (TimeSpan)CurrentRequest.Timestamp;
                }
                var timeLogResponse = this.timeLogService.UpdateTimeLog(CurrentRequest.TimeLog);
                if (!timeLogResponse)
                {
                    this.CreateToast("Unable to update time log status", "Unable to update status of the timelog, please try again later");
                    CloseDialog();
                    LoadRequests();
                    return;
                }
            }
            CurrentRequest.Project = null;
            CurrentRequest.CreatedByUser = null;
            CurrentRequest.ModifiedByUser = null;
            CurrentRequest.TimeLog = null;
            var response = this.requestService.UpdateRequest(CurrentRequest);
            if (response)
            {
                var commentResponse = CreateRequestComment(this.UpdateStatusComment, CurrentRequest.Id ?? 0);
                if (commentResponse)
                {
                    this.CreateToast("Successfully modified status", "The status was changed successfully");
                    this.CloseDialog();
                    LoadRequests();
                }
                else
                {
                    this.CreateToast("Update status comment could not be created", "The status update comment could not be created, please try again later.");
                    this.CloseDialog();
                    LoadRequests();
                }
            }
            else
            {
                this.CreateToast("Failed to update request status", "Failed to update the request status, please try again later");
                this.CloseDialog();
                LoadRequests();
            }
        });
    }

    private void PostComment(Request request)
    {
        Task.Run(() =>
        {
            if (string.IsNullOrWhiteSpace(request.PendingComment))
            {
                CreateToast("Cannot add comment", "Comment body is missing");
                return;
            }
            var commentText = request.PendingComment;
            var response = CreateRequestComment(commentText, request.Id ?? 0);

            if (response)
            {
                this.CreateToast("Comment Added", "Your comment was added successfully");
                this.LoadRequests();
            }
            else
            {
                this.CreateToast("Comment creation failed", "Could not add your comment, please try again later");
            }
        });
    }

    private bool CreateRequestComment(string commentText, int requestId)
    {
        var comment = new RequestComment()
        {
            Comment = commentText,
            UserID = App.CurrentUser.Id ?? 0,
            CreatedBy = App.CurrentUser.Id ?? 0,
            Created = DateTime.UtcNow,
            ModifiedBy = App.CurrentUser.Id ?? 0,
            Modified = DateTime.UtcNow,
            RequestID = requestId,
        };

        return this.requestCommentService.InsertRequestComment(comment);
    }
}
