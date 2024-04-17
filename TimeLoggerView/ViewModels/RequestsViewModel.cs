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
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TimeLoggerView.Views;

namespace TimeLoggerView.ViewModels;

public class RequestsViewModel : ModuleViewModel
{
    public const int UserRoleId = 3;
    public const int PlanningEngineerRoleId = 2;
    private readonly IUserService userService;
    private readonly IRequestService requestService;
    private readonly IRequestCommentService requestCommentService;
    private readonly ITimeLogService timeLogService;
    private readonly IProjectService projectService;
    private readonly IAttachmentService attachmentService;
    private bool isUser;
    private bool isPlanningEngineer;
    private bool isNewTimeLog;
    private Request currentRequest;
    private RequestStatus? updateStatus;
    private string updateStatusComment;
    private bool isAddingEndTime = true;
    private RequestType selectedRequestType = RequestType.TimeLog;
    private bool isAddingProjectRequest = false;
    private string submitButtonText;
    private DateTime? startDateTime;
    private DateTime? endDateTime;
    private string endDateLocalTime = "Unknown";
    private TimeSpan? duration;
    private TimeLog previousTimelog;
    private Project? selectedProject;
    private Drawing selectedDeliverable;
    private DisciplineType? disciplineType;
    private DrawingType? drawingType;
    private ScopeType? scopeType;
    private TeamType? teamType;

    public bool IsUser { get => this.isUser; set => this.RaiseAndSetIfChanged(ref this.isUser, value); }
    public bool IsPlanningEngineer { get => this.isPlanningEngineer; set => this.RaiseAndSetIfChanged(ref this.isPlanningEngineer, value); }
    public bool RequestValid => this.CurrentRequest.PlanningEngineer != null && this.CurrentRequest.StartTimeOffset != null;
    public bool IsNewTimeLog
    {
        get => this.isNewTimeLog;
        set
        {
            this.RaiseAndSetIfChanged(ref this.isNewTimeLog, value);
            if (value)
            {
                this.PreviousTimelog = this.CurrentRequest.TimeLog;
                if (this.PreviousTimelog == null)
                {
                    this.RaiseAndSetIfChanged(ref this.CurrentRequest.timeLog, new());
                }
                else
                {
                    this.RaiseAndSetIfChanged(ref this.CurrentRequest.timeLog, this.PreviousTimelog);
                }
            }
            else
            {
                var pretl = this.CurrentRequest.TimeLog;
                this.RaiseAndSetIfChanged(ref this.CurrentRequest.timeLog, this.PreviousTimelog);
                this.PreviousTimelog = pretl;
            }
        }
    }
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

    public List<RequestType> AvailableRequestTypes { get; set; } = new()
    {
        RequestType.Project,
        RequestType.TimeLog
    };

    public bool IsAddingProjectRequest { get => isAddingProjectRequest; set => this.RaiseAndSetIfChanged(ref isAddingProjectRequest, value); }

    public RequestType SelectedRequestType
    {
        get => selectedRequestType;
        set
        {
            this.RaiseAndSetIfChanged(ref selectedRequestType, value);
            IsAddingProjectRequest = value == RequestType.Project;
        }
    }
    public RequestStatus? UpdateStatus { get => updateStatus; set => this.RaiseAndSetIfChanged(ref this.updateStatus, value); }
    public string UpdateStatusComment { get => updateStatusComment; set => this.RaiseAndSetIfChanged(ref this.updateStatusComment, value); }
    public string SubmitButtonText { get => submitButtonText; set => this.RaiseAndSetIfChanged(ref this.submitButtonText, value); }
    public Request CurrentRequest { get => currentRequest; set => this.RaiseAndSetIfChanged(ref this.currentRequest, value); }
    public TimeLog? PreviousTimelog { get => previousTimelog; set => this.RaiseAndSetIfChanged(ref previousTimelog, value); }

    public ICommand ReloadRequestsCommand { get; }
    public ICommand CloseDialogCommand { get; }


    public ICommand CreateRequestCommand { get; }
    public ICommand EditRequestCommand { get; }
    public ICommand SubmitCreateRequestCommand { get; }
    public ICommand UpdateRequestStatusCommand { get; }
    public ICommand SubmitUpdateRequestStatusCommand { get; }
    public ICommand PostCommentCommand { get; }
    public bool IsAddingEndTime { get => this.isAddingEndTime; set => this.RaiseAndSetIfChanged(ref this.isAddingEndTime, value); }


