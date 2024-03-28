using Avalonia.Controls;
using TimeLoggerView.ViewModels;

namespace TimeLoggerView.Views
{
    public partial class TimesheetMainView : UserControl
    {
        public TimesheetMainView()
        {
            InitializeComponent();
            this.DataContext = MainViewModel.TimesheetManagement;
        }
    }
}
