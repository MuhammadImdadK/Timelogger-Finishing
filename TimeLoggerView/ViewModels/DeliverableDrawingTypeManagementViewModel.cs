using Avalonia.Controls;
using Avalonia.Threading;
using Common.Enums;
using DynamicData;
using Model.ModelSql;
using Pec.ProjectManagement.Ui.Views.Designations;
using ReactiveUI;
using Service.Interface;
using Session;
using SukiUI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TimeLoggerView.ViewModels;

public class DeliverableDrawingTypeManagementViewModel : ModuleViewModel
{
    private string errorText;
    private int skip = 0;
    private int take = 50;
    private readonly IDeliverableDrawingTypeService designationService;
    private readonly IAttachmentService attachmentService;

    private readonly UserManagementViewModel? userModel;
    private User currentUser;
    private string busyText = "Loading Designations";
    private bool isBusy = true;
    private bool isEditing = false;
    private bool isDeleting = false;
    private DeliverableDrawingType? modifyingUser;
    private string searchTerm;
    private string primaryActionText = "Add";

    public DeliverableDrawingTypeManagementViewModel()
    {
        this.designationService = (IDeliverableDrawingTypeService)App.Container.GetService(typeof(IDeliverableDrawingTypeService));
        this.attachmentService = (IAttachmentService)App.Container.GetService(typeof(IAttachmentService));
        this.AddUserCommand = ReactiveCommand.Create(this.AddUser);
        this.EditUserCommand = ReactiveCommand.Create<DeliverableDrawingType>(this.EditUser);
        this.DeleteUserCommand = ReactiveCommand.Create<DeliverableDrawingType>(this.DeleteUser);
        this.PerformEditCommand = ReactiveCommand.Create(this.PerformEdit);
        this.PerformDeleteCommand = ReactiveCommand.Create(this.PerformDelete);
        this.CloseDialogCommand = ReactiveCommand.Create(this.CloseDialog);
        this.PerformSearchCommand = ReactiveCommand.Create(this.PerformSearch);
        this.CurrentUser = App.CurrentUser;
        this.LoadUsers();
    }

    public ICommand AddUserCommand { get; }
    public ICommand EditUserCommand { get; }
    public ICommand DeleteUserCommand { get; }

    public ICommand PerformEditCommand { get; }
    public ICommand PerformDeleteCommand { get; }

    public ICommand CloseDialogCommand { get; }
    public ICommand PerformSearchCommand { get; }

    public List<Role> AvailableRoles { get; } = Constants.Roles;
    public List<TeamType> AvailableTeams { get; } = new List<TeamType> { TeamType.None, TeamType.CoreTeam, TeamType.AdditionalTeam };


    public int Skip { get => this.skip; set => this.RaiseAndSetIfChanged(ref this.skip, value); }
    public int Take { get => this.take; set => this.RaiseAndSetIfChanged(ref this.take, value); }

    public ObservableCollection<DeliverableDrawingType> Users { get; set; } = new ObservableCollection<DeliverableDrawingType>();
    public string SearchTerm { get => this.searchTerm; set => this.RaiseAndSetIfChanged(ref this.searchTerm, value); }
    public string BusyText { get => this.busyText; set => this.RaiseAndSetIfChanged(ref this.busyText, value); }
    public string PrimaryActionText { get => this.primaryActionText; set => this.RaiseAndSetIfChanged(ref this.primaryActionText, value); }
    public string ErrorText { get => this.errorText; set => this.RaiseAndSetIfChanged(ref this.errorText, value); }
    public bool IsBusy { get => this.isBusy; set => this.RaiseAndSetIfChanged(ref this.isBusy, value); }

    public bool IsEditing { get => this.isEditing; set => this.RaiseAndSetIfChanged(ref this.isEditing, value); }
    public bool IsDeleting { get => this.isDeleting; set => this.RaiseAndSetIfChanged(ref this.isDeleting, value); }

    public DeliverableDrawingType? ModifyingUser { get => this.modifyingUser; set => this.RaiseAndSetIfChanged(ref this.modifyingUser, value); }
    public User CurrentUser { get => this.currentUser; set => this.RaiseAndSetIfChanged(ref this.currentUser, value); }

