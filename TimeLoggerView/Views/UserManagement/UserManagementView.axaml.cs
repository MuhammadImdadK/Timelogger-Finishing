using Avalonia.Controls;
using Model.ModelSql;
using TimeLoggerView.ViewModels;

namespace TimeLoggerView.Views
{
    public partial class UserManagementView : UserControl
    {
        public UserManagementView()
        {
            InitializeComponent();
        }

        private void Edit_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if(UserTable.SelectedItem == null)
            {
                if(sender is Button btn)
                {
                    (btn.Parent as Control).Focus();
                }
            }

            if(this.DataContext is UserManagementViewModel vm && UserTable.SelectedItem is User user)
            {
                vm.EditUserCommand.Execute(user);
            }
        }

        private void Delete_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (UserTable.SelectedItem == null)
            {
                if (sender is Button btn)
                {
                    (btn.Parent as Control).Focus();
                }
            }

            if (this.DataContext is UserManagementViewModel vm && UserTable.SelectedItem is User user)
            {
                vm.DeleteUserCommand.Execute(user);
            }
        }

        private void Image_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if(sender is Image img && img.Parent is Button btn)
            {
                Edit_Click(btn, e);
            }
        }
    }
}
