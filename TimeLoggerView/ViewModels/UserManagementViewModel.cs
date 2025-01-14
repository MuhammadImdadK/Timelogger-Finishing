﻿using Avalonia.Controls;
using Avalonia.Threading;
using Common.Enums;
using DynamicData;
using Model.ModelSql;
using ReactiveUI;
using Service.Interface;
using Service.Service;
using Session;
using SukiUI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TimeLoggerView.Views;

namespace TimeLoggerView.ViewModels;

public class UserManagementViewModel : ViewModelBase
{
    private string errorText;
    private int skip = 0;
    private int take = 50;
    private readonly IUserService userService;
    private User currentUser;
    private string busyText = "Loading Users";
    private bool isBusy = true;
    private bool isEditing = false;
    private bool isDeleting = false;
    private User? modifyingUser;
    private string searchTerm;
    private string primaryActionText = "Add";

    public UserManagementViewModel()
    {
        this.userService = (IUserService)App.Container.GetService(typeof(IUserService));
        this.AddUserCommand = ReactiveCommand.Create(this.AddUser);
        this.EditUserCommand = ReactiveCommand.Create<User>(this.EditUser);
        this.DeleteUserCommand = ReactiveCommand.Create<User>(this.DeleteUser);
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

    public ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();
    public string SearchTerm { get => this.searchTerm; set => this.RaiseAndSetIfChanged(ref this.searchTerm, value); }
    public string BusyText { get => this.busyText; set => this.RaiseAndSetIfChanged(ref this.busyText, value); }
    public string PrimaryActionText { get => this.primaryActionText; set => this.RaiseAndSetIfChanged(ref this.primaryActionText, value); }
    public string ErrorText { get => this.errorText; set => this.RaiseAndSetIfChanged(ref this.errorText, value); }
    public bool IsBusy { get => this.isBusy; set => this.RaiseAndSetIfChanged(ref this.isBusy, value); }

    public bool IsEditing { get => this.isEditing; set => this.RaiseAndSetIfChanged(ref this.isEditing, value); }
    public bool IsDeleting { get => this.isDeleting; set => this.RaiseAndSetIfChanged(ref this.isDeleting, value); }

    public User? ModifyingUser { get => this.modifyingUser; set => this.RaiseAndSetIfChanged(ref this.modifyingUser, value); }
    public User CurrentUser { get => this.currentUser; set => this.RaiseAndSetIfChanged(ref this.currentUser, value); }

    private void AddUser()
    {
        this.ModifyingUser = new();
        this.IsEditing = true;
        this.ErrorText = string.Empty;
        var userEdit = new UserEditorView();
        this.PrimaryActionText = "Add User";
        this.ModifyingUser.Role = this.AvailableRoles.FirstOrDefault(itm => itm.Id == ModifyingUser.RoleID);
        userEdit.DataContext = this;
        SukiHost.ShowDialog(App.WorkspaceInstance, userEdit, allowBackgroundClose: false);
    }

    private void EditUser(User user)
    {
        this.ModifyingUser = user.Clone() as User;
        this.IsEditing = true;
        this.ErrorText = string.Empty;
        var userEdit = new UserEditorView();
        this.PrimaryActionText = "Edit User";
        this.ModifyingUser.Role = this.AvailableRoles.FirstOrDefault(itm => itm.Id == ModifyingUser.RoleID);
        userEdit.DataContext = this;
        SukiHost.ShowDialog(App.WorkspaceInstance, userEdit, allowBackgroundClose: false);
    }

    private void DeleteUser(User user)
    {
        this.ModifyingUser = user.Clone() as User;
        this.IsDeleting = true;
        var deleteDialog = new ConfirmUserDeletionView();
        deleteDialog.DataContext = this;
        SukiHost.ShowDialog(App.WorkspaceInstance, deleteDialog, allowBackgroundClose: true);
    }

    private void LoadUsers()
    {
        this.IsBusy = true;
        this.BusyText = "Loading User List";
        Task.Run(() =>
        {
            var result = this.userService.GetUsers(Skip, Take);
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
        this.IsBusy = true;
        this.BusyText = $"Searching for '{searchTerm}'";
        Task.Run(() =>
        {
            var results = this.userService.SearchFor(this.SearchTerm);
            if(results.Count == 0)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    this.IsBusy = false;
                    var tb = new TextBlock();
                    tb.Text = $"No results were found for '{SearchTerm}' try refining your search.";
                    tb.Margin = new(5);

                    CloseDialog();
                    SukiHost.ShowToast(App.WorkspaceInstance, new("No results", tb, TimeSpan.FromSeconds(5), null));
                    this.LoadUsers();
                });
            }
            else
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    this.IsBusy = false;

                    Users.Clear();
                    Users.AddRange(results);
                });
            }
        });
    }

    private void RunInsertTask()
    {
        this.IsBusy = true;
        this.BusyText = "Adding new user";
        Task.Run(() =>
        {
            var valid = true;
            var tempText = "The following validation errors were encountered:\n";
            if (this.ModifyingUser.Role == null)
            {
                this.ModifyingUser.RoleID = Constants.Roles.LastOrDefault().Id;
            }
            else
            {
                this.ModifyingUser.RoleID = this.ModifyingUser.Role.Id;
            }
            
            if(string.IsNullOrWhiteSpace(this.ModifyingUser.EmployeeNumber))
            {
                valid = false;
                tempText += "- Employee number is required\n";
            }
            if (string.IsNullOrWhiteSpace(this.ModifyingUser.Username))
            {
                valid = false;
                tempText += "- Username is required\n";
            }
            if (this.ModifyingUser.Id == null && string.IsNullOrEmpty(this.ModifyingUser.NewPassword))
            {
                valid = false;
                tempText += "- Password is required\n";
            }
            if (string.IsNullOrWhiteSpace(this.ModifyingUser.FirstName))
            {
                valid = false;
                tempText += "- First Name is required\n";
            }
            if (string.IsNullOrWhiteSpace(this.ModifyingUser.Email)){
                valid = false;
                tempText += "- Email is required\n";
            }


            if (!valid)
            {
                this.IsBusy = false;
                ErrorText = tempText;
                return;
            }
            ErrorText = string.Empty;
            if (!string.IsNullOrEmpty(this.ModifyingUser.NewPassword))
            {
                var authMan = new AuthenticationManager();
                var salt = authMan.GetSalt();
                var hash = authMan.GenerateHash(Encoding.UTF8.GetBytes(this.ModifyingUser.NewPassword), salt);
                this.ModifyingUser.Password = hash;
                this.ModifyingUser.Salt = salt;
            }

            this.ModifyingUser.Role = null;
            this.ModifyingUser.CreatedBy = this.currentUser.Id;
            this.ModifyingUser.Created = DateTime.UtcNow;
            this.ModifyingUser.Modified = DateTime.UtcNow;
            this.ModifyingUser.ModifiedBy = this.currentUser.Id;
            this.userService.AddUser(this.ModifyingUser!);
            this.IsEditing = false;

            Dispatcher.UIThread.Invoke(() =>
            {
                var tb = new TextBlock();
                tb.Text = $"User {ModifyingUser.Username} was created";
                tb.Margin = new(5);
                CloseDialog();
                SukiHost.ShowToast(App.WorkspaceInstance, new("Successfully inserted user", tb, TimeSpan.FromSeconds(5), null));
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
            if (this.ModifyingUser.Role == null)
            {
                this.ModifyingUser.RoleID = Constants.Roles.LastOrDefault().Id;
            }
            else
            {
                this.ModifyingUser.RoleID = this.ModifyingUser.Role.Id;
            }

            if (string.IsNullOrWhiteSpace(this.ModifyingUser.EmployeeNumber))
            {
                valid = false;
                tempText += "- Employee number is required\n";
            }
            if (string.IsNullOrWhiteSpace(this.ModifyingUser.Username))
            {
                valid = false;
                tempText += "- Username is required\n";
            }
            if (string.IsNullOrWhiteSpace(this.ModifyingUser.FirstName))
            {
                valid = false;
                tempText += "- First Name is required\n";
            }
            if (string.IsNullOrWhiteSpace(this.ModifyingUser.Email))
            {
                valid = false;
                tempText += "- Email is required\n";
            }

            if (!valid)
            {
                this.IsBusy = false;
                ErrorText = tempText;
                return;
            }
            ErrorText = string.Empty;

            if (!string.IsNullOrEmpty(this.ModifyingUser.NewPassword))
            {
                var authMan = new AuthenticationManager();
                var salt = authMan.GetSalt();
                var hash = authMan.GenerateHash(Encoding.UTF8.GetBytes(this.ModifyingUser.NewPassword), salt);
                this.ModifyingUser.Password = hash;
                this.ModifyingUser.Salt = salt;
            }
            
            this.ModifyingUser.Modified = DateTime.UtcNow;
            this.ModifyingUser.ModifiedBy = this.currentUser.Id;
            this.ModifyingUser.Role = null;

            Thread.Sleep(1000);
            this.userService.EditUser(this.ModifyingUser);
            Thread.Sleep(1000);
            this.IsEditing = false;

            Dispatcher.UIThread.Invoke(() =>
            {
                var tb = new TextBlock();
                tb.Text = $"User {ModifyingUser.Username} was modified";
                tb.Margin = new(5);

                CloseDialog();
                SukiHost.ShowToast(App.WorkspaceInstance, new("Successfully updated user", tb, TimeSpan.FromSeconds(5), null));
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
                Dispatcher.UIThread.Invoke(() =>
                {
                    CloseDialog();
                });

                this.IsBusy = true;
                this.BusyText = "Deleting user";
                this.ModifyingUser.Role = null;
                Thread.Sleep(1000);
                this.userService.DeleteUser(this.ModifyingUser);
                Thread.Sleep(1000);
                this.IsDeleting = false;
                Dispatcher.UIThread?.Invoke(() =>
                {
                    var tb = new TextBlock();
                    tb.Text = $"User {ModifyingUser.Username} was deleted";
                    tb.Margin = new(5);
                    SukiHost.ShowToast(App.WorkspaceInstance, new("Successfully updated user", tb, TimeSpan.FromSeconds(5), null));
                    this.LoadUsers();
                });
            });
        }
    }

    private void CloseDialog()
    {
        SukiHost.CloseDialog(App.WorkspaceInstance);
    }
}