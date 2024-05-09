using Autofac;
using Avalonia.Controls;
using Avalonia.Threading;
using Common.Enums;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Model.ModelSql;
using Newtonsoft.Json.Linq;
using Pec.ProjectManagement.Ui.Views;
using ReactiveUI;
using ReportGenerator.EmailReporting;
using ReportGenerator.EmailReporting.Configuration;
using Service.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TimeLoggerView.ViewModels;


public class ReportsViewModel : ModuleViewModel
{
    private DateTimeOffset reportDate = DateTimeOffset.Now;
    private int selectedReport;
    private int selectedTeam;
    private bool isReportLoaded;
    private string reportString;
    private ComboBoxItem selectedReportText;
    private ComboBoxItem selectedTeamText;
    private DateTimeOffset reportToDate;
    private bool isDataAvailable;
    private readonly ITimeLogService timelogService;
    private readonly IUserService userService;
    private readonly IProjectService projectService;
    private readonly IAttachmentService? activityService;
    private readonly IActivityTypeService activityTypeService;
    private readonly IDeliverableDrawingTypeService deliverableService;
    private readonly IDesignationService designationService;
    private readonly IDesignationRateService designationRateService;
    private readonly ILogger<ReportsViewModel> logger;
    private const string CacheDataFolder = "InformaticaSystems";
    private ReportData reportData;
    private string reportHtml;
    public ICommand ReloadRequestsCommand { get; }
    public ICommand ExportToExcelCommand { get; }
    public bool IsReportLoaded { get => isReportLoaded; set => this.RaiseAndSetIfChanged(ref isReportLoaded, value); }
    public string ReportString { get => reportString; set => this.RaiseAndSetIfChanged(ref reportString, value); }
    public DateTimeOffset ReportDate
    {
        get => reportDate; set
        {
            var newValue = value.AddDays(value.Day > 1 ? (value.Day * -1) + 1 : 0).Date;
            this.RaiseAndSetIfChanged(ref reportDate, newValue);
        }
    }
    public DateTimeOffset ReportToDate { get => reportToDate; set => this.RaiseAndSetIfChanged(ref reportToDate, value); }
    public int SelectedReport { get => selectedReport; set => this.RaiseAndSetIfChanged(ref selectedReport, value); }
    public int SelectedTeam { get => selectedTeam; set => this.RaiseAndSetIfChanged(ref selectedTeam, value); }
    public ComboBoxItem SelectedReportText { get => selectedReportText; set => this.RaiseAndSetIfChanged(ref selectedReportText, value); }
    public ComboBoxItem SelectedTeamText { get => selectedTeamText; set => this.RaiseAndSetIfChanged(ref selectedTeamText, value); }
    public bool IsDataAvailable { get => isDataAvailable; set => this.RaiseAndSetIfChanged(ref isDataAvailable, value); }
    public ReportData ReportData { get => reportData; set => this.RaiseAndSetIfChanged(ref reportData, value); }
    public string ReportHtml { get => reportHtml; set => this.RaiseAndSetIfChanged(ref reportHtml, value); }
    public ReportsViewModel()
    {
        this.timelogService = (ITimeLogService)App.Container.GetService(typeof(ITimeLogService));
        this.userService = (IUserService)App.Container.GetService(typeof(IUserService));
        this.projectService = (IProjectService)App.Container.GetService(typeof(IProjectService));
        this.activityService = (IAttachmentService)App.Container.GetService(typeof(IAttachmentService));
        this.activityTypeService = (IActivityTypeService)App.Container.GetService(typeof(IActivityTypeService));
        this.deliverableService = (IDeliverableDrawingTypeService)App.Container.GetService(typeof(IDeliverableDrawingTypeService));
        this.designationService = (IDesignationService)App.Container.GetService(typeof(IDesignationService));
        this.designationRateService = (IDesignationRateService)App.Container.GetService(typeof(IDesignationRateService));
        this.logger = (ILogger<ReportsViewModel>)App.Container.GetService<ILogger<ReportsViewModel>>();
        this.ReloadRequestsCommand = ReactiveCommand.Create(LoadReports);
        this.ExportToExcelCommand = ReactiveCommand.Create(ExportToExcel);
    }

