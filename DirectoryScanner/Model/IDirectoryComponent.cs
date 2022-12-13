using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryScanner.Model
{
    public interface IDirectoryComponent
    {
        public string Name { get; set; }

        public long Size { get; set; }

        public double Percentage { get; set; }

        public string FullName { get; set; }

        public ObservableCollection<IDirectoryComponent> ChildNodes { get; set; }

        public ComponentType Type { get; set; } 
    }
}
