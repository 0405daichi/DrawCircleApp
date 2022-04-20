using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace WpfApp4
{
    public class LineList
    {
        private double _index = 1;
        public ObservableCollection<MyLine> List { get; }

        public LineList()
        {
            List = new ObservableCollection<MyLine>();
        }
        public ICommand Command => new RelayCommand(AddList);
        private void AddList(object param)
        {
            List.Add(new MyLine() { CircleName = $"Circle{_index}"});
            _index++;
        }
    }
}
