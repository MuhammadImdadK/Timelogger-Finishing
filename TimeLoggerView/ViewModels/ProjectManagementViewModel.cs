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
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TimeLoggerView.Views;
using TimeLoggerView.Views.Projects;
using TimeLoggerView.Views.Timesheet;

namespace TimeLoggerView.ViewModels;

public class ProjectManagementViewModel : ModuleViewModel
{
    private const int ErfOffset = 100001;
    private const int PlanningEngineerRoleId = 2;
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
    private bool isPlanningEngineer;
    private readonly IProjectService projectService;
    private readonly IAttachmentService attachmentService;
    private readonly IUserService userService;
    private readonly IRequestService requestService;
    private readonly ITimeLogService timesheetService;

    public bool IsPlanningEngineer { get => isPlanningEngineer; set => this.RaiseAndSetIfChanged(ref isPlanningEngineer, value); }
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
    public ICommand LoadProjectCommand { get; }
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
    public ICommand SubmitDeleteDeliverableCommand { get; }

    public ICommand AddAttachmentCommand { get; }
    public ICommand EditDeliverableCommand { get; }
    public ICommand DeleteDeliverableCommand { get; }
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
        this.timesheetService = (ITimeLogService)App.Container.GetService(typeof(ITimeLogService));
        this.IsPlanningEngineer = App.CurrentUser.RoleID == PlanningEngineerRoleId || App.CurrentUser.RoleID == 1;
        this.ViewProjectCommand = ReactiveCommand.Create<Project>(ViewProject);
        this.BeginEditCommand = ReactiveCommand.Create(BeginEdit);
        this.SaveEditCommand = ReactiveCommand.Create(SaveEdit);
        this.CancelEditCommand = ReactiveCommand.Create(CancelEdit);
        this.SubmitBudgetCommand = ReactiveCommand.Create(SubmitBudget);
        this.CancelBudgetCommand = ReactiveCommand.Create(CancelSubmitBudget);
        this.PerformSubmitBudgetCommand = ReactiveCommand.Create(PerformSubmitBudget);
        this.MarkProjectAsClosedCommand = ReactiveCommand.Create(MarkProjectAsClosed);
        this.AddAttachmentCommand = ReactiveCommand.Create(AddAttachment);
        this.EditDeliverableCommand = ReactiveCommand.Create<Model.ModelSql.Drawing>(EditAttachment);
        this.DeleteDeliverableCommand = ReactiveCommand.Create<Model.ModelSql.Drawing>(DeleteAttachment);
        this.SubmitAttachmentCommand = ReactiveCommand.Create(SubmitAttachment);
        this.CancelAttachmentCommand = ReactiveCommand.Create(CancelAttachment);
        this.LogTimeCommand = ReactiveCommand.Create(LogTime);
        this.CloseDialogCommand = ReactiveCommand.Create(CloseDialog);
        this.SubmitMarkProjectAsClosedCommand = ReactiveCommand.Create(SubmitMarkProjectAsClosed);
        this.CancelMarkProjectAsClosedCommand = ReactiveCommand.Create(CancelMarkProjectAsClosed);
        this.SubmitDeleteDeliverableCommand = ReactiveCommand.Create(SubmitDeleteDeliverable);
        this.CreateProjectCommand = ReactiveCommand.Create(CreateProject);
        this.SubmitCreateProjectCommand = ReactiveCommand.Create(SubmitCreateProject);
        this.CloseProjectViewCommand = ReactiveCommand.Create(CloseProjectView);
        this.LoadProjectCommand = ReactiveCommand.Create(LoadProjects);
        this.LoadProjects();
    }

    private void SubmitDeleteDeliverable()
    {
        Task.Run(() =>
        {
            ErrorText = string.Empty;
            var hasTimelogs = this.timesheetService.GetTimeLogs().Any(itm => itm.DeliverableID == CurrentAttachment.Id);
            if (hasTimelogs)
            {
                ErrorText = $"{CurrentAttachment.Name} has time logged against it, so it cannot be deleted.";
                return;
            }

            this.attachmentService.DeleteAttachment(CurrentAttachment);

            this.CloseDialog();
            this.ViewProject();
        });
    }

    private void DeleteAttachment(Model.ModelSql.Drawing deliverable)
    {
        CurrentAttachment = deliverable;
        var view = new ConfirmDeleteDeliverable()
        {
            DataContext = this
        };
        SukiHost.ShowDialog(App.WorkspaceInstance, view, allowBackgroundClose: false);
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

                MainViewModel.TimesheetManagement.LoadDataCommand.Execute(Unit.Default);
                Dispatcher.UIThread.Invoke(() =>
                {

                    if (App.WorkspaceInstance.DataContext is MainViewModel mvm)
                    {
                        mvm.ProjectManagement.LoadProjectCommand.Execute(Unit.Default);
                    }
                });
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
                MainViewModel.TimesheetManagement.LoadDataCommand.Execute(Unit.Default);
                Dispatcher.UIThread.Invoke(() =>
                {

                    if (App.WorkspaceInstance.DataContext is MainViewModel mvm)
                    {
                        mvm.ProjectManagement.LoadProjectCommand.Execute(Unit.Default);
                    }
                });
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
        this.ErrorText = string.Empty;
        this.CloseDialog();
        this.ViewProject();
    }

    private void AddAttachment()
    {
        this.IsAddingAttachment = true;
        this.CurrentAttachment = new();
    }

    private void EditAttachment(Model.ModelSql.Drawing attachment)
    {
        this.IsAddingAttachment = true;
        this.CurrentAttachment = attachment;
    }

    private void SubmitAttachment()
    {
        Task.Run(() =>
        {
            this.IsBusy = true;
            this.BusyText = this.CurrentAttachment.Id != null ? "Updating Deliverable" : "Adding Deliverable";
            this.CurrentAttachment.ProjectId = this.CurrentProject.Id ?? 0;
            this.CurrentAttachment.ModifiedByUser = null;
            this.CurrentAttachment.CreatedByUser = null;
            this.CurrentAttachment.CreatedBy = App.CurrentUser.Id;
            this.CurrentAttachment.Created = DateTime.UtcNow;
            this.CurrentAttachment.ModifiedBy = App.CurrentUser.Id;
            this.CurrentAttachment.Modified = DateTime.UtcNow;

            var result = false;
            if (this.CurrentAttachment.Id == null)
            {
                result = this.attachmentService.InsertAttachment(this.CurrentAttachment);
            }
            else
            {
                result = this.attachmentService.UpdateAttachment(this.CurrentAttachment);
            }
            if (result)
            {
                this.IsEditing = false;
                this.IsAddingAttachment = false;
                this.CurrentAttachment = new();
                MainViewModel.TimesheetManagement.LoadDataCommand.Execute(Unit.Default);
                Dispatcher.UIThread.Invoke(() =>
                {
                    if (App.WorkspaceInstance.DataContext is MainViewModel mvm)
                    {
                        mvm.ProjectManagement.LoadProjectCommand.Execute(Unit.Default);
                    }
                });
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
        this.ErrorText = string.Empty;
        this.CurrentProject = new();
        var erf = (this.projectService.GetLatestProjectId() ?? 0) + ErfOffset;
        CurrentProject.ERFNumber = erf.ToString();
        var view = new CreateProjectView()
        {
            DataContext = this
        };
        SukiHost.ShowDialog(App.WorkspaceInstance, view, allowBackgroundClose: false);
    }

    private void SubmitCreateProject()
    {
        var tempText = "The following validation errors were encountered:\n";
        var valid = true;
        if (string.IsNullOrWhiteSpace(this.CurrentProject.ERFNumber))
        {
            valid = false;
            tempText += "- ERF Number is required\n";
        }
        if (string.IsNullOrWhiteSpace(this.CurrentProject.ProjectName))
        {
            valid = false;
            tempText += "- Project Name is required\n";
        }
        if (string.IsNullOrWhiteSpace(this.CurrentProject.ManhourBudget.ToString()) || this.CurrentProject.ManhourBudget <= 0)
        {
            valid = false;
            tempText += "- An initial estimate more than zero is required\n";
        }

        if (!valid)
        {
            this.ErrorText = tempText;

            return;
        }

        Task.Run(() =>
        {
            this.IsBusy = true;
            this.BusyText = "Creating Project";

            CurrentProject.Created = DateTime.UtcNow;
            CurrentProject.CreatedBy = App.CurrentUser.Id;
            CurrentProject.Modified = DateTime.UtcNow;
            CurrentProject.ModifiedBy = App.CurrentUser.Id;
            CurrentProject.IsActive = true;
            CurrentProject.ApprovalState = RequestStatus.Accepted;
            if (string.IsNullOrWhiteSpace(CurrentProject.ERFNumber?.Trim()))
            {
                var erf = (this.projectService.GetLatestProjectId() ?? 0) + ErfOffset;
                CurrentProject.ERFNumber = erf.ToString();
            }
            var result = this.projectService.InsertProject(CurrentProject);

            Thread.Sleep(1000);

            if (result)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    this.CloseDialog();
                    MainViewModel.TimesheetManagement.LoadDataCommand.Execute(Unit.Default);
                    if (App.WorkspaceInstance.DataContext is MainViewModel mvm)
                    {
                        mvm.ProjectManagement.LoadProjectCommand.Execute(Unit.Default);
                    }
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
                CurrentRequest.RequestStatus = RequestStatus.Accepted;
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
        this.IsAddingAttachment = false;
        this.CurrentAttachment = new();
        this.CloseDialog();
        Task.Run(() =>
        {
            Thread.Sleep(500);
            this.LoadProjects();
        });
    }
}