    public DateTimeOffset? EndDateTimeOffset
    {
        get
        {
            if (this.EndDateTime == null)
                return null;
            return new DateTimeOffset((DateTime)this.EndDateTime);
        }
        set
        {
            if (value == null)
                this.EndDateTime = null;
            else
                this.EndDateTime = ((DateTimeOffset)value).UtcDateTime;
        }
    }

    public DateTimeOffset? StartDateTimeOffset
    {
        get => this.StartDateTime != null ? new DateTimeOffset((DateTime)this.StartDateTime) : null;
        set
        {
            if (value != null)
            {
                this.StartDateTime = ((DateTimeOffset)value).Date.Add(StartDateTimeSpan ?? TimeSpan.Zero);
            }
            else
            {
                this.StartDateTime = null;
            }

            this.EndDateTimeOffset = value;
        }
    }
    public DateTime? StartDateTime
    {
        get => startDateTime;
        set
        {
            this.RaiseAndSetIfChanged(ref this.startDateTime, value);
            this.EndDateTime = value;
            this.EndDateLocalTime = EndDateTime != null
                ? ((DateTime)EndDateTime).ToLocalTime().ToString("f", CultureInfo.CurrentCulture)
                : "Unknown";
        }
    }
    public DateTime? EndDateTime
    {
        get => endDateTime;
        set
        {
            if (StartDateTime != null && Duration != null)
            {
                this.RaiseAndSetIfChanged(ref this.endDateTime, (((DateTimeOffset)this.StartDateTimeOffset).UtcDateTime).Add((TimeSpan)Duration));
                this.EndDateLocalTime = EndDateTime != null
                    ? ((DateTime)EndDateTime).ToLocalTime().ToString("f", CultureInfo.CurrentCulture)
                    : "Unknown";
            }
        }
    }
    public string EndDateLocalTime { get => endDateLocalTime; set => this.RaiseAndSetIfChanged(ref endDateLocalTime, value); }
    public TimeSpan? StartDateTimeSpan
    {
        get
        {
            if (this.StartDateTime == null)
            {
                return null;
            }
            var conv = (DateTime)StartDateTime;
            return conv.TimeOfDay;
        }
        set
        {
            if (value != null && StartDateTime != null)
            {
                var conv = (DateTime)StartDateTime;
                var ts = (TimeSpan)value;
                StartDateTime = conv.Date.Add(ts);
            }
        }
    }

    public TimeSpan? Duration
    {
        get => duration;
        set
        {
            this.RaiseAndSetIfChanged(ref this.duration, value);
            this.EndDateTime = DateTime.UtcNow;
        }
    }

    public Project? SelectedProject
    {
        get => selectedProject;
        set
        {
            this.RaiseAndSetIfChanged(ref this.selectedProject, value);
            this.AvailableDeliverables.Clear();
            if (value != null)
            {
                var deliverables = this.attachmentService.GetDrawingsByProjectId(value.Id ?? 0);
                this.AvailableDeliverables.AddRange(deliverables);
            }
        }
    }
    public List<DisciplineType> AvailableDisciplineTypes { get; } = new List<DisciplineType>()
    {
        Common.Enums.DisciplineType.None,
        Common.Enums.DisciplineType.Process,
        Common.Enums.DisciplineType.Piping,
        Common.Enums.DisciplineType.IE,
        Common.Enums.DisciplineType.Civil
    };
    public List<DrawingType> AvailableDrawingTypes { get; } = new List<DrawingType>()
    {
        Common.Enums.DrawingType.None,
        Common.Enums.DrawingType.PipingAndInstrimentDiagram,
        Common.Enums.DrawingType.CauseAndEffect,
        Common.Enums.DrawingType.ProcessFlowDiagram,
        Common.Enums.DrawingType.MasterEquipmentList,
        Common.Enums.DrawingType.MasterLinetList,
        Common.Enums.DrawingType.Calculation,
        Common.Enums.DrawingType.Report
    };
    public List<ScopeType> AvailableScopeTypes { get; } = new List<ScopeType>()
    {
        Common.Enums.ScopeType.None,
        Common.Enums.ScopeType.Installation,
        Common.Enums.ScopeType.Demolation,
        Common.Enums.ScopeType.AsBuilt,
        Common.Enums.ScopeType.Relocation,
        Common.Enums.ScopeType.Standard
    };
    public List<TeamType> AvailableTeamTypes { get; } = new List<TeamType>()
    {
        Common.Enums.TeamType.None,
        Common.Enums.TeamType.CoreTeam,
        Common.Enums.TeamType.AdditionalTeam
    };
    public Drawing SelectedDeliverable { get => selectedDeliverable; set => selectedDeliverable = value; }
    public ObservableCollection<Project> Projects { get; set; } = new();
    public ObservableCollection<Drawing> AvailableDeliverables { get; set; } = new();
    public ObservableCollection<User> Users { get; set; } = new();
    public DisciplineType? DisciplineType { get => disciplineType; set => this.RaiseAndSetIfChanged(ref disciplineType, value); }
    public DrawingType? DrawingType { get => drawingType; set => this.RaiseAndSetIfChanged(ref drawingType, value); }
    public ScopeType? ScopeType { get => scopeType; set => this.RaiseAndSetIfChanged(ref scopeType, value); }
    public TeamType? TeamType { get => teamType; set => this.RaiseAndSetIfChanged(ref teamType, value); }

