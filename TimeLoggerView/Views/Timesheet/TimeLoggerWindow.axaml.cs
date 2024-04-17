using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Common.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;
using TimeLoggerView.ViewModels;

namespace TimeLoggerView.Views.Timesheet
{
    public partial class TimeLoggerWindow : Window
    {
        public static TimeLoggerWindow? Instance { get; set; }
        
        public TimeLoggerWindow()
        {
            InitializeComponent();
            Instance = this;
            MainViewModel.TimesheetManagement.OnCompactModeChanged += OnCompactModeChanged;
            MainViewModel.TimesheetManagement.OnTriggerWindowClose += OnTriggerWindowClose;
            this.DataContext = MainViewModel.TimesheetManagement;
            if (MainViewModel.TimesheetManagement != null)
            {
                var vm = MainViewModel.TimesheetManagement;
                vm.ErrorText = string.Empty;
                vm.EndDateLocalTime = "00:00:00";
                vm.CurrentTimeLog = null;
                vm.CanRunTimeRecorder = true;
                vm.Comment = string.Empty;
                vm.StartDateTime = DateTime.Now.ToLocalTime();
                vm.EndDateTime = null;
                vm.Duration = null;
                vm.SelectedProject = null;
                vm.TeamType = TeamType.None;
                vm.DisciplineType = DisciplineType.None;
                vm.DrawingType = DrawingType.None;
                vm.ScopeType = ScopeType.None;
                vm.SelectedDeliverable = null;
                vm.AvailableDeliverables.Clear();
                vm.TimerString = "00:00:00";
            }
        }
        private bool _mouseDownForWindowMoving = false;
        private PointerPoint _originalPoint;

        private void InputElement_OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (!_mouseDownForWindowMoving) return;

            PointerPoint currentPoint = e.GetCurrentPoint(this);
            Position = new PixelPoint(Position.X + (int)(currentPoint.Position.X - _originalPoint.Position.X),
                Position.Y + (int)(currentPoint.Position.Y - _originalPoint.Position.Y));
        }

        private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (WindowState == WindowState.Maximized || WindowState == WindowState.FullScreen) return;

            _mouseDownForWindowMoving = true;
            _originalPoint = e.GetCurrentPoint(this);
        }

        private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            _mouseDownForWindowMoving = false;
        }

        private void Window_PointerEntered(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            if (!this.MainView.IsVisible)
            {
                this.SetValue(OpacityProperty, 1);
                return;
            }
        }

        private void Window_PointerExited_1(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            if (!this.MainView.IsVisible)
            {
                this.SetValue(OpacityProperty, 0.9);
            }
        }

        private void Window_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            MoveWindow();
        }

        private void Window_Closed(object? sender, System.EventArgs e)
        {
            Instance = null;
            MainViewModel.TimesheetManagement.OnCompactModeChanged -= OnCompactModeChanged;
            MainViewModel.TimesheetManagement.OnTriggerWindowClose -= OnTriggerWindowClose;

        }
        bool toggle = false;
        private void Button_Click_1(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            toggle = !toggle;
            SetDecorations(toggle);
        }

        private void OnTriggerWindowClose(object? sender, EventArgs e)
        {
            this.Close();
        }

        private void OnCompactModeChanged(object? sender, bool e)
        {
            SetDecorations(!e);
        }

        private void SetDecorations(bool enable = false)
        {
            this.SetValue(SizeToContentProperty, SizeToContent.Manual);
            this.Width = 10;
            this.Height = 10;
            if (enable)
            {
                this.SetValue(SystemDecorationsProperty, SystemDecorations.Full);
                this.SetValue(ExtendClientAreaChromeHintsProperty, Avalonia.Platform.ExtendClientAreaChromeHints.SystemChrome);
                this.SetValue(ExtendClientAreaToDecorationsHintProperty, false);
                this.SetValue(ExtendClientAreaTitleBarHeightHintProperty, 0);
                this.SetValue(CanResizeProperty, true);
                this.SetValue(MinWidthProperty, 500);
            }
            else
            {
                this.SetValue(MinWidthProperty, 70);
                this.SetValue(SystemDecorationsProperty, SystemDecorations.None);
                this.SetValue(ExtendClientAreaChromeHintsProperty, Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome);
                this.SetValue(ExtendClientAreaToDecorationsHintProperty, true);
                this.SetValue(ExtendClientAreaTitleBarHeightHintProperty, -1);
                this.SetValue(CanResizeProperty, false);
            }
            this.SetValue(SizeToContentProperty, SizeToContent.WidthAndHeight);
            Task.Run(() => {
                Thread.Sleep(100);
                MoveWindow();
            });
        }

        private void MoveWindow()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                Avalonia.Platform.Screen screen = this.Screens.Primary;
                PixelSize screenSize = screen.WorkingArea.Size;
                PixelSize windowSize = PixelSize.FromSize(this.ClientSize, screen.Scaling);

                this.Position = new PixelPoint(
                  screenSize.Width - windowSize.Width,
                  screenSize.Height - windowSize.Height);
            }
        }
    }
}
