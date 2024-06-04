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
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TimeLoggerView.Views;
using TimeLoggerView.Views.Timesheet;

namespace TimeLoggerView.ViewModels;

public class TimesheetViewModel : ModuleViewModel
{
    private DispatcherTimer timer = new DispatcherTimer();
    private string errorText;
    public const int UserRoleId = 3;
    public const int PlanningEngineerRoleId = 2;
    private bool inCompactMode = false;
    private bool enteringTimeManually = false;
    private bool timerRunning = false;
    private string timerString = string.Empty;
    public EventHandler<bool> OnCompactModeChanged;
    public EventHandler OnTriggerWindowClose;
    private Project selectedProject;
    private Common.Enums.TeamType? teamType = Common.Enums.TeamType.None;
    private Common.Enums.ScopeType? scopeType = Common.Enums.ScopeType.None;
    private Common.Enums.DisciplineType? disciplineType = Common.Enums.DisciplineType.None;
    private string comment;
    private TimeSpan? duration;
    private DateTime? endDateTime;
    private DateTime? startDateTime = DateTime.Now;
    private bool isUser;
    private bool canSave;
    private bool isPlanningEngineer;
    private bool canRunTimeRecorder = true;
    private readonly ITimeLogService timesheetService;
    private readonly IProjectService projectService;
    private readonly IUserService userService;
    private readonly IAttachmentService attachmentService;
    private readonly IDeliverableDrawingTypeService deliverableService;
    private TimeLog? currentTimeLog = null;
    private string endDateLocalTime = "Unknown";
    private Drawing? selectedDeliverable;
    private DeliverableDrawingType? selectedDeliverableType;

    public TimesheetViewModel()
    {
        this.timesheetService = (ITimeLogService)App.Container.GetService(typeof(ITimeLogService));
        this.projectService = (IProjectService)App.Container.GetService(typeof(IProjectService));
        this.userService = (IUserService)App.Container.GetService(typeof(IUserService));
        this.attachmentService = (IAttachmentService)App.Container.GetService(typeof(IAttachmentService));
        this.deliverableService = (IDeliverableDrawingTypeService)App.Container.GetService(typeof(IDeliverableDrawingTypeService));
        this.IsUser = App.CurrentUser.RoleID == UserRoleId;
        this.IsPlanningEngineer = App.CurrentUser.RoleID == PlanningEngineerRoleId || App.CurrentUser.RoleID == 1;
        this.RestartTimerCommand = ReactiveCommand.Create(RestartTimer);
        this.StartTimerCommand = ReactiveCommand.Create(StartTimer);
        this.StopTimerCommand = ReactiveCommand.Create(StopTimer);
        this.SaveTimeLogCommand = ReactiveCommand.Create(SaveTimeLog);
        this.CloseDialogCommand = ReactiveCommand.Create(CloseDialog);
        this.LoadDataCommand = ReactiveCommand.Create(LoadData);
        this.ShowTimeLoggerCommand = ReactiveCommand.Create(ShowTimeLogger);
        this.EditTimeLogCommand = ReactiveCommand.Create<TimeLog>(EditTimeLog);
        this.LoadData();
    }

    private void EditTimeLog(TimeLog log)
    {
        this.ErrorText = string.Empty;
        this.CurrentTimeLog = log;
        this.CanRunTimeRecorder = false;
        this.Comment = log.Comment ?? string.Empty;
        this.StartDateTime = log.StartDateTime.ToLocalTime();
        this.EndDateTime = log.EndDateTime != null ? ((DateTime)log.EndDateTime).ToLocalTime() : null;
        this.Duration = log.Duration;
        this.SelectedProject = this.Projects.FirstOrDefault(itm => itm.Id == log.ProjectID) ?? new();
        this.TeamType = log.TeamType;
        this.DisciplineType = log.DisciplineType;
        this.ScopeType = log.ScopeType;
        this.SelectedDeliverable = log.Deliverable;
        this.SelectedDeliverableType = this.AvailableDeliverableTypes.FirstOrDefault(itm => itm.Id == log.DeliverableDrawingTypeID);
        var deliverables = this.attachmentService.GetDrawingsByProjectId(this.SelectedProject.Id ?? 0);
        this.SelectedDeliverable = deliverables.FirstOrDefault(itm => itm.Id == log.DeliverableID);
        this.AvailableDeliverables.AddRange(deliverables);

        var view = new TimesheetEditorView()
        {
            DataContext = this,
        };

        SukiHost.ShowDialog(App.WorkspaceInstance,content: view, allowBackgroundClose: false);
    }

