using ReactiveUI;
using System.Windows.Input;

namespace TimeLoggerView.ViewModels;

public class ReportsViewModel : ModuleViewModel
{
    public ICommand ReloadRequestsCommand { get; }

    public ReportsViewModel()
    {
        this.ReloadRequestsCommand = ReactiveCommand.Create(LoadReports);
        LoadReports();
    }

    private void LoadReports()
    {

    }
}
