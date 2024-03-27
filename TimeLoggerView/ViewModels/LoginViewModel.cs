using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using Model.ModelSql;
using ReactiveUI;
using Service.Interface;
using Session;

namespace TimeLoggerView.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private bool isLoggingIn;
        private bool isSuccess = true;

        private string username;
        private string password;
        private string errorText;

        public LoginViewModel(IAuthenticationService authenticationService)
        {
            LoginCommand = ReactiveCommand.Create(Login);
            AuthenticationService = authenticationService;
        }

        public EventHandler<User> OnLoginSuccessful;

        public ICommand LoginCommand { get; }
        public IAuthenticationService AuthenticationService { get; }

        public string Username { get => this.username; set => this.RaiseAndSetIfChanged(ref username, value); }
        public string Password { get => this.password; set => this.RaiseAndSetIfChanged(ref password, value); }
        public string ErrorText { get => this.errorText; set => this.RaiseAndSetIfChanged(ref errorText, value); }

        public bool IsLoggingIn { get => this.isLoggingIn; set => this.RaiseAndSetIfChanged(ref isLoggingIn, value); }
        public bool IsSuccess { get => this.isSuccess; set => this.RaiseAndSetIfChanged(ref isSuccess, value); }


        public void Login()
        {
            this.IsLoggingIn = true;
            this.IsSuccess = true;
            this.ErrorText = string.Empty;
            Task.Run(() =>
            {
                var response = AuthenticationService.Login(Username, Password);

                if (response.Status == Common.Enums.ResponseStatus.Success)
                {
                    if (response.Data is User user)
                    {
                        this.IsLoggingIn = false;

                        if (user.IsActive)
                        {
                            this.IsSuccess = true;
                            this.Username = string.Empty;
                            this.Password = string.Empty;
                            user.Role = Constants.Roles.FirstOrDefault(itm => itm.Id == user.RoleID) ?? new Role { Id = user.RoleID, Name = "Unknown" };

                            Dispatcher.UIThread.Invoke(() => this.OnLoginSuccessful?.Invoke(this, user));
                        } 
                        else
                        {
                            this.IsSuccess = false;
                            this.ErrorText = "Unauthorized\nYour account is inactive";
                        }
                    }
                }
                else
                {
                    this.IsLoggingIn = false;
                    this.IsSuccess = false;
                    this.ErrorText = response.Message;
                }
            });
        }
    }
}