    public RequestsViewModel()
    {
        this.userService = (IUserService)App.Container.GetService(typeof(IUserService));
        this.requestService = (IRequestService)App.Container.GetService(typeof(IRequestService));
        this.requestCommentService = (IRequestCommentService)App.Container.GetService(typeof(IRequestCommentService));
        this.timeLogService = (ITimeLogService)App.Container.GetService(typeof(ITimeLogService));
        this.projectService = (IProjectService)App.Container.GetService(typeof(IProjectService));
        this.attachmentService = (IAttachmentService)App.Container.GetService(typeof(IAttachmentService));
        this.IsUser = App.CurrentUser.RoleID == UserRoleId;
        this.IsPlanningEngineer = App.CurrentUser.RoleID == PlanningEngineerRoleId || App.CurrentUser.RoleID == 1;
        this.ReloadRequestsCommand = ReactiveCommand.Create(LoadRequests);
        this.CloseDialogCommand = ReactiveCommand.Create(CloseDialog);
        this.CreateRequestCommand = ReactiveCommand.Create(CreateRequest);
        this.SubmitCreateRequestCommand = ReactiveCommand.Create(SubmitCreateRequest);
        this.UpdateRequestStatusCommand = ReactiveCommand.Create<Request>(UpdateRequestStatus);
        this.EditRequestCommand = ReactiveCommand.Create<Request>(EditRequest);
        this.SubmitUpdateRequestStatusCommand = ReactiveCommand.Create(SubmitUpdateRequestStatus);
        this.PostCommentCommand = ReactiveCommand.Create<Request>(PostComment);
        LoadRequests();
    }