    private void AddUser()
    {
        this.ModifyingUser = new() { IsActive = true };
        this.IsEditing = true;
        this.ErrorText = string.Empty;
        var userEdit = new CreateDeliverableTypeView();
        this.PrimaryActionText = "Add Deliverable Type";
        userEdit.DataContext = this;
        SukiHost.ShowDialog(App.WorkspaceInstance, userEdit, allowBackgroundClose: false);
    }

    private void EditUser(DeliverableDrawingType user)
    {
        this.ModifyingUser = user.Clone() as DeliverableDrawingType;
        this.IsEditing = true;
        this.ErrorText = string.Empty;
        var userEdit = new CreateDeliverableTypeView();
        this.PrimaryActionText = "Edit Deliverable Type";
        userEdit.DataContext = this;

        SukiHost.ShowDialog(App.WorkspaceInstance, userEdit, allowBackgroundClose: false);
    }

    private void DeleteUser(DeliverableDrawingType user)
    {
        this.ModifyingUser = user.Clone() as DeliverableDrawingType;
        this.IsDeleting = true;
        var deleteDialog = new ConfirmDeliverableTypeDeletion();
        deleteDialog.DataContext = this;
        SukiHost.ShowDialog(App.WorkspaceInstance, deleteDialog, allowBackgroundClose: true);
    }

