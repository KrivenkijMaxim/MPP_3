using DirectoryScanner.Model;

namespace DirectoryScanner.Tests
{
    public class Tests
    {
        private CancellationTokenSource _cancelTokenSource;

        private Scanner _scanner;

        [SetUp]
        public void Setup()
        {
            _scanner = new Scanner(10);
            _cancelTokenSource = new CancellationTokenSource();
        }

        [Test]
        public void Test_RootSize()
        {
            string path = @"C:\\Users\\maksimbell\\bsuir";
            long size = 0;

            IDirectoryComponent root = _scanner.StartScanner(path, _cancelTokenSource.Token);
            foreach(var dir in root.ChildNodes.Where(child => child.Type == ComponentType.Directory))
            {
                size += dir.Size; 
            }

            Assert.That(size, Is.EqualTo(root.Size));
        }

        [Test]
        public void Test_SymLink()
        {
            string path = @"C:\\Users\\maksimbell\\bsuir\\5sem\\dir_test\\dir_test_2";

            IDirectoryComponent root = _scanner.StartScanner(path, _cancelTokenSource.Token);

            Assert.That(root.Size, Is.EqualTo(131578));
        }

        [Test]
        public void Test_WindowsFolderFileCount()
        {
            string path = @"C:\\Windows";

            IDirectoryComponent root = _scanner.StartScanner(path, _cancelTokenSource.Token);
            Assert.That(root.ChildNodes.Where(node => node.Type == ComponentType.File).Count, Is.EqualTo(42));
        }

        [Test]
        public void Test_Cancellation()
        {
            string path = @"C:\\Users\\maksimbell\\bsuir\\5sem\\dir_test\\";

            var task = Task<DirectoryComponent>.Factory.StartNew(() => 
                (DirectoryComponent)_scanner.StartScanner(path, _cancelTokenSource.Token));

            for(int i = 0; i < 15000; i++)
            {
                Console.WriteLine("_-_");
            }
            _cancelTokenSource.Cancel();

            var result = task.Result;
            Assert.That(result.Size, Is.LessThan(4124324));
        }
    }
}