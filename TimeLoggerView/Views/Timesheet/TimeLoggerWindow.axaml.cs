using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;

namespace TimeLoggerView.Views.Timesheet
{
    public partial class TimeLoggerWindow : Window
    {
        public static TimeLoggerWindow? Instance { get; set; }
        public TimeLoggerWindow()
        {
            InitializeComponent();
            Instance = this;
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
                this.SetValue(OpacityProperty, 0.7);
            }
        }

        private void Window_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
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

        private void Window_Closed(object? sender, System.EventArgs e)
        {
            Instance = null;
        }
    }
}
