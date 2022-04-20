using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;

namespace WpfApp4
{
    public class LinePoint : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private Point _point;
        private List<MyLine> _accessLine;
        private MyLine _parent;
        private bool _frag;

        public LinePoint(double x, double y, MyLine parent, bool flag)
        {
            _point = new Point(x, y);
            _accessLine = new List<MyLine>();
            _parent = parent;
            _frag = flag;
        }

        public Point Point
        {
            get { return _point; }
            set
            {
                if (_point == value)
                    return;
                _point = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Point"));
            }
        }

        public List<MyLine> AccessLine
        {
            get { return _accessLine; }
            set
            {
                if (_accessLine == value)
                    return;
                _accessLine = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AccessLine"));
            }
        }

        public MyLine Parent
        {
            get { return _parent; }
            set
            {
                if (_parent == value)
                    return;
                _parent = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Parent"));
            }
        }

        public bool Flag
        {
            get { return _frag; }
            set
            {
                if (_frag == value)
                    return;
                _frag = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Flag"));
            }
        }
    }
}