    private void ShowTimeLogger()
    {
        LoadData();
        this.ErrorText = string.Empty;

        if (TimeLoggerWindow.Instance == null)
        {
            MainViewModel.TimesheetManagement.CanRunTimeRecorder = true;
            MainViewModel.TimesheetManagement.TeamType = App.CurrentUser.TeamType;
            MainViewModel.TimesheetManagement.SelectedDeliverableType = null;
            MainViewModel.TimesheetManagement.TimerString = "00:00:00";
            var window = new TimeLoggerWindow();
            window.Show();
        }
        else
        {
            this.CreateToast("Time logger open", "The time logger window is already open.");
        }
    }

    public void LoadData()
    {
        Project? reattach = null;
        Drawing? reattachDeliverable = null;
        DeliverableDrawingType? reattachDeliverableType = null;
        if (SelectedProject != null)
        {
            reattach = SelectedProject;
        }
        if (SelectedDeliverable != null)
        {
            reattachDeliverable = SelectedDeliverable;
        }
        if (SelectedDeliverableType != null)
        {
            reattachDeliverableType = SelectedDeliverableType;
        }

        Dispatcher.UIThread.Invoke(() =>
        {
            this.IsBusy = true;
            this.BusyText = "Fetching Data";
        });

        Task.Run(() =>
        {
            var availableUsers = this.userService.GetUsers();
            var availableProjects = ((IProjectService)App.Container.GetService(typeof(IProjectService))).GetProjects();
            var availableDeliverables = this.attachmentService.GetDrawings();
            var availableDeliverableDrawingTypes = this.deliverableService.GetDeliverableDrawingTypes();

            //if (this.IsUser)
            //{
            //    availableProjects = availableProjects.Where(itm => itm.IsActive && itm.ApprovalState == Common.Enums.RequestStatus.Accepted).ToList();
            //}

            var availableTimeLogs = new List<TimeLog>();
            if (this.IsPlanningEngineer)
            {
                availableTimeLogs = this.timesheetService.GetTimeLogs();
            }
            else
            {
                availableTimeLogs = this.timesheetService.GetTimeLogsByUserId(App.CurrentUser.Id ?? 0).Where(itm => itm.IsVisibleToUser).ToList();
            }

            foreach (var timelog in availableTimeLogs)
            {
                timelog.Project = availableProjects.FirstOrDefault(itm => itm.Id == timelog.ProjectID) ?? new();
                timelog.User = availableUsers.FirstOrDefault(itm => itm.Id == timelog.UserID) ?? new();
                timelog.Deliverable = availableDeliverables.FirstOrDefault(itm => itm.Id == timelog.DeliverableID) ?? new();
                timelog.DeliverableDrawingType = availableDeliverableDrawingTypes.FirstOrDefault(itm => itm.Id == timelog.DeliverableDrawingTypeID) ?? new();
            }

            Dispatcher.UIThread.Invoke(() =>
            {
                this.Projects.Clear();
                this.TimeLogs.Clear();
                this.Users.Clear();
                this.AvailableDeliverables.Clear();
                this.AvailableDeliverables.Add(availableDeliverables);
                this.AvailableDeliverableTypes.Clear();
                this.AvailableDeliverableTypes.AddRange(availableDeliverableDrawingTypes.Where(itm => itm.IsActive));
                this.Projects.AddRange(availableProjects.Where(itm => itm.IsActive));
                this.TimeLogs.AddRange(availableTimeLogs);
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
                if(reattachDeliverableType != null)
                {
                    this.SelectedDeliverableType = this.AvailableDeliverableTypes.FirstOrDefault(itm => itm.Id == reattachDeliverableType.Id) ?? new();
                }
            });
        });
    }

