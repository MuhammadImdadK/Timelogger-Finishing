using Avalonia.Collections;
using Avalonia.Media;
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
        Drawings = new List<Model.ModelSql.Drawing>()
        {
            new()
            {
                Name = "Name",
                Description = "Description",
            }
        }
    };

    public Project CurrentProject { get => this.currentProject; set => this.RaiseAndSetIfChanged(ref this.currentProject, value); }

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

    public bool IsTicketApproved => CurrentProject.ApprovalState == Common.Enums.ApprovalState.Accepted;
    public bool CanSubmit => CurrentProject.ManhourBudget > 0 && CurrentProject.ApprovalState == null;

    public string ApprovalState => CurrentProject.ApprovalState switch
    {
        Common.Enums.ApprovalState.AwaitingApproval => "Awaiting Approval",
        Common.Enums.ApprovalState.Accepted => nameof(Common.Enums.ApprovalState.Accepted),
        Common.Enums.ApprovalState.Rejected => nameof(Common.Enums.ApprovalState.Rejected),
        _ => "Not Submitted"
    };

    public Brush ApprovalStateBrush => CurrentProject.ApprovalState switch
    {
        Common.Enums.ApprovalState.AwaitingApproval => new SolidColorBrush(Color.FromRgb(255, 252, 79), 1),
        Common.Enums.ApprovalState.Accepted => new SolidColorBrush(Color.FromRgb(58, 156, 34), 1),
        Common.Enums.ApprovalState.Rejected => new SolidColorBrush(Color.FromRgb(200, 0, 0), 1),
        _ => new SolidColorBrush(Color.FromRgb(170, 170, 170), 1)
    };

    public ICommand ViewProjectCommand { get; }


    public ProjectManagementViewModel()
    {
        this.ViewProjectCommand = ReactiveCommand.Create<Project>(ViewProject);
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

