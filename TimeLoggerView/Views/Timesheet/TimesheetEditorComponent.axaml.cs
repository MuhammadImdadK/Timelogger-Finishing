using Avalonia.Controls;

namespace TimeLoggerView.Views
{
    public partial class TimesheetEditorComponent : UserControl
    {
        public TimesheetEditorComponent()
        {
            InitializeComponent();
        }

        private void AutoCompleteBox_GotFocus(object? sender, Avalonia.Input.GotFocusEventArgs e)
        {
            this.DeliverableAuto.MinimumPrefixLength = 0;
            this.DeliverableAuto.IsDropDownOpen = true;

        }
    }
}
