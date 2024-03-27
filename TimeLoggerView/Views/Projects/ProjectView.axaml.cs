using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TimeLoggerView.Views
{
    public partial class ProjectView : UserControl
    {
        public static ProjectView Instance { get; private set; }

        public ProjectView()
        {
            InitializeComponent();
            Instance = this;
        }
    }
}