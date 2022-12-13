using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DirectoryScanner.Model;
using System.Diagnostics;

namespace DirectoryScanner.ViewModel
{
    public class DirectoryScannerViewModel: INotifyPropertyChanged // обязательно наследует
    {
        private CancellationTokenSource _cancelTokenSource;

        private Scanner _scanner;

        private IDirectoryComponent? _root;

        public IDirectoryComponent Root { 
            get { return _root; }
            set
            {
                _root = value;
                OnPropertyChanged("Root");  
            }
        }

        private void StartScanner()
        {
            var fbd = new FolderBrowserForWPF.Dialog();
            string path = String.Empty;
            if(!fbd.ShowDialog().GetValueOrDefault())
                return;

            _cancelTokenSource = new CancellationTokenSource(); 
            var token = _cancelTokenSource.Token;
            Trace.WriteLine(Thread.CurrentThread.ManagedThreadId);
            Trace.WriteLine("________________________________");

            Task.Run(() =>
            {
                Trace.WriteLine(Thread.CurrentThread.ManagedThreadId);
                Trace.WriteLine("________________________________");
                var root = new DirectoryComponent("", "", ComponentType.Directory); //чтобы была видна первая папка
                root.ChildNodes = new ObservableCollection<IDirectoryComponent> { _scanner.StartScanner(fbd.FileName, token) };
                Root = root;
            });

        }

        public event PropertyChangedEventHandler? PropertyChanged;      //---обработчик событий

        public void OnPropertyChanged([CallerMemberName] string prop = "") 
        {
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }                                                               //------

        private BaseCommand? _startScannerCommand;
        public BaseCommand StartScannerCommand
        {
            get { return _startScannerCommand ??= new BaseCommand(obj => StartScanner()); }
        }

        private BaseCommand? _cancelScannerCommand;
        public BaseCommand CancelScannerCommand
        {
            get { return _cancelScannerCommand ??= new BaseCommand(obj => CancelScanner()); }
        }

        public void CancelScanner()
        {
            if(_cancelTokenSource != null && !_cancelTokenSource.IsCancellationRequested)
            {
                _cancelTokenSource.Cancel();
                _cancelTokenSource.Dispose();
            }
        }

        public DirectoryScannerViewModel()
        {
            var threadCount = 3;
            _scanner = new Scanner(threadCount);
        }
    }
}
