using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using SukiUI.Controls;
using System;
using TimeLoggerView.ViewModels;

namespace TimeLoggerView.Views;

public partial class MainWindow : SukiWindow
{
    public MainWindow()
    {
        InitializeComponent();

    }

    private void SukiWindow_Closed(object? sender, System.EventArgs e)
    {
       
        if(this.DataContext is MainViewModel vm && vm.ShouldQuit)
        {
            if (App.Current?.ApplicationLifetime is ClassicDesktopStyleApplicationLifetime lt)
            {
                lt.TryShutdown();

               
            }
        }
    }
}
