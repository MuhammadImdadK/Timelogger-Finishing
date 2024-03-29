using Avalonia.Controls;
using Avalonia.Threading;
using ReactiveUI;
using SukiUI.Controls;
using System;

namespace TimeLoggerView.ViewModels;

public class ModuleViewModel : ViewModelBase
{
    private bool isBusy;
    private string busyText = string.Empty;
    private string errorText = string.Empty;

    public bool IsBusy { get => this.isBusy; set => this.RaiseAndSetIfChanged(ref this.isBusy, value); }
    public string BusyText { get => this.busyText; set => this.RaiseAndSetIfChanged(ref this.busyText, value); }
    public string ErrorText { get => this.errorText; set => this.RaiseAndSetIfChanged(ref this.errorText, value); }

    protected void CloseDialog()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            SukiHost.CloseDialog(App.WorkspaceInstance);
        });
    }

    protected void CreateToast(string title, string message)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var tb = new TextBlock();
            tb.Text = message;
            tb.Margin = new(5);
            SukiHost.ShowToast(App.WorkspaceInstance, new(title, tb, TimeSpan.FromSeconds(5), null));
        });
    }
}

