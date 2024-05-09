﻿using Avalonia.Controls;
using Avalonia.Threading;
using Common.Enums;
using DynamicData;
using Microsoft.EntityFrameworkCore.Storage;
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

public class DesignationManagementViewModel : ModuleViewModel
{
    private string errorText;
    private int skip = 0;
    private int take = 50;
    private readonly IDesignationService designationService;
    private readonly IDesignationRateService designationRateService;
    private readonly IUserService userService;
    private User currentUser;
    private string busyText = "Loading Designations";
    private bool isBusy = true;
    private bool isEditing = false;
    private bool isDeleting = false;
    private Designation? modifyingUser;
    private string searchTerm;
    private string primaryActionText = "Add";

    public DesignationManagementViewModel()
    {
        this.designationService = (IDesignationService)App.Container.GetService(typeof(IDesignationService));
        this.designationRateService = (IDesignationRateService)App.Container.GetService(typeof(IDesignationRateService));
        this.userService = (IUserService)App.Container.GetService(typeof(IUserService));
        this.AddUserCommand = ReactiveCommand.Create(this.AddUser);
        this.EditUserCommand = ReactiveCommand.Create<Designation>(this.EditUser);
        this.DeleteUserCommand = ReactiveCommand.Create<Designation>(this.DeleteUser);
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

    public ObservableCollection<Designation> Users { get; set; } = new ObservableCollection<Designation>();
    public ObservableCollection<DesignationRates> DesignationRates { get; set; } = new ObservableCollection<DesignationRates>();
    public string SearchTerm { get => this.searchTerm; set => this.RaiseAndSetIfChanged(ref this.searchTerm, value); }
    public string BusyText { get => this.busyText; set => this.RaiseAndSetIfChanged(ref this.busyText, value); }
    public string PrimaryActionText { get => this.primaryActionText; set => this.RaiseAndSetIfChanged(ref this.primaryActionText, value); }
    public string ErrorText { get => this.errorText; set => this.RaiseAndSetIfChanged(ref this.errorText, value); }
    public bool IsBusy { get => this.isBusy; set => this.RaiseAndSetIfChanged(ref this.isBusy, value); }

    public bool IsEditing { get => this.isEditing; set => this.RaiseAndSetIfChanged(ref this.isEditing, value); }
    public bool IsDeleting { get => this.isDeleting; set => this.RaiseAndSetIfChanged(ref this.isDeleting, value); }

    public Designation? ModifyingUser { get => this.modifyingUser; set => this.RaiseAndSetIfChanged(ref this.modifyingUser, value); }
    public User CurrentUser { get => this.currentUser; set => this.RaiseAndSetIfChanged(ref this.currentUser, value); }

    private void AddUser()
    {
        this.ModifyingUser = new();
        this.IsEditing = true;
        this.ErrorText = string.Empty;
        var userEdit = new CreateDesignationView();
        this.PrimaryActionText = "Add Designation";
        userEdit.DataContext = this;
        this.DesignationRates.Clear();
        this.DesignationRates.AddRange(this.ModifyingUser.DesignationRatesTemplate);
        SukiHost.ShowDialog(App.WorkspaceInstance, userEdit, allowBackgroundClose: false);
    }

    private void EditUser(Designation user)
    {
        this.ModifyingUser = user.Clone() as Designation;
        this.IsEditing = true;
        this.ErrorText = string.Empty;
        var userEdit = new CreateDesignationView();
        this.PrimaryActionText = "Edit Designation";
        userEdit.DataContext = this;
        var drs = this.designationRateService.GetAllRatesByDesignationId(this.ModifyingUser.Id ?? 0);
        this.DesignationRates.Clear();
        if (drs.Count > 0) 
        {
            this.DesignationRates.AddRange(drs);
        }
        else
        {
            this.DesignationRates.AddRange(this.ModifyingUser.DesignationRatesTemplate);
        }

        SukiHost.ShowDialog(App.WorkspaceInstance, userEdit, allowBackgroundClose: false);
    }

    private void DeleteUser(Designation user)
    {
        this.ModifyingUser = user.Clone() as Designation;
        this.IsDeleting = true;
        var deleteDialog = new ConfirmDesignationDeletion();
        deleteDialog.DataContext = this;
        SukiHost.ShowDialog(App.WorkspaceInstance, deleteDialog, allowBackgroundClose: true);
    }

    private void LoadUsers()
    {
        this.IsBusy = true;
        this.BusyText = "Loading User List";
        Task.Run(() =>
        {
            var result = this.designationService.GetAllDesignations();
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
        this.BusyText = "Adding new user";
        Task.Run(() =>
        {
            var valid = true;
            var tempText = "The following validation errors were encountered:\n";

            if (string.IsNullOrWhiteSpace(this.ModifyingUser.Name))
            {
                valid = false;
                tempText += "- Designation Name is required\n";
            }
            if(this.designationService.GetAllDesignations().Any(itm => itm.Name.ToLower().Trim() == this.ModifyingUser.Name.ToLower().Trim()))
            {
                valid = false;
                tempText += $"- There is already a designation named '{this.ModifyingUser.Name}'";
            }

            if (!valid)
            {
                this.IsBusy = false;
                ErrorText = tempText;
                return;
            }
            ErrorText = string.Empty;
            this.designationService.InsertDesignation(this.ModifyingUser!);
            this.IsEditing = false;

            var created = this.designationService.GetAllDesignations().FirstOrDefault(itm => itm.Name.ToLower().Trim() == this.ModifyingUser.Name.ToLower().Trim());

            if(created != null)
            {
                foreach(var rate in this.DesignationRates)
                {
                    rate.IsActive = true;
                    rate.DesignationID = created.Id ?? 0;
                    rate.Modified = DateTime.Now.ToUniversalTime();
                    if(rate.Id == null)
                    {
                        rate.Created = DateTime.Now.ToUniversalTime();
                        this.designationRateService.InsertDesignationRate(rate);
                    }
                    else
                    {
                        this.designationRateService.UpdateDesignationRate(rate);
                    }
                }
            }


            Dispatcher.UIThread.Invoke(() =>
            {
                if (App.WorkspaceInstance.DataContext is MainViewModel mvm)
                {
                    mvm.RequestsViewModel.ReloadRequestsCommand.Execute(Unit.Default);
                }
                var tb = new TextBlock();
                tb.Text = $"Designation {ModifyingUser.Name} was created";
                tb.Margin = new(5);
                CloseDialog();
                SukiHost.ShowToast(App.WorkspaceInstance, new("Successfully inserted designation", tb, TimeSpan.FromSeconds(5), null));
                this.LoadUsers();
            });
        });
    }

    private void RunUpdateTask()
    {
        this.IsBusy = true;
        this.BusyText = "Editing user";

        Task.Run(() =>
        {
            var valid = true;
            var tempText = "The following validation errors were encountered:\n";

            if (string.IsNullOrWhiteSpace(this.ModifyingUser.Name))
            {
                valid = false;
                tempText += "- Designation name is required\n";
            }

            if (!valid)
            {
                this.IsBusy = false;
                ErrorText = tempText;
                return;
            }
            ErrorText = string.Empty;
            this.designationService.UpdateDesignation(this.ModifyingUser);
            Thread.Sleep(1000);
            this.IsEditing = false;

            if (this.ModifyingUser != null)
            {
                foreach (var rate in this.DesignationRates)
                {
                    rate.IsActive = true;
                    rate.DesignationID = this.ModifyingUser.Id ?? 0;
                    rate.Modified = DateTime.Now.ToUniversalTime();
                    if (rate.Id == null)
                    {
                        rate.Created = DateTime.Now.ToUniversalTime();
                        this.designationRateService.InsertDesignationRate(rate);
                    }
                    else
                    {
                        this.designationRateService.UpdateDesignationRate(rate);
                    }
                }
            }

            Dispatcher.UIThread.Invoke(() =>
            {
                var tb = new TextBlock();
                tb.Text = $"User {ModifyingUser.Name} was modified";
                tb.Margin = new(5);

                CloseDialog();
                SukiHost.ShowToast(App.WorkspaceInstance, new("Successfully updated designation", tb, TimeSpan.FromSeconds(5), null));
                this.LoadUsers();
            });
        });
    }

    private void PerformDelete()
    {
        if (this.ModifyingUser != null)
        {
            Task.Run(() =>
            {
                if(this.userService.GetUsers().Any(itm => itm.DesignationID == this.ModifyingUser.Id))
                {
                    this.CloseDialog();
                    this.CreateToast("Cannot delete designation", $"{ModifyingUser.Name} has users assigned to it so it cannot be deleted.");
                    return;
                }

                Dispatcher.UIThread.Invoke(() =>
                {
                    CloseDialog();
                });

                this.IsBusy = true;
                this.BusyText = "Deleting user";
                Thread.Sleep(1000);
                this.designationService.DeleteDesignation(this.ModifyingUser);
                Thread.Sleep(1000);
                this.IsDeleting = false;
                Dispatcher.UIThread?.Invoke(() =>
                {
                    var tb = new TextBlock();
                    tb.Text = $"User {ModifyingUser.Name} was deleted";
                    tb.Margin = new(5);
                    SukiHost.ShowToast(App.WorkspaceInstance, new("Successfully deleted designation", tb, TimeSpan.FromSeconds(5), null));
                    this.LoadUsers();
                    this.UpdateUsers?.Invoke(this, EventArgs.Empty);
                });
            });
        }
    }

    public EventHandler UpdateUsers;

    private void CloseDialog()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            SukiHost.CloseDialog(App.WorkspaceInstance);
        });
    }
}