    private void LoadUsers()
    {
        this.IsBusy = true;
        this.BusyText = "Loading User List";
        Task.Run(() =>
        {
            var result = ((IDeliverableDrawingTypeService)App.Container.GetService(typeof(IDeliverableDrawingTypeService))).GetDeliverableDrawingTypes();
            lock (Users)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    Users.Clear();
                    Users.AddRange(result);
                });
            }
            Dispatcher.UIThread.Invoke(() => this.IsBusy = false);
        });
    }

    private void PerformEdit()
    {
        if (this.ModifyingUser != null && this.ModifyingUser?.Id == null)
        {
            RunInsertTask();
        }
        else if (this.ModifyingUser != null)
        {
            RunUpdateTask();
        }
    }

    private void PerformSearch()
    {
        //this.IsBusy = true;
        //this.BusyText = $"Searching for '{searchTerm}'";
        //Task.Run(() =>
        //{
        //    var results = this.designationService.SearchFor(this.SearchTerm);
        //    if (results.Count == 0)
        //    {
        //        Dispatcher.UIThread.Invoke(() =>
        //        {
        //            this.IsBusy = false;
        //            var tb = new TextBlock();
        //            tb.Text = $"No results were found for '{SearchTerm}' try refining your search.";
        //            tb.Margin = new(5);

        //            CloseDialog();
        //            SukiHost.ShowToast(App.WorkspaceInstance, new("No results", tb, TimeSpan.FromSeconds(5), null));
        //            this.LoadUsers();
        //        });
        //    }
        //    else
        //    {
        //        Dispatcher.UIThread.Invoke(() =>
        //        {
        //            this.IsBusy = false;

        //            Users.Clear();
        //            Users.AddRange(results);
        //        });
        //    }
        //});
    }

    private void RunInsertTask()
    {
        this.IsBusy = true;
        this.BusyText = "Adding new Deliverable Type";
        Task.Run(() =>
        {
            var valid = true;
            var tempText = "The following validation errors were encountered:\n";

            if (string.IsNullOrWhiteSpace(this.ModifyingUser.Name))
            {
                valid = false;
                tempText += "- Deliverable Type Name is required\n";
            }
            if (((IDeliverableDrawingTypeService)App.Container.GetService(typeof(IDeliverableDrawingTypeService))).GetDeliverableDrawingTypes().Any(itm => itm.Name.ToLower().Trim() == this.ModifyingUser.Name.ToLower().Trim()))
            {
                valid = false;
                tempText += $"- There is already an Deliverable Type named '{this.ModifyingUser.Name}'";
            }

            if (!valid)
            {
                this.IsBusy = false;
                ErrorText = tempText;
                return;
            }
            ErrorText = string.Empty;
            this.designationService.InsertDeliverableDrawingType(this.ModifyingUser!);
            this.IsEditing = false;

            Dispatcher.UIThread.Invoke(() =>
            {
                if (App.WorkspaceInstance.DataContext is MainViewModel mvm)
                {
                    mvm.RequestsViewModel.ReloadRequestsCommand.Execute(Unit.Default);
                }
                var tb = new TextBlock();
                tb.Text = $"Deliverable Type {ModifyingUser.Name} was created";
                tb.Margin = new(5);
                CloseDialog();
                SukiHost.ShowToast(App.WorkspaceInstance, new("Successfully inserted Deliverable Type", tb, TimeSpan.FromSeconds(5), null));
                this.LoadUsers();
                MainViewModel.Current.ProjectManagement.LoadProjects();
                MainViewModel.TimesheetManagement.LoadData();
                MainViewModel.Current.TimesheetModel.LoadData();
            });
        });
    }

    private void RunUpdateTask()
    {
        this.IsBusy = true;
        this.BusyText = "Editing Deliverable Type";

        Task.Run(() =>
        {
            var valid = true;
            var tempText = "The following validation errors were encountered:\n";

            if (string.IsNullOrWhiteSpace(this.ModifyingUser.Name))
            {
                valid = false;
                tempText += "- Deliverable Type Name is required\n";
            }

            if (!valid)
            {
                this.IsBusy = false;
                ErrorText = tempText;
                return;
            }
            ErrorText = string.Empty;
            this.designationService.UpdateDeliverableDrawingType(this.ModifyingUser);
            Thread.Sleep(1000);
            this.IsEditing = false;

            Dispatcher.UIThread.Invoke(() =>
            {
                var tb = new TextBlock();
                tb.Text = $"Deliverable Type {ModifyingUser.Name} was modified";
                tb.Margin = new(5);

                CloseDialog();
                SukiHost.ShowToast(App.WorkspaceInstance, new("Successfully updated Deliverable Type", tb, TimeSpan.FromSeconds(5), null));
                this.LoadUsers();
                MainViewModel.Current.ProjectManagement.LoadProjects();
                MainViewModel.TimesheetManagement.LoadData();
                MainViewModel.Current.TimesheetModel.LoadData();
            });
        });
    }

    private void PerformDelete()
    {
        if (this.ModifyingUser != null)
        {
            Task.Run(() =>
            {
                if (((ITimeLogService)App.Container.GetService(typeof(ITimeLogService))).GetTimeLogs().Any(itm => itm.DeliverableDrawingTypeID == this.ModifyingUser.Id))
                {
                    this.CloseDialog();
                    this.CreateToast("Cannot delete Deliverable Type", $"{ModifyingUser.Name} has been assigned to a timelog, so it so it cannot be deleted.");
                    return;
                }

                Dispatcher.UIThread.Invoke(() =>
                {
                    CloseDialog();
                });

                this.IsBusy = true;
                this.BusyText = "Deleting Deliverable Type";
                Thread.Sleep(1000);
                this.designationService.DeleteDeliverableDrawingType(this.ModifyingUser);
                Thread.Sleep(1000);
                this.IsDeleting = false;
                Dispatcher.UIThread?.Invoke(() =>
                {
                    var tb = new TextBlock();
                    tb.Text = $"Deliverable Type {ModifyingUser.Name} was deleted";
                    tb.Margin = new(5);
                    SukiHost.ShowToast(App.WorkspaceInstance, new("Successfully deleted Deliverable Type", tb, TimeSpan.FromSeconds(5), null));
                    this.LoadUsers();
                    this.userModel!.LoadUsers();
                    MainViewModel.Current.ProjectManagement.LoadProjects();
                    MainViewModel.TimesheetManagement.LoadData();
                    MainViewModel.Current.TimesheetModel.LoadData();
                });
            });
        }
    }

    private void CloseDialog()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            SukiHost.CloseDialog(App.WorkspaceInstance);
        });
    }
}