    private void SaveTimeLog()
    {
        this.ErrorText = string.Empty;
        var tempText = "The following validation errors were encountered:\n";
        var valid = true;
        this.IsBusy = true;
        this.BusyText = "Inserting Time log";

        var timeLog = new TimeLog();

        Duration = TimeSpan.FromSeconds(Math.Truncate(Duration.Value.TotalSeconds));
        if (this.CurrentTimeLog == null)
        {

            timeLog = new TimeLog()
            {
                Comment = this.Comment,
                Created = DateTime.UtcNow,
                CreatedBy = App.CurrentUser.Id,
                Modified = DateTime.UtcNow,
                ModifiedBy = App.CurrentUser.Id,
                StartDateTime = StartDateTime != null ? ((DateTime)StartDateTime).ToUniversalTime() : DateTime.UtcNow,
                EndDateTime = EndDateTime != null ? ((DateTime)EndDateTime).ToUniversalTime() : DateTime.UtcNow,
                Duration = this.Duration ?? TimeSpan.Zero,
                ProjectID = this.SelectedProject?.Id ?? 0,
                TeamType = this.TeamType,
                DisciplineType = this.DisciplineType,
                ScopeType = this.ScopeType,
                TimeLogStatus = TimeLogStatus.None,
                IsActive = true,
                UserID = App.CurrentUser.Id ?? 0,
                DeliverableID = SelectedDeliverable?.Id ?? 0,
                DeliverableDrawingTypeID = SelectedDeliverableType.Id ?? 0,
                IsVisibleToUser = true,
                IsNewTimeLog = false,
            };
        }
        else
        {
            timeLog = this.CurrentTimeLog;
            timeLog.Comment = this.Comment;
            timeLog.Modified = DateTime.UtcNow;
            timeLog.ModifiedBy = App.CurrentUser.Id;
            timeLog.StartDateTime = StartDateTime != null ? ((DateTime)StartDateTime).ToUniversalTime() : DateTime.UtcNow;
            timeLog.EndDateTime = EndDateTime != null ? ((DateTime)EndDateTime).ToUniversalTime() : DateTime.UtcNow;
            timeLog.Duration = this.Duration ?? TimeSpan.Zero;
            timeLog.ProjectID = this.SelectedProject?.Id ?? 0;
            timeLog.TeamType = this.TeamType;
            timeLog.DisciplineType = this.DisciplineType;
            timeLog.ScopeType = this.ScopeType;
            timeLog.TimeLogStatus = TimeLogStatus.None;
            timeLog.DeliverableDrawingTypeID = SelectedDeliverableType?.Id ?? 0;
            timeLog.DeliverableID = SelectedDeliverable?.Id ?? 0;
        }

        if (this.SelectedProject == null)
        {
            tempText += "- A Project must be selected\n";
            valid = false;
        }
        if (this.SelectedDeliverable == null)
        {
            tempText += "- An Activity must be selected\n";
            valid = false;
        }
        if (this.SelectedDeliverableType == null)
        {
            tempText += "- A valid deliverable must be selected\n";
            valid = false;
        }
        if (this.TeamType == null || this.TeamType == Common.Enums.TeamType.None)
        {
            tempText += "- A team must be selected\n";
            valid = false;
        }
        if (this.StartDateTimeOffset == null || this.StartDateTime == null)
        {
            tempText += "- Start date must be set\n";
            valid = false;
        }
        if (this.Duration == null || this.Duration == TimeSpan.Zero)
        {
            tempText += "- Duration must be set\n";
            valid = false;
        }

        if (!valid)
        {
            this.ErrorText = tempText;
            this.IsBusy = false;
            return;
        }


        var response = false;

        if (timeLog.Id == null)
        {
            response = this.timesheetService.InsertTimeLog(timeLog);
        }
        else
        {
            response = this.timesheetService.UpdateTimeLog(timeLog);
        }

        if (response)
        {
            this.LoadData();
            this.CreateToast("Inserted Time Log", "Successfully inserted time log");
            if (this.CanRunTimeRecorder)
            {
                if (App.WorkspaceInstance.DataContext is MainViewModel mvm)
                {
                    mvm.TimesheetModel.LoadData();
                    mvm.RequestsViewModel.ReloadRequestsCommand.Execute(Unit.Default);
                }
                this.OnTriggerWindowClose?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                this.CloseDialog();
            }
        }
        else
        {
            this.LoadData();
            this.CreateToast("Failed to insert Time Log", "Something went wrong whilst attempting to inserting the time log, please try again later.");
        }
    }
    private void RestartTimer()
    {
        this.Duration = null;
        this.EndDateTime = null;
        this.TimerString = "00:00:00";
    }
    private void StopTimer()
    {
        this.InCompactMode = false;
        this.OnCompactModeChanged?.Invoke(this, false);
        this.TimerRunning = false;
        this.EndDateTime = DateTime.UtcNow;
        timer.Tick -= DispatcherTimer_Tick;
        timer.Stop();
    }

