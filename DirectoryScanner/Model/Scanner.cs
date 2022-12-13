using System.Collections.Concurrent;
using System.Diagnostics;

namespace DirectoryScanner.Model
{
    public class Scanner
    {
        private string _path; //путь к директории

        private DirectoryComponent _root; //первая папка

        public DirectoryComponent Root
        {
            get { return _root; }
            set { _root = value; }
        }

        private ConcurrentQueue<Task> _folderQueue;
        private CancellationToken _cancellationToken;

        private int _threadCount;
        private SemaphoreSlim _semaphore;


        public Scanner(int threadCount)
        {
            _threadCount = threadCount;
            _folderQueue = new ConcurrentQueue<Task>();
            _semaphore = new SemaphoreSlim(_threadCount, _threadCount); //слим дает инфу сколько потоков доступно к созданию
        }

        public IDirectoryComponent StartScanner(string path, CancellationToken token)
        {
            _cancellationToken = token;
            _path = path;
            _root = new DirectoryComponent(new DirectoryInfo(_path).Name, _path, ComponentType.Directory, 0, 100);

            _semaphore.Wait(_cancellationToken);
            _folderQueue.Enqueue(Task.Run(() => ScanDirectory(_root), _cancellationToken));

            try
            {
                while(_folderQueue.TryDequeue(out var task) && !_cancellationToken.IsCancellationRequested) // пока есть что забирать из очереди и нет прерывания
                {
                    if(task.Status.Equals(TaskStatus.Created) && !task.IsCompleted) // если таска создана и не закончена то раним как такс.ран
                        task.Start();
                    task.Wait(_cancellationToken);// ждем выполнения таски
                }
            }
            catch(OperationCanceledException e) // если было кинут кэнцел
            {
                _folderQueue.Clear(); //  чистим очередь
            }
            Trace.WriteLine(_semaphore.CurrentCount);

            _root.Size = CountSize(_root);
            CountRelativeSize(_root);

            return Root;
        }

        private void ScanDirectory(DirectoryComponent dir)
        {
            Trace.WriteLine(_semaphore.CurrentCount);
            //
            var dirInfo = new DirectoryInfo(dir.FullName);

            try
            {
                foreach(var dirPath in dirInfo.EnumerateDirectories().Where(dir => dir.LinkTarget == null))// ицем вложенные директории
                {
                    if(_cancellationToken.IsCancellationRequested) 
                        return;
                    var child = new DirectoryComponent(dirPath.Name, dirPath.FullName, ComponentType.Directory);
                    dir.ChildNodes.Add(child);
                    if(_semaphore.CurrentCount != 0)// может создать поток
                    {
                        _folderQueue.Enqueue(Task.Run(() => //сразу запускаем тк есть потоки к созданию
                        {
                            _semaphore.Wait();
                            ScanDirectory(child);
                        }, _cancellationToken));
                    }
                    else
                    {
                        _folderQueue.Enqueue(new Task(() => // кидаем просто в очередь тк нет свободного места в семафоре для потоков
                        {//нет доступа
                            _semaphore.Wait();
                            //есть доступа
                            ScanDirectory(child);
                        }, _cancellationToken));
                    }
                }

                foreach(var dirPath in dirInfo.EnumerateFiles().Where(file => file.LinkTarget == null))
                {
                    if(_cancellationToken.IsCancellationRequested) 
                        return;
                    dir.ChildNodes.Add(new DirectoryComponent(dirPath.Name, dirPath.FullName, ComponentType.File, dirPath.Length));
                    dir.Size += dirPath.Length;
                }
            }
            catch(Exception e)
            {

            }

            //Trace.WriteLine(_semaphore.CurrentCount);
            _semaphore.Release();
        }

        private long CountSize(IDirectoryComponent parentNode)
        {
            long size = 0;

            foreach(var childNode in parentNode.ChildNodes.ToList())
            {
                if(childNode.Type == ComponentType.Directory)
                {
                    var childDirSize = CountSize(childNode);
                    size += childDirSize;
                    childNode.Size = childDirSize;
                }
                else
                {
                    size += childNode.Size;
                }
            }

            return size;
        }

        private void CountRelativeSize(IDirectoryComponent parentNode)
        {
            foreach(var childNode in parentNode.ChildNodes.ToList())
            {
                childNode.Percentage = childNode.Percentage == 0 ?
                    (double)childNode.Size / (double)parentNode.Size * 100 : childNode.Percentage;

                if(childNode.Type == ComponentType.Directory)
                {
                    CountRelativeSize(childNode);
                }
            }
        }
    }
}