    private void LoadRequests()
    {
        Project? reattach = null;
        Drawing? reattachDeliverable = null;
        if (SelectedProject != null)
        {
            reattach = SelectedProject;
        }
        if (SelectedDeliverable != null)
        {
            reattachDeliverable = SelectedDeliverable;
        }

        Dispatcher.UIThread.Invoke(() =>
        {
            this.IsBusy = true;
            this.BusyText = "Fetching Data";
        });

        Task.Run(() =>
        {
            var availableUsers = this.userService.GetUsers();
            var availableProjects = this.projectService.GetProjects();
            var availableDeliverables = this.attachmentService.GetDrawings();

            Dispatcher.UIThread.Invoke(() =>
            {
                this.IsBusy = true;
                this.BusyText = "Fetching Requests";
            });


            var availableTimelogs = this.timeLogService.GetTimeLogs().ToList();
            List<Request> requests = new List<Request>();
            requests = this.requestService.GetRequests();

            if (IsUser)
            {
                requests = requests.Where(itm => itm.UserID == App.CurrentUser.Id).ToList();
                availableTimelogs = availableTimelogs.Where(itm => itm.UserID == App.CurrentUser.Id && itm.IsVisibleToUser).ToList();

            }
            foreach (var availableTimelog in availableTimelogs)
            {
                var proj = availableProjects.FirstOrDefault(itm => itm.Id == availableTimelog.ProjectID);
                availableTimelog.Project = proj ?? new();
                availableTimelog.Project.ModifiedByUser = availableUsers.FirstOrDefault(itm => itm.Id == availableTimelog.Project.ModifiedBy) ?? new();
                availableTimelog.Project.CreatedByUser = availableUsers.FirstOrDefault(itm => itm.Id == availableTimelog.Project.CreatedBy) ?? new();
                availableTimelog.ProjectNumber = proj?.ERFNumber ?? string.Empty;
                availableTimelog.ProjectPrefix = proj?.ProjectPrefix ?? string.Empty;
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
                    var proj = availableProjects.FirstOrDefault(itm => itm.Id == request.TimeLog.ProjectID);
                    request.TimeLog.Project =  proj ?? new();
                    request.TimeLog.Project.ModifiedByUser = availableUsers.FirstOrDefault(itm => itm.Id == request.TimeLog.Project.ModifiedBy) ?? new();
                    request.TimeLog.Project.CreatedByUser = availableUsers.FirstOrDefault(itm => itm.Id == request.TimeLog.Project.CreatedBy) ?? new();
                    request.TimeLog.ProjectNumber = proj?.ERFNumber ?? string.Empty;
                    request.TimeLog.ProjectPrefix = proj?.ProjectPrefix ?? string.Empty;
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
                this.Users.Clear();
                this.AvailableDeliverables.Clear();
                this.AvailableDeliverables.Add(availableDeliverables);
                this.Users.AddRange(availableUsers);
                this.IsBusy = false;

                if (reattach != null)
                {
                    this.SelectedProject = this.Projects.FirstOrDefault(itm => itm.Id == reattach.Id) ?? new();
                }
                if (reattachDeliverable != null)
                {
                    this.SelectedDeliverable = this.AvailableDeliverables.FirstOrDefault(itm => itm.Id == reattachDeliverable.Id) ?? new();
                }
            });
        });
    }

    private void CreateRequest()
    {
        ErrorText = string.Empty;
        this.EndDateLocalTime = "Unknown";
        this.SubmitButtonText = "Create Request";
        this.PreviousTimelog = null;
        this.SelectedProject = null;
        this.CurrentRequest = new();
        this.IsNewTimeLog = false;
        this.IsAddingEndTime = true;
        this.DisciplineType = null;
        this.DrawingType = null;
        this.TeamType = null;
        this.ScopeType = null;

        var view = new CreateRequestView()
        {
            DataContext = this
        };
        SukiHost.ShowDialog(App.WorkspaceInstance, view, allowBackgroundClose: false);
    }

    private void EditRequest(Request request)
    {
        ErrorText = string.Empty;
        this.StartDateTime = request.StartTime;
        this.Duration = request.Timestamp;
        this.PreviousTimelog = null;
        this.IsNewTimeLog = request.IsNewTimeLog;
        this.EndDateLocalTime = EndDateTime != null
                    ? ((DateTime)EndDateTime).ToLocalTime().ToString("f", CultureInfo.CurrentCulture)
                    : "Unknown";

        this.SubmitButtonText = "Update Request";

        this.CurrentRequest = request;
        this.IsAddingEndTime = true;
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
            ErrorText = string.Empty;
            var valid = true;
            var tempText = "The following validation errors were encountered:\n";
            if (this.CurrentRequest.TimeLog != null)
            {
                this.CurrentRequest.TimeLog.StartDateTime = StartDateTime != null ? ((DateTime)StartDateTime).ToUniversalTime() : DateTime.UtcNow;
                this.CurrentRequest.TimeLog.EndDateTime = EndDateTime != null ? ((DateTime)EndDateTime).ToUniversalTime() : DateTime.UtcNow;
                this.CurrentRequest.TimeLog.Duration = Duration ?? TimeSpan.Zero;
                this.CurrentRequest.StartTime = StartDateTime != null ? ((DateTime)StartDateTime).ToUniversalTime() : DateTime.UtcNow;
                this.CurrentRequest.EndTime = EndDateTime != null ? ((DateTime)EndDateTime).ToUniversalTime() : DateTime.UtcNow;
                this.CurrentRequest.Timestamp = Duration ?? TimeSpan.Zero;
            }

            if (this.CurrentRequest.PlanningEngineer == null)
            {
                valid = false;
                tempText += "- A planning engineer must be assigned\n";
            }
            if (this.Duration == null || this.Duration == TimeSpan.Zero)
            {
                valid = false;
                tempText += "- A duration must be provided\n";
            }
            if (this.CurrentRequest.TimeLog == null && !this.IsNewTimeLog)
            {
                valid = false;
                tempText += "- The time log entry to modify must be provided\n";
            }

            if (this.CurrentRequest.TimeLog?.Id == null && this.IsNewTimeLog)
            {
                // we're creating a new time log
                this.CurrentRequest.IsNewTimeLog = true;
                this.CurrentRequest.TimeLog.IsNewTimeLog = true;
                this.CurrentRequest.TimeLog.IsVisibleToUser = false;
                var timelog = this.CurrentRequest.TimeLog;
                if (this.SelectedProject == null)
                {
                    tempText += "- A Project must be selected\n";
                    valid = false;
                }
                if (this.SelectedDeliverable == null)
                {
                    tempText += "- A Deliverable must be selected\n";
                    valid = false;
                }
                if (this.DisciplineType == null)
                {
                    tempText += "- A discipline type must be selected\n";
                    valid = false;
                }
                if (this.DrawingType == null)
                {
                    tempText += "- A drawing type must be selected\n";
                    valid = false;
                }
                if (this.ScopeType == null)
                {
                    tempText += "- A scope type must be selected\n";
                    valid = false;
                }
                if (this.TeamType == null || this.TeamType == Common.Enums.TeamType.None)
                {
                    tempText += "- A team must be selected\n";
                    valid = false;
                }
            }
            if (!valid)
            {
                this.ErrorText = tempText;
                return;
            }
            this.IsBusy = true;
            this.BusyText = "Submitting Request";
            if (this.CurrentRequest.Id == null)
            {
                CurrentRequest.Created = DateTime.UtcNow;
                CurrentRequest.CreatedBy = App.CurrentUser.Id;
            }
            CurrentRequest.CreatedByUser = null;
            CurrentRequest.Modified = DateTime.UtcNow;
            CurrentRequest.ModifiedBy = App.CurrentUser.Id;
            CurrentRequest.ModifiedByUser = null;
            CurrentRequest.IsActive = true;
            CurrentRequest.RequestStatus = RequestStatus.Open;
            CurrentRequest.RequestType = RequestType.TimeLog;
            CurrentRequest.Deliverable = null;
            CurrentRequest.ProjectID = CurrentRequest.Project?.Id;
            CurrentRequest.Project = null;
            var tl = CurrentRequest.TimeLog;
            if (tl != null)
            {
                if (tl.Id == null)
                {
                    // we'll have to create the time log first.. 
                    tl.Modified = DateTime.UtcNow;
                    tl.Created = DateTime.UtcNow;
                    tl.CreatedBy = App.CurrentUser.Id;
                    tl.ModifiedBy = App.CurrentUser.Id;
                    tl.ProjectID = SelectedProject?.Id ?? 0;
                    tl.DeliverableID = SelectedDeliverable?.Id ?? 0;
                    tl.UserID = App.CurrentUser.Id ?? 0;
                    tl.DisciplineType = this.DisciplineType;
                    tl.DrawingType = this.DrawingType;
                    tl.ScopeType = this.ScopeType;
                    tl.TeamType = this.TeamType;
                    tl.StartDateTime = (this.StartDateTime ?? DateTime.UtcNow).ToUniversalTime();
                    tl.EndDateTime = (this.EndDateTime ?? DateTime.UtcNow).ToUniversalTime();
                    tl.Duration = this.Duration ?? TimeSpan.Zero;
                    tl.IsActive = true;
                    tl.TimeLogStatus = TimeLogStatus.None;
                    tl.Comment = string.Empty;
                    CurrentRequest.IsNewTimeLog = true;

                    var tlResponse = this.timeLogService.InsertTimeLog(tl);
                    if (!tlResponse)
                    {
                        ErrorText += "An error occurred while creating the new time log. Please check the logs";
                        IsBusy = false;
                        return;
                    }
                    else
                    {
                        var tls = this.timeLogService.GetTimeLogsByUserId(App.CurrentUser.Id ?? 0).Where(itm => itm.ProjectID == (SelectedProject?.Id ?? 0) && itm.DeliverableID == (SelectedDeliverable?.Id ?? 0));
                        var maxTime = tls.Max(itm => itm.Created);
                        CurrentRequest.TimeLog = tls.FirstOrDefault(itm => itm.Created == maxTime);
                    }
                }
            }

            if (CurrentRequest.TimeLog != null && SelectedRequestType == RequestType.TimeLog)
            {
                CurrentRequest.TimeLogID = CurrentRequest.TimeLog.Id;
                CurrentRequest.ProjectID = CurrentRequest.TimeLog.ProjectID;
                CurrentRequest.DeliverableID = CurrentRequest.TimeLog.DeliverableID;
                CurrentRequest.TimeLog = null;
            }
            if (this.CurrentRequest.PlanningEngineer != null)
            {
                CurrentRequest.PlanningEngineerID = this.CurrentRequest.PlanningEngineer.Id;
                this.CurrentRequest.PlanningEngineer = null;
            }


            this.CurrentRequest.UserID = App.CurrentUser.Id ?? 0;
            var response = false;

            if (this.CurrentRequest.Id == null)
            {
                tl.User = null;
                tl.CreatedByUser = null;
                tl.ModifiedByUser = null;
                tl.Project = null;
                tl.IsVisibleToUser = false;
                this.timeLogService.UpdateTimeLog(tl);
                response = this.requestService.InsertRequest(CurrentRequest);
            }
            else
            {
                response = this.requestService.UpdateRequest(CurrentRequest);
            }
            if (response)
            {
                this.IsBusy = false;
                this.StartDateTime = null;
                this.EndDateTime = null;
                this.Duration = null;

                CreateToast("Request Submitted", "Your request was successfully submitted");
                CloseDialog();
                LoadRequests();
            }
            else
            {
                CurrentRequest.TimeLog = AvailableTimelogs.FirstOrDefault(itm => itm.Id == CurrentRequest.TimeLogID);
                CurrentRequest.Project = AvailableProjects.FirstOrDefault(itm => itm.Id == CurrentRequest.ProjectID);
                CurrentRequest.PlanningEngineer = AvailablePlanningEngineers.FirstOrDefault(itm => itm.Id == CurrentRequest.PlanningEngineerID);
                this.IsBusy = false;
                CreateToast("Couldn't create your request", "Request creation ran into an error, please try again later.");
            }
        });
    }

    private void UpdateRequestStatus(Request request)
    {
        this.CurrentRequest = request;
        this.CurrentRequest.PendingComment = string.Empty;
        this.UpdateStatus = null;  

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
            this.ErrorText = string.Empty;
            var valid = true;
            var tempText = "The following validation errors were encountered:\n";
            if (!this.AvailableRequestStatuses.Any(itm => itm == UpdateStatus))
            {
                valid = false;
                tempText += "- Updated status was not provided";
            }
            if (!valid)
            {
                this.ErrorText = tempText;
                return;
            }
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
                CurrentRequest.Project.ApprovalState = UpdateStatus;
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
            else if (CurrentRequest.TimeLog != null && UpdateStatus == RequestStatus.Accepted)
            {
                CurrentRequest.TimeLog.CreatedByUser = null;
                CurrentRequest.TimeLog.ModifiedByUser = null;
                CurrentRequest.TimeLog.ModifiedBy = App.CurrentUser.Id;
                CurrentRequest.TimeLog.Modified = DateTime.UtcNow;
                CurrentRequest.TimeLog.StartDateTime = this.StartDateTime ?? DateTime.UtcNow;
                CurrentRequest.TimeLog.EndDateTime = (this.StartDateTime ?? DateTime.UtcNow).Add(CurrentRequest.Timestamp ?? TimeSpan.Zero);
                CurrentRequest.TimeLog.Duration = CurrentRequest.TimeLog.StartDateTime - CurrentRequest.TimeLog.EndDateTime ?? TimeSpan.Zero;
                CurrentRequest.TimeLog.ProjectID = CurrentRequest.TimeLog.Project.Id ?? 0;
                CurrentRequest.TimeLog.IsVisibleToUser = true;
                CurrentRequest.TimeLog.IsNewTimeLog = false;
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
            CurrentRequest.TimeLog = null;
            CurrentRequest.Project = null;
            CurrentRequest.CreatedByUser = null;
            CurrentRequest.ModifiedByUser = null;
            CurrentRequest.RequestStatus = UpdateStatus?? RequestStatus.None;
            CurrentRequest.TimeLog = null;
            var response = this.requestService.UpdateRequest(CurrentRequest);
            if (response)
            {
                var commentResponse = false;
                if (!string.IsNullOrWhiteSpace(UpdateStatusComment))
                {
                    commentResponse = CreateRequestComment(this.UpdateStatusComment, CurrentRequest.Id ?? 0);
                }
                else
                {
                    this.CreateToast("Successfully modified status", "The status was changed successfully");
                    this.CloseDialog();
                    LoadRequests();
                    return;
                }
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