    private void StartTimer()
    {
        this.ErrorText = string.Empty;
        var tempText = "The following validation errors were encountered:\n";
        var valid = true;

        if (this.SelectedProject == null)
        {
            tempText += "- A Project must be selected\n";
            valid = false;
        }
        if (this.SelectedDeliverable == null)
        {
            tempText += "- An Activity must be selected\n";
            valid = false;
        }
        if (this.SelectedDeliverableType == null)
        {
            tempText += "- A valid deliverable must be selected\n";
            valid = false;
        }
        if (this.TeamType == null || this.TeamType == Common.Enums.TeamType.None)
        {
            tempText += "- A team must be selected\n";
            valid = false;
        }


        if (!valid)
        {
            this.ErrorText = tempText;
            return;
        }

        this.InCompactMode = true;
        this.OnCompactModeChanged?.Invoke(this, true);
        this.TimerRunning = true;
        this.Duration = TimeSpan.Zero;
        this.StartDateTime = DateTime.Now;
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += DispatcherTimer_Tick;
        timer.Start();

    }

    private void DispatcherTimer_Tick(object? sender, EventArgs e)
    {
        if (this.Duration != null)
        {
            this.Duration = DateTime.UtcNow.Subtract(((DateTime)StartDateTime).ToUniversalTime());// ((TimeSpan)this.Duration).Add(TimeSpan.FromSeconds(1));
            if (Duration.Value.Seconds %10==0)
            {
                Duration = TimeSpan.FromSeconds(Math.Truncate(Duration.Value.TotalSeconds));
                var timeLog = new TimeLog();

                if (this.CurrentTimeLog == null)
                {
                    timeLog = new TimeLog()
                    {
                        Comment = this.Comment,
                        Created = DateTime.UtcNow,
                        CreatedBy = App.CurrentUser.Id,
                        Modified = DateTime.UtcNow,
                        ModifiedBy = App.CurrentUser.Id,
                        StartDateTime = StartDateTime != null ? ((DateTime)StartDateTime).ToUniversalTime() : DateTime.UtcNow,
                        EndDateTime = EndDateTime != null ? ((DateTime)EndDateTime).ToUniversalTime() : DateTime.UtcNow,
                        Duration = this.Duration ?? TimeSpan.Zero,
                        ProjectID = this.SelectedProject?.Id ?? 0,
                        TeamType = this.TeamType,
                        DisciplineType = this.DisciplineType,
                        ScopeType = this.ScopeType,
                        TimeLogStatus = TimeLogStatus.None,
                        IsActive = true,
                        UserID = App.CurrentUser.Id ?? 0,
                        DeliverableID = SelectedDeliverable?.Id ?? 0,
                        DeliverableDrawingTypeID = SelectedDeliverableType.Id ?? 0,
                        IsVisibleToUser = true,
                        IsNewTimeLog = false,
                    };

                    this.timesheetService.InsertTimeLog(timeLog);
                }
                else
                {
                    timeLog = this.CurrentTimeLog;
                    timeLog.Comment = this.Comment;
                    timeLog.Modified = DateTime.UtcNow;
                    timeLog.ModifiedBy = App.CurrentUser.Id;
                    timeLog.StartDateTime = StartDateTime != null ? ((DateTime)StartDateTime).ToUniversalTime() : DateTime.UtcNow;
                    timeLog.EndDateTime = EndDateTime != null ? ((DateTime)EndDateTime).ToUniversalTime() : DateTime.UtcNow;
                    timeLog.Duration = this.Duration ?? TimeSpan.Zero;
                    timeLog.ProjectID = this.SelectedProject?.Id ?? 0;
                    timeLog.TeamType = this.TeamType;
                    timeLog.DisciplineType = this.DisciplineType;
                    timeLog.ScopeType = this.ScopeType;
                    timeLog.TimeLogStatus = TimeLogStatus.None;
                    timeLog.DeliverableDrawingTypeID = SelectedDeliverableType?.Id ?? 0;
                    timeLog.DeliverableID = SelectedDeliverable?.Id ?? 0;

                    this.timesheetService.UpdateTimeLog(timeLog);
                }
                CurrentTimeLog = timeLog;
            }
        }
    }

