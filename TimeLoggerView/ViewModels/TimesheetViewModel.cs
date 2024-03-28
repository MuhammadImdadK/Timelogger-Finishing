using Avalonia.Threading;
using Common.Enums;
using DynamicData;
using Model.ModelSql;
using ReactiveUI;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
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
    private TeamType teamType = TeamType.None;
    private ScopeType scopeType = ScopeType.None;
    private DrawingType drawingType = DrawingType.None;
    private DisciplineType disciplineType = DisciplineType.None;
    private string comment;
    private TimeSpan? duration;
    private DateTime? endDateTime;
    private DateTime startDateTime = DateTime.UtcNow;
    private bool isUser;
    private bool canSave;
    private bool isPlanningEngineer;
    private readonly ITimeLogService timesheetService;
    private readonly IProjectService projectService;

    public TimesheetViewModel()
    {
        this.timesheetService = (ITimeLogService)App.Container.GetService(typeof(ITimeLogService));
        this.projectService = (IProjectService)App.Container.GetService(typeof(IProjectService));
        this.IsUser = App.CurrentUser.RoleID == UserRoleId;
        this.IsPlanningEngineer = App.CurrentUser.RoleID == PlanningEngineerRoleId || App.CurrentUser.RoleID == 1;
        this.RestartTimerCommand = ReactiveCommand.Create(RestartTimer);
        this.StartTimerCommand = ReactiveCommand.Create(StartTimer);
        this.StopTimerCommand = ReactiveCommand.Create(StopTimer);
        this.SaveTimeLogCommand = ReactiveCommand.Create(SaveTimeLog);
        this.CloseDialogCommand = ReactiveCommand.Create(CloseDialog);
        this.LoadDataCommand = ReactiveCommand.Create(LoadData);
        this.ShowTimeLoggerCommand = ReactiveCommand.Create(ShowTimeLogger);
        this.LoadData();
    }

    private void ShowTimeLogger()
    {
        if (TimeLoggerWindow.Instance == null)
        {
            var window = new TimeLoggerWindow();
            window.Show();
        }
    }

    private void LoadData()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            this.IsBusy = true;
            this.BusyText = "Fetching Data";
        });

        Task.Run(() =>
        {
            var availableProjects = this.projectService.GetProjects();

            if (this.IsUser)
            {
                availableProjects = availableProjects.Where(itm => itm.IsActive && itm.ApprovalState == Common.Enums.RequestStatus.Accepted).ToList();
            }

            var availableTimeLogs = this.timesheetService.GetTimeLogsByUserId(App.CurrentUser.Id ?? 0);

            foreach(var timelog in availableTimeLogs)
            {
                timelog.Project = availableProjects.FirstOrDefault(itm => itm.Id == timelog.ProjectID) ?? new();
            }

            Dispatcher.UIThread.Invoke(() =>
            {
                this.Projects.Clear();
                this.TimeLogs.Clear();
                this.Projects.AddRange(availableProjects);
                this.TimeLogs.AddRange(availableTimeLogs);
                this.IsBusy = false;
            });
        });
    }

    private void SaveTimeLog()
    {
        this.IsBusy = true;
        this.BusyText = "Inserting Time log";

        var timeLog = new TimeLog()
        {
            Comment = this.Comment,
            Created = DateTime.UtcNow,
            CreatedBy = App.CurrentUser.Id,
            Modified = DateTime.UtcNow,
            ModifiedBy = App.CurrentUser.Id,
            StartDateTime = this.StartDateTime,
            EndDateTime = this.EndDateTime,
            Duration = this.Duration ?? TimeSpan.Zero,
            ProjectID = this.SelectedProject.Id ?? 0,
            TeamType = this.TeamType,
            DisciplineType = this.DisciplineType,
            DrawingType = this.DrawingType,
            ScopeType = this.ScopeType,
            TimeLogStatus = TimeLogStatus.None,
            IsActive = true,
            UserID = App.CurrentUser.Id ?? 0,
        };

        var response = this.timesheetService.InsertTimeLog(timeLog);

        if (response)
        {
            this.LoadData();
            this.CreateToast("Inserted Time Log", "Successfully inserted time log");
            this.OnTriggerWindowClose?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            this.LoadData();
            this.CreateToast("Failed to insert Time Long", "Something went wrong whilst attempting to inserting the time log, please try again later.");
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
        this.InCompactMode = true;
        this.OnCompactModeChanged?.Invoke(this, true);
        this.TimerRunning = true;
        this.Duration = TimeSpan.Zero;
        this.StartDateTime = DateTime.UtcNow;
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += DispatcherTimer_Tick;
        timer.Start();
    }

    private void DispatcherTimer_Tick(object? sender, EventArgs e)
    {
        if (this.Duration != null)
        {
            this.Duration = ((TimeSpan)this.Duration).Add(TimeSpan.FromSeconds(1));
        }
    }

    public ICommand RestartTimerCommand { get; }
    public ICommand StartTimerCommand { get; }
    public ICommand StopTimerCommand { get; }
    public ICommand SaveTimeLogCommand { get; }
    public ICommand CloseDialogCommand { get; }
    public ICommand LoadDataCommand { get; }
    public ICommand ShowTimeLoggerCommand { get; }
    public bool IsUser { get => this.isUser; set => this.RaiseAndSetIfChanged(ref this.isUser, value); }
    public bool IsPlanningEngineer { get => this.isPlanningEngineer; set => this.RaiseAndSetIfChanged(ref this.isPlanningEngineer, value); }
    public ObservableCollection<Project> Projects { get; set; } = new ObservableCollection<Project>();
    public ObservableCollection<TimeLog> TimeLogs { get; set; } = new ObservableCollection<TimeLog>();
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
    public DateTimeOffset StartDateTimeOffset { get => new DateTimeOffset(this.StartDateTime); set => this.StartDateTime = value.UtcDateTime; }
    public DateTime StartDateTime { get => startDateTime; set => this.RaiseAndSetIfChanged(ref this.startDateTime, value); }
    public DateTime? EndDateTime { get => endDateTime; set => this.RaiseAndSetIfChanged(ref this.endDateTime, value); }
    public TimeSpan? Duration
    {
        get => duration;
        set
        {
            this.RaiseAndSetIfChanged(ref this.duration, value);
            if (value != null)
            {
                this.TimerString = ((TimeSpan)value).ToString(@"hh\:mm\:ss");
                this.CanSave = true;
            } else
            {
                this.CanSave = false;
            }
        }
    }
    public string Comment { get => comment; set => this.RaiseAndSetIfChanged(ref this.comment, value); }
    public List<DisciplineType> AvailableDisciplineTypes { get; } = new List<DisciplineType>()
    {
        DisciplineType.None,
        DisciplineType.Process,
        DisciplineType.Piping,
        DisciplineType.IE,
        DisciplineType.Civil
    };
    public DisciplineType DisciplineType { get => disciplineType; set => this.RaiseAndSetIfChanged(ref this.disciplineType, value); }
    public List<DrawingType> AvailableDrawingTypes { get; } = new List<DrawingType>()
    {
        DrawingType.None,
        DrawingType.PipingAndInstrimentDiagram,
        DrawingType.CauseAndEffect,
        DrawingType.ProcessFlowDiagram,
        DrawingType.MasterEquipmentList,
        DrawingType.MasterLinetList,
        DrawingType.Calculation,
        DrawingType.Report
    };
    public DrawingType DrawingType { get => drawingType; set => this.RaiseAndSetIfChanged(ref this.drawingType, value); }
    public List<ScopeType> AvailableScopeTypes { get; } = new List<ScopeType>()
    {
        ScopeType.None,
        ScopeType.Installation,
        ScopeType.Demolation,
        ScopeType.AsBuilt,
        ScopeType.Relocation,
        ScopeType.Standard
    };
    public ScopeType ScopeType { get => scopeType; set => this.RaiseAndSetIfChanged(ref this.scopeType, value); }
    public List<TeamType> AvailableTeamTypes { get; } = new List<TeamType>()
    {
        TeamType.None,
        TeamType.CoreTeam,
        TeamType.AdditionalTeam
    };
    public TeamType TeamType { get => teamType; set => this.RaiseAndSetIfChanged(ref this.teamType, value); }
    public Project SelectedProject { get => selectedProject; set => this.RaiseAndSetIfChanged(ref this.selectedProject, value); }
}
