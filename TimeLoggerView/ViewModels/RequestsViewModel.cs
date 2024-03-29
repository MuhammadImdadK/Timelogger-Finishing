using Avalonia.Threading;
using Common.Enums;
using DynamicData;
using Model.ModelSql;
using ReactiveUI;
using Service.Interface;
using SukiUI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    private bool isUser;
    private bool isPlanningEngineer;
    private Request currentRequest;
    private RequestStatus updateStatus;
    private string updateStatusComment;
    private bool isAddingEndTime = true;
    private RequestType selectedRequestType = RequestType.TimeLog;
    private bool isAddingProjectRequest = false;
    private string submitButtonText;

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
    public RequestStatus UpdateStatus { get => updateStatus; set => this.RaiseAndSetIfChanged(ref this.updateStatus, value); }
    public string UpdateStatusComment { get => updateStatusComment; set => this.RaiseAndSetIfChanged(ref this.updateStatusComment, value); }
    public string SubmitButtonText { get => submitButtonText; set => this.RaiseAndSetIfChanged(ref this.submitButtonText, value); }
    public Request CurrentRequest { get => currentRequest; set => this.RaiseAndSetIfChanged(ref this.currentRequest, value); }

    public ICommand ReloadRequestsCommand { get; }
    public ICommand CloseDialogCommand { get; }


    public ICommand CreateRequestCommand { get; }
    public ICommand EditRequestCommand { get; }
    public ICommand SubmitCreateRequestCommand { get; }
    public ICommand UpdateRequestStatusCommand { get; }
    public ICommand SubmitUpdateRequestStatusCommand { get; }
    public ICommand PostCommentCommand { get; }
    public bool IsAddingEndTime { get => this.isAddingEndTime; set => this.RaiseAndSetIfChanged(ref this.isAddingEndTime, value); }

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
        this.EditRequestCommand = ReactiveCommand.Create<Request>(EditRequest);
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
        ErrorText = string.Empty;
        this.SubmitButtonText = "Create Request";
        this.CurrentRequest = new();
        this.IsAddingEndTime = true;

        var view = new CreateRequestView()
        {
            DataContext = this
        };
        SukiHost.ShowDialog(App.WorkspaceInstance, view, allowBackgroundClose: false);
    }

    private void EditRequest(Request request)
    {
        ErrorText = string.Empty;
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
            if(this.CurrentRequest.PlanningEngineer == null)
            {
                valid = false;
                tempText += "- A planning engineer must be assigned\n";
            }
            if(this.CurrentRequest.Timestamp == null || this.CurrentRequest.Timestamp == TimeSpan.Zero)
            {
                valid = false;
                tempText += "- A duration must be provided\n";
            }
            if(this.CurrentRequest.TimeLog == null)
            {
                valid = false;
                tempText += "- The time log entry to modify must be provided\n";
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

            CurrentRequest.ProjectID = CurrentRequest.Project?.Id;
            CurrentRequest.Project = null;
            if (CurrentRequest.TimeLog != null && SelectedRequestType == RequestType.TimeLog)
            {
                CurrentRequest.TimeLogID = CurrentRequest.TimeLog.Id;
                CurrentRequest.ProjectID = CurrentRequest.TimeLog.ProjectID;
                CurrentRequest.TimeLog = null;
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
            var response = false;

            if (this.CurrentRequest.Id == null)
            {
                response = this.requestService.InsertRequest(CurrentRequest);
            } 
            else
            {
                response = this.requestService.UpdateRequest(CurrentRequest);
            }

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
            CurrentRequest.TimeLog = null;
            CurrentRequest.Project = null;
            CurrentRequest.CreatedByUser = null;
            CurrentRequest.ModifiedByUser = null;
            CurrentRequest.RequestStatus = UpdateStatus;
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
