using System;
using System.Windows.Input;
using ReactiveUI;
using SukiUI.Controls;

namespace TimeLoggerView.ViewModels
{
    public partial class RecursiveViewModel : ViewModelBase, ISukiStackPageTitleProvider
    {
        public string Title { get; }

        private readonly int _index;
        private readonly Action<RecursiveViewModel> _onRecurseClicked;
        public ICommand RecurseCommand { get; }


        public RecursiveViewModel(int index, Action<RecursiveViewModel> onRecurseClicked)
        {
            _index = index;
            _onRecurseClicked = onRecurseClicked;
            Title = $"Recursive {index}";
            RecurseCommand = ReactiveCommand.Create(Recurse);
        }

        public void Recurse() =>
            _onRecurseClicked.Invoke(new RecursiveViewModel(_index + 1, _onRecurseClicked));
    }
}