    public ICommand RestartTimerCommand { get; }
    public ICommand StartTimerCommand { get; }
    public ICommand StopTimerCommand { get; }
    public ICommand SaveTimeLogCommand { get; }
    public ICommand EditTimeLogCommand { get; }
    public ICommand CloseDialogCommand { get; }
    public ICommand LoadDataCommand { get; }
    public ICommand ShowTimeLoggerCommand { get; }
    public bool IsUser { get => this.isUser; set => this.RaiseAndSetIfChanged(ref this.isUser, value); }
    public bool IsPlanningEngineer { get => this.isPlanningEngineer; set => this.RaiseAndSetIfChanged(ref this.isPlanningEngineer, value); }
    public bool CanRunTimeRecorder { get => this.canRunTimeRecorder; set => this.RaiseAndSetIfChanged(ref this.canRunTimeRecorder, value); }
    public ObservableCollection<Project> Projects { get; set; } = new ObservableCollection<Project>();
    public ObservableCollection<TimeLog> TimeLogs { get; set; } = new ObservableCollection<TimeLog>();
    public ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();
    public ObservableCollection<Drawing> AvailableDeliverables { get; set; } = new ObservableCollection<Drawing>();
    public Drawing? SelectedDeliverable { get => selectedDeliverable; set => this.RaiseAndSetIfChanged(ref selectedDeliverable, value); }
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
    public bool InCompactMode { get => this.inCompactMode; set => this.RaiseAndSetIfChanged(ref this.inCompactMode, value); }
    public bool CanSave { get => this.canSave; set => this.RaiseAndSetIfChanged(ref this.canSave, value); }
    public bool EnteringTimeManually { get => this.enteringTimeManually; set => this.RaiseAndSetIfChanged(ref this.enteringTimeManually, value); }
    public bool TimerRunning { get => this.timerRunning; set => this.RaiseAndSetIfChanged(ref this.timerRunning, value); }
    public string TimerString { get => this.timerString; set => this.RaiseAndSetIfChanged(ref this.timerString, value); }
    public TimeLog CurrentTimeLog { get => this.currentTimeLog; set => this.RaiseAndSetIfChanged(ref this.currentTimeLog, value); }
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
            if (this.EnteringTimeManually)
            {
                this.EndDateTimeOffset = value;
            }
        }
    }
    public DateTime? StartDateTime
    {
        get => startDateTime;
        set
        {
            this.RaiseAndSetIfChanged(ref this.startDateTime, value);
            if (EnteringTimeManually)
            {
                this.EndDateTime = value;
                this.EndDateLocalTime = EndDateTime != null
                    ? ((DateTime)EndDateTime).ToLocalTime().ToString("f", CultureInfo.CurrentCulture)
                    : "Unknown";
            }
        }
    }
    public DateTime? EndDateTime
    {
        get => endDateTime;
        set
        {
            if (StartDateTime != null && Duration != null)
            {
                this.RaiseAndSetIfChanged(ref this.endDateTime, ((DateTime)this.StartDateTime).Add((TimeSpan)Duration));
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
            if (value != null && StartDateTime != null && this.EnteringTimeManually)
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
            if (value != null)
            {
                this.TimerString = ((TimeSpan)value).ToString(@"hh\:mm\:ss");
                this.EndDateTime = DateTime.UtcNow;
                this.CanSave = true;
            }
            else
            {
                this.CanSave = false;
            }
        }
    }
    public string Comment { get => comment; set => this.RaiseAndSetIfChanged(ref this.comment, value); }
    public List<DisciplineType> AvailableDisciplineTypes { get; } = new List<DisciplineType>()
    {
        Common.Enums.DisciplineType.None,
        Common.Enums.DisciplineType.Process,
        Common.Enums.DisciplineType.Piping,
        Common.Enums.DisciplineType.IE,
        Common.Enums.DisciplineType.Civil
    };
    public DisciplineType? DisciplineType { get => disciplineType; set => this.RaiseAndSetIfChanged(ref this.disciplineType, value); }
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
    public ScopeType? ScopeType { get => scopeType; set => this.RaiseAndSetIfChanged(ref this.scopeType, value); }
    public List<TeamType> AvailableTeamTypes { get; } = new List<TeamType>()
    {
        Common.Enums.TeamType.None,
        Common.Enums.TeamType.CoreTeam,
        Common.Enums.TeamType.AdditionalTeam
    };
    public TeamType? TeamType { get => teamType; set => this.RaiseAndSetIfChanged(ref this.teamType, value); }
    public Project SelectedProject
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

    public ObservableCollection<DeliverableDrawingType> AvailableDeliverableTypes { get; set; } = new();
    public DeliverableDrawingType? SelectedDeliverableType { get => selectedDeliverableType; set => this.RaiseAndSetIfChanged(ref this.selectedDeliverableType, value); }
}