    private void LoadReports()
    {
        this.ReportDate = this.ReportDate;
        this.ReportToDate = ReportDate.AddMonths(1).ToLocalTime().Date.AddMinutes(-1);

        this.IsReportLoaded = true;
        this.ReportString = $"{SelectedReportText.Content.ToString()} report for {SelectedTeamText.Content.ToString()} {ReportDate.Date.ToLocalTime().ToString("f")}-{ReportToDate.Date.ToLocalTime().ToString("f")}";

        Task.Run(async () =>
        {
            IsBusy = true;
            IsDataAvailable = true;
            BusyText = "The report is being loaded";
            var teamType = (TeamType)SelectedTeam + 1;
            var users = this.userService.GetUsers().Where(itm => itm.TeamType == teamType).ToList();
            var timeLogs = this.timelogService.GetTimeLogs().Where(itm => users.Any(usr => usr.Id == itm.UserID) &&
                itm.StartDateTime >= ReportDate && itm.EndDateTime <= ReportToDate);
            if (!timeLogs.Any())
            {
                IsBusy = false;
                BusyText = string.Empty;
                IsDataAvailable = false;
                return;
            }
            var projects = this.projectService.GetProjects().Where(itm => timeLogs.Any(log => log.ProjectID == itm.Id));
            var activities = this.activityService.GetDrawings().Where(itm => projects.Any(proj => proj.Id == itm.ProjectId));
            var activityTypes = this.activityTypeService.GetActivityTypes().Where(itm => activities.Any(log => log.ActivityTypeId == itm.Id));
            var deliverables = this.deliverableService.GetDeliverableDrawingTypes().Where(itm => timeLogs.Any(log => log.DeliverableDrawingTypeID == itm.Id));
            var designations = this.designationService.GetAllDesignations().Where(itm => users.Any(user => user.DesignationID == itm.Id));
            var designationRates = this.designationRateService.GetAllDesignationsRates().Where(itm => designations.Any(des => des.Id == itm.DesignationID));


            this.ReportData = new ReportData()
            {
                Month = this.ReportDate.ToString("MMMM - yyyy"),
                StartDate = this.ReportDate,
                TeamType = teamType,
                Users = users,
                TimeLogs = timeLogs,
                Projects = projects,
                Activities = activities,
                ActivityTypes = activityTypes,
                Deliverables = deliverables,
                Designations = designations,
                DesignationRates = designationRates
            };
            await GenerateReportHtml();

        });
    }

    private async Task GenerateReportHtml()
    {
        this.logger.LogInformation($"Loading report generator");

        var application = Program.CompositionRoot().Resolve<Application>();
        var emailSettings = Program.CompositionRoot().Resolve<IConfigurationRoot>();
        var smtpConfig = emailSettings.GetSection("SmtpConfiguration").Get<SmtpConfiguration>();

        this.logger.LogInformation("Generation Starting.");
        try
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            if (!Directory.Exists(Path.Join(appData, CacheDataFolder)))
            {
                Directory.CreateDirectory(Path.Combine(appData, CacheDataFolder));
            }

            var outputFile = Path.Join(appData, CacheDataFolder, $"{Guid.NewGuid()}.json");
            File.WriteAllText(outputFile, JsonSerializer.Serialize(ReportData));

            // we don't care about the email functionality but it is possible to do
            // so keeping in case needed in the future 
            //if (smtpConfig == null)
            //{
            //    throw new InvalidOperationException("SMTP Config was not sent.");
            //}

            await Dispatcher.UIThread.Invoke(async () =>
            {
                await application.Run(new GenerationOptions
                {
                    EmailSubject = string.Empty,
                    Input = new FileInfo(outputFile),
                    Output = string.Empty, // if email report required -- fetch the emails from user tabel && join by ; for the users we need to send to
                    OutputType = "email",
                    TemplateName = SelectedReportText.Content.ToString(),
                    JobName = SelectedReportText.Content.ToString(),
                });
                this.logger.LogInformation("Generation complete");
                this.ReportHtml = application.Result;
                IsDataAvailable = true;
                IsBusy = false;
                BusyText = string.Empty;

                if(File.Exists(outputFile))
                {
                    File.Delete(outputFile);
                }
            });

        }
        catch (Exception ex)
        {
            this.logger.LogInformation($"Generation failed {ex.Message}");
        }
    }

    private void ExportToExcel()
    {
        if (!this.IsDataAvailable)
        {
            this.CreateToast("Cannot Export Data", $"There is no available data for the following report: \"{ReportString}\"");
            return;
        }
    